using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.API.Controllers;

public sealed record ReceiptLineDto(string MedicineName,string MedicineCode,string BatchNumber,int Quantity,decimal UnitPrice,decimal LineTotal);
public sealed record ReceiptDto(string Kind,int Id,string InvoiceNumber,DateTime DateUtc,string PartyName,
    decimal Subtotal,decimal Discount,decimal GrandTotal,IReadOnlyList<ReceiptLineDto> Items);

[ApiController]
[Route("api/receipts")]
[Authorize]
public sealed class ReceiptsController(ApplicationDbContext db,IAuthorizationService authorization):ControllerBase
{
    [HttpGet("{kind}/{id:int}")]
    public async Task<IActionResult> Get(string kind,int id,CancellationToken ct)
    {
        if(kind.Equals("sales",StringComparison.OrdinalIgnoreCase)){
            if(!(await authorization.AuthorizeAsync(User,"Sales.Read")).Succeeded)return Forbid();
            var sale=await db.Sales.AsNoTracking().Where(x=>x.Id==id)
                .Select(x=>new ReceiptDto("Sale",x.Id,x.InvoiceNumber,x.SoldAtUtc,
                    x.Customer!=null?x.Customer.Name:"Walk-in customer",
                    x.Lines.Sum(l=>l.LineTotal),x.Discount,x.GrandTotal,
                    x.Lines.OrderBy(l=>l.Id).Select(l=>new ReceiptLineDto(l.StockBatch.Medicine.Name,
                        l.StockBatch.Medicine.Code,l.StockBatch.BatchNumber,l.Quantity,l.UnitPrice,l.LineTotal)).ToList()))
                .SingleOrDefaultAsync(ct);
            return sale is null?NotFound():Ok(sale);
        }
        if(kind.Equals("purchases",StringComparison.OrdinalIgnoreCase)){
            if(!(await authorization.AuthorizeAsync(User,"Purchases.Read")).Succeeded)return Forbid();
            var purchase=await db.Purchases.AsNoTracking().Where(x=>x.Id==id)
                .Select(x=>new ReceiptDto("Purchase",x.Id,x.InvoiceNumber,x.PurchasedAtUtc,x.Supplier.Name,
                    x.Lines.Sum(l=>l.LineTotal),x.Discount,x.GrandTotal,
                    x.Lines.OrderBy(l=>l.Id).Select(l=>new ReceiptLineDto(l.StockBatch.Medicine.Name,
                        l.StockBatch.Medicine.Code,l.StockBatch.BatchNumber,l.Quantity,l.UnitPrice,l.LineTotal)).ToList()))
                .SingleOrDefaultAsync(ct);
            return purchase is null?NotFound():Ok(purchase);
        }
        return NotFound();
    }
}
