using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Sales.Queries;
public sealed record GetSalesQuery() : IRequest<IReadOnlyList<SaleDto>>;
public sealed class GetSalesHandler(IApplicationDbContext db) : IRequestHandler<GetSalesQuery,IReadOnlyList<SaleDto>>
{
    public async Task<IReadOnlyList<SaleDto>> Handle(GetSalesQuery request,CancellationToken ct)=>
        await db.Sales.AsNoTracking().OrderByDescending(x=>x.Id)
            .Select(x=>new SaleDto(x.Id,x.InvoiceNumber,x.CustomerId,x.SoldAtUtc,x.GrandTotal,x.Lines.Count))
            .ToListAsync(ct);
}
