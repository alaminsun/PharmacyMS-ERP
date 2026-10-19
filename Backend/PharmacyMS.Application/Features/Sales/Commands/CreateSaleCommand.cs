using System.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;
using PharmacyMS.Domain.Rules;

namespace PharmacyMS.Application.Features.Sales.Commands;

public sealed record CreateSaleCommand(CreateSaleRequest Input) : IRequest<int>;
public sealed class CreateSaleHandler(IApplicationDbContext db) : IRequestHandler<CreateSaleCommand,int>
{
    public async Task<int> Handle(CreateSaleCommand request,CancellationToken ct)
    {
        var input=request.Input;
        await using var tx=await db.BeginTransactionAsync(IsolationLevel.Serializable,ct);
        if (await db.Sales.AnyAsync(x=>x.InvoiceNumber==input.InvoiceNumber,ct))
            throw new ApiException(409,"Sale invoice already exists.");
        if (input.CustomerId.HasValue && !await db.Customers.AnyAsync(x=>x.Id==input.CustomerId.Value,ct))
            throw new ApiException(400,"Customer not found.");
        var sale=new Sale {InvoiceNumber=input.InvoiceNumber, CustomerId=input.CustomerId,
            Notes=input.Notes,Discount=input.Discount};
        var today=DateOnly.FromDateTime(DateTime.UtcNow);
        foreach (var item in input.Items)
        {
            var batch=await db.StockBatches.Include(x=>x.Medicine)
                .SingleOrDefaultAsync(x=>x.Id==item.StockBatchId,ct)
                ?? throw new ApiException(404,$"Batch {item.StockBatchId} not found.");
            if (!batch.Medicine.IsActive) throw new ApiException(400,"Medicine is not active.");
            if (batch.ExpiryDate<=today) throw new ApiException(400,"Expired medicine cannot be sold.");
            if (batch.QuantityOnHand<item.Quantity) throw new ApiException(409,$"Insufficient stock in batch {batch.Id}.");
            StockRules.EnsureSaleable(batch.QuantityOnHand,item.Quantity,batch.ExpiryDate,today);
            batch.QuantityOnHand-=item.Quantity;
            var unitPrice=batch.Medicine.SellingPrice; // Server-side price; never trust client-submitted totals.
            sale.Lines.Add(new SaleLine {StockBatchId=batch.Id,Quantity=item.Quantity,
                UnitPrice=unitPrice,LineTotal=StockRules.LineTotal(item.Quantity,unitPrice)});
        }
        sale.GrandTotal=StockRules.GrandTotal(sale.Lines.Select(x=>x.LineTotal),input.Discount);
        db.Sales.Add(sale);
        await db.SaveChangesAsync(ct); // EF Core transaction + rowversion protects inventory from overselling.
        await tx.CommitAsync(ct);
        return sale.Id;
    }
}
public sealed class CreateSaleValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleValidator()
    {
        RuleFor(x=>x.Input).NotNull();
        RuleFor(x=>x.Input.InvoiceNumber).NotEmpty().MaximumLength(50);
        RuleFor(x=>x.Input.CustomerId).GreaterThan(0).When(x=>x.Input.CustomerId.HasValue);
        RuleFor(x=>x.Input.Notes).MaximumLength(500);
        RuleFor(x=>x.Input.Discount).GreaterThanOrEqualTo(0).PrecisionScale(18,2,true);
        RuleFor(x=>x.Input.Items).NotEmpty().Must(items=>items is {Count:<=100}).WithMessage("At most 100 items per sale.");
        RuleForEach(x=>x.Input.Items).ChildRules(item => {
            item.RuleFor(x=>x.StockBatchId).GreaterThan(0);
            item.RuleFor(x=>x.Quantity).GreaterThan(0);
        });
        RuleFor(x=>x.Input.Items).Must(items=>items is not null &&
            items.Select(x=>x.StockBatchId).Distinct().Count()==items.Count)
            .WithMessage("Duplicate batch lines are not allowed.");
    }
}
