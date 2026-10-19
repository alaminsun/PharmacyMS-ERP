using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Stock.Queries;

public sealed record GetLowStockMedicinesQuery() : IRequest<IReadOnlyList<LowStockMedicineDto>>;
public sealed class GetLowStockMedicinesHandler(IApplicationDbContext db)
    : IRequestHandler<GetLowStockMedicinesQuery, IReadOnlyList<LowStockMedicineDto>>
{
    public async Task<IReadOnlyList<LowStockMedicineDto>> Handle(GetLowStockMedicinesQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var medicines = await db.Medicines.AsNoTracking().Where(m => m.IsActive)
            .Select(m => new
            {
                m.Id, m.Code, m.Name, m.MinimumStockLevel,
                AvailableUnits = m.StockBatches.Where(b => b.ExpiryDate > today)
                    .Sum(b => (int?)b.QuantityOnHand) ?? 0
            })
            .ToListAsync(ct);

        return medicines.Where(m => m.AvailableUnits == 0 || m.AvailableUnits <= m.MinimumStockLevel)
            .OrderBy(m => m.AvailableUnits == 0 ? 0 : 1)
            .ThenBy(m => m.AvailableUnits)
            .ThenBy(m => m.Name)
            .Select(m => new LowStockMedicineDto(m.Id, m.Code, m.Name, m.AvailableUnits,
                m.MinimumStockLevel, m.AvailableUnits == 0 ? "OutOfStock" : "LowStock"))
            .ToList();
    }
}
