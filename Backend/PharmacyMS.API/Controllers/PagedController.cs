using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Domain.Entities;
using PharmacyMS.Infrastructure.Data;

namespace PharmacyMS.API.Controllers;

public sealed record PageResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

/// <summary>True SQL-level paging: Count/OrderBy/Skip/Take execute on SQL Server, not in Angular.</summary>
[ApiController]
[Route("api/catalog-pages")]
[Authorize]
public sealed class CatalogPagesController(ApplicationDbContext db, IAuthorizationService authorization) : ControllerBase
{
    [HttpGet("{key}")]
    public async Task<IActionResult> List(string key, [FromQuery]string? search, [FromQuery]string? sortBy,
        [FromQuery]bool descending=false, [FromQuery]int page=1, [FromQuery]int pageSize=20, CancellationToken ct=default)
    {
        if (page<1 || pageSize<1 || pageSize>100) return BadRequest(new {message="Page must be positive; pageSize must be 1-100."});
        var permission=key.ToLowerInvariant() switch {
            "medicines"=>"Medicines.Read", "categories"=>"Categories.Read", "generics"=>"Generics.Read",
            "shelves"=>"Shelves.Read", "suppliers"=>"Suppliers.Read", "customers"=>"Customers.Read", _=>null
        };
        if (permission is null) return NotFound();
        if (!(await authorization.AuthorizeAsync(User, permission)).Succeeded) return Forbid();
        return key.ToLowerInvariant() switch {
            "medicines"=>Ok(await GetPage(db.Medicines.AsNoTracking(),x=>new MedicineDto(x.Id,x.Code,x.Name,x.Strength,
                x.CategoryId,x.Category!=null?x.Category.Name:null,x.GenericId,x.Generic!=null?x.Generic.Name:null,
                x.ShelfId,x.Shelf!=null?x.Shelf.Name:null,x.SupplierId,x.Supplier!=null?x.Supplier.Name:null,
                x.BuyingPrice,x.SellingPrice,x.IsActive,x.MinimumStockLevel),search,sortBy,descending,page,pageSize,ct)),
            "categories"=>Ok(await GetPage(db.Categories.AsNoTracking(),x=>new CategoryDto(x.Id,x.Name,x.Description),search,sortBy,descending,page,pageSize,ct)),
            "generics"=>Ok(await GetPage(db.Generics.AsNoTracking(),x=>new GenericDto(x.Id,x.Code,x.Name),search,sortBy,descending,page,pageSize,ct)),
            "shelves"=>Ok(await GetPage(db.Shelves.AsNoTracking(),x=>new ShelfDto(x.Id,x.Name,x.Number),search,sortBy,descending,page,pageSize,ct)),
            "suppliers"=>Ok(await GetPage(db.Suppliers.AsNoTracking(),x=>new SupplierDto(x.Id,x.Code,x.Name,x.ShortName,x.Email,x.Phone,x.Address,x.Notes),search,sortBy,descending,page,pageSize,ct)),
            "customers"=>Ok(await GetPage(db.Customers.AsNoTracking(),x=>new CustomerDto(x.Id,x.Code,x.Name,x.Email,x.Phone,x.Address,x.Notes),search,sortBy,descending,page,pageSize,ct)),
            _=>NotFound()
        };
    }
    private static async Task<PageResult<TDto>> GetPage<TEntity,TDto>(IQueryable<TEntity> query,
        Expression<Func<TEntity,TDto>> projection,string? search,string? sortBy,bool descending,int page,int pageSize,CancellationToken ct)
        where TEntity:EntityBase
    {
        if(!string.IsNullOrWhiteSpace(search)) {
            var term=search.Trim();
            var hasCode=typeof(TEntity).GetProperty("Code") is not null;
            var hasNumber=typeof(TEntity).GetProperty("Number") is not null;
            if(hasCode) query=query.Where(x=>EF.Property<string>(x,"Name").Contains(term)||EF.Property<string>(x,"Code").Contains(term));
            else if(hasNumber) query=query.Where(x=>EF.Property<string>(x,"Name").Contains(term)||EF.Property<string>(x,"Number").Contains(term));
            else query=query.Where(x=>EF.Property<string>(x,"Name").Contains(term));
        }
        // Only real mapped CLR properties may be used as sort columns; user input is never SQL text.
        var candidate=typeof(TEntity).GetProperties().FirstOrDefault(p=>p.Name.Equals(sortBy,StringComparison.OrdinalIgnoreCase));
        var supported=new[]{"Id","Name","Code","Number","BuyingPrice","SellingPrice","MinimumStockLevel","CreatedAtUtc"};
        var field=candidate is not null && supported.Contains(candidate.Name) ? candidate.Name : "Name";
        var count=await query.CountAsync(ct);
        var sorted=descending?query.OrderByDescending(x=>EF.Property<object>(x,field)).ThenBy(x=>x.Id)
                             :query.OrderBy(x=>EF.Property<object>(x,field)).ThenBy(x=>x.Id);
        var items=await sorted.Skip((page-1)*pageSize).Take(pageSize).Select(projection).ToListAsync(ct);
        return new PageResult<TDto>(items,count,page,pageSize);
    }
}

