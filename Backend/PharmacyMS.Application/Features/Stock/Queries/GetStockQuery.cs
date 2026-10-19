using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Stock.Queries;

public sealed record GetStockQuery(bool IncludeZero=false) : IRequest<IReadOnlyList<StockDto>>;
public sealed class GetStockHandler(IApplicationDbContext db) : IRequestHandler<GetStockQuery,IReadOnlyList<StockDto>>
{
    public async Task<IReadOnlyList<StockDto>> Handle(GetStockQuery request,CancellationToken ct)
    {
        var today=DateOnly.FromDateTime(DateTime.UtcNow);
        var query=db.StockBatches.AsNoTracking();
        if (!request.IncludeZero) query=query.Where(x=>x.QuantityOnHand>0);
        return await query.OrderBy(x=>x.ExpiryDate)
            .Select(x=>new StockDto(x.Id,x.MedicineId,x.Medicine.Code,x.Medicine.Name,
                x.BatchNumber,x.ExpiryDate,x.QuantityOnHand,x.ExpiryDate<=today)).ToListAsync(ct);
    }
}
