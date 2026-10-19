using System.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;
using PharmacyMS.Domain.Rules;

namespace PharmacyMS.Application.Features.Purchases.Commands;

public sealed record CreatePurchaseCommand(CreatePurchaseRequest Input) : IRequest<int>;
public sealed class CreatePurchaseHandler(IApplicationDbContext db) : IRequestHandler<CreatePurchaseCommand,int>
{
    public async Task<int> Handle(CreatePurchaseCommand request,CancellationToken ct)
    {
        var input=request.Input;
        await using var tx=await db.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        if (!await db.Suppliers.AnyAsync(x=>x.Id==input.SupplierId,ct)) throw new ApiException(400,"Supplier not found.");
        if (await db.Purchases.AnyAsync(x=>x.InvoiceNumber==input.InvoiceNumber,ct)) throw new ApiException(409,"Purchase invoice already exists.");
        var purchase=new Purchase {InvoiceNumber=input.InvoiceNumber, SupplierId=input.SupplierId,
            Notes=input.Notes,Discount=input.Discount};
        var today=DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var item in input.Items)
        {
            if (item.ExpiryDate <= today) throw new ApiException(400,"Cannot receive an expired batch.");
            if (!await db.Medicines.AnyAsync(x=>x.Id==item.MedicineId && x.IsActive,ct))
                throw new ApiException(400,$"Active medicine {item.MedicineId} not found.");
            var batch=await db.StockBatches.SingleOrDefaultAsync(x=>x.MedicineId==item.MedicineId &&
                x.BatchNumber==item.BatchNumber && x.ExpiryDate==item.ExpiryDate,ct);
            if (batch is null)
            {
                batch=new StockBatch {MedicineId=item.MedicineId,BatchNumber=item.BatchNumber,
                    ExpiryDate=item.ExpiryDate,QuantityOnHand=0};
                db.StockBatches.Add(batch);
            }
            batch.QuantityOnHand=checked(batch.QuantityOnHand+item.Quantity);
            purchase.Lines.Add(new PurchaseLine {StockBatch=batch,Quantity=item.Quantity,
                UnitPrice=item.UnitPrice,LineTotal=StockRules.LineTotal(item.Quantity,item.UnitPrice)});
        }
        purchase.GrandTotal=StockRules.GrandTotal(purchase.Lines.Select(x=>x.LineTotal),input.Discount);
        db.Purchases.Add(purchase);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return purchase.Id;
    }
}
public sealed class CreatePurchaseValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseValidator()
    {
        RuleFor(x=>x.Input).NotNull();
        RuleFor(x=>x.Input.InvoiceNumber).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Input.SupplierId).GreaterThan(0);
        RuleFor(x=>x.Input.Notes).MaximumLength(500);
        RuleFor(x=>x.Input.Discount).GreaterThanOrEqualTo(0).PrecisionScale(18,2,true);
        RuleFor(x=>x.Input.Items).NotEmpty().Must(items=>items is {Count:<=100}).WithMessage("At most 100 items per purchase.");
        RuleForEach(x=>x.Input.Items).ChildRules(item =>
        {
            item.RuleFor(x=>x.MedicineId).GreaterThan(0);
            item.RuleFor(x=>x.BatchNumber).NotEmpty().MaximumLength(50);
            item.RuleFor(x=>x.Quantity).GreaterThan(0);
            item.RuleFor(x=>x.UnitPrice).GreaterThanOrEqualTo(0).PrecisionScale(18,2,true);
        });
        RuleFor(x=>x.Input.Items).Must(items=>items is not null &&
            items.Select(x=>(x.MedicineId,x.BatchNumber,x.ExpiryDate)).Distinct().Count()==items.Count)
            .WithMessage("Duplicate batch lines are not allowed.");
    }
}