[ApiController]
[Route("api/transaction-pages")]
[Authorize]
public sealed class TransactionPagesController(ApplicationDbContext db, IAuthorizationService authorization) : ControllerBase
{
    [HttpGet("{kind}")]
    public async Task<IActionResult> List(string kind,[FromQuery]string? search,[FromQuery]string? sortBy,
        [FromQuery]bool descending=true,[FromQuery]int page=1,[FromQuery]int pageSize=20,CancellationToken ct=default)
    {
        if(page<1||pageSize<1||pageSize>100)return BadRequest(new{message="Invalid page or pageSize (1-100)."});
        var sale=kind.Equals("sales",StringComparison.OrdinalIgnoreCase);
        if(!sale && !kind.Equals("purchases",StringComparison.OrdinalIgnoreCase))return NotFound();
        if(!(await authorization.AuthorizeAsync(User,sale?"Sales.Read":"Purchases.Read")).Succeeded)return Forbid();
        if(sale){
            var q=db.Sales.AsNoTracking();
            if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.InvoiceNumber.Contains(search.Trim()));
            var total=await q.CountAsync(ct);
            var ordered=(sortBy?.ToLowerInvariant()) switch {
                "grandtotal"=>descending?q.OrderByDescending(x=>x.GrandTotal):q.OrderBy(x=>x.GrandTotal),
                "invoicenumber"=>descending?q.OrderByDescending(x=>x.InvoiceNumber):q.OrderBy(x=>x.InvoiceNumber),
                _=>descending?q.OrderByDescending(x=>x.SoldAtUtc):q.OrderBy(x=>x.SoldAtUtc)
            };
            var items=await ordered.ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
                .Select(x=>new SaleDto(x.Id,x.InvoiceNumber,x.CustomerId,x.SoldAtUtc,x.GrandTotal,x.Lines.Count)).ToListAsync(ct);
            return Ok(new PageResult<SaleDto>(items,total,page,pageSize));
        }else{
            var q=db.Purchases.AsNoTracking();
            if(!string.IsNullOrWhiteSpace(search))q=q.Where(x=>x.InvoiceNumber.Contains(search.Trim()));
            var total=await q.CountAsync(ct);
            var ordered=(sortBy?.ToLowerInvariant()) switch {
                "grandtotal"=>descending?q.OrderByDescending(x=>x.GrandTotal):q.OrderBy(x=>x.GrandTotal),
                "invoicenumber"=>descending?q.OrderByDescending(x=>x.InvoiceNumber):q.OrderBy(x=>x.InvoiceNumber),
                _=>descending?q.OrderByDescending(x=>x.PurchasedAtUtc):q.OrderBy(x=>x.PurchasedAtUtc)
            };
            var items=await ordered.ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize)
                .Select(x=>new PurchaseDto(x.Id,x.InvoiceNumber,x.SupplierId,x.PurchasedAtUtc,x.GrandTotal,x.Lines.Count)).ToListAsync(ct);
            return Ok(new PageResult<PurchaseDto>(items,total,page,pageSize));
        }
    }
}
