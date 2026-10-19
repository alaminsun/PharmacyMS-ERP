using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Purchases.Queries;
public sealed record GetPurchasesQuery() : IRequest<IReadOnlyList<PurchaseDto>>;
public sealed class GetPurchasesHandler(IApplicationDbContext db) : IRequestHandler<GetPurchasesQuery,IReadOnlyList<PurchaseDto>>
{
    public async Task<IReadOnlyList<PurchaseDto>> Handle(GetPurchasesQuery request,CancellationToken ct)=>
        await db.Purchases.AsNoTracking().OrderByDescending(x=>x.Id)
            .Select(x=>new PurchaseDto(x.Id,x.InvoiceNumber,x.SupplierId,x.PurchasedAtUtc,x.GrandTotal,x.Lines.Count))
            .ToListAsync(ct);
}
