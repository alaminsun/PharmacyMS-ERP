using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Dashboard.Queries;

public sealed record GetDashboardQuery() : IRequest<DashboardDto>;
public sealed class GetDashboardHandler(IApplicationDbContext db) : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // Available stock excludes batches that expire today or have already expired.
        // Start at Medicines so medicines without any batches count as OutOfStock.
        var thresholds = await db.Medicines.AsNoTracking().Where(m => m.IsActive)
            .Select(m => new
            {
                m.MinimumStockLevel,
                AvailableUnits = m.StockBatches.Where(b => b.ExpiryDate > today)
                    .Sum(b => (int?)b.QuantityOnHand) ?? 0
            }).ToListAsync(ct);

        return new DashboardDto(
            await db.Medicines.CountAsync(x => x.IsActive, ct),
            await db.Suppliers.CountAsync(ct),
            await db.Customers.CountAsync(ct),
            await db.StockBatches.Where(x => x.ExpiryDate > today)
                .SumAsync(x => (int?)x.QuantityOnHand, ct) ?? 0,
            await db.StockBatches.Where(x => x.ExpiryDate <= today)
                .SumAsync(x => (int?)x.QuantityOnHand, ct) ?? 0,
            await db.Sales.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0m,
            await db.Purchases.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0m,
            thresholds.Count(x => x.AvailableUnits > 0 && x.AvailableUnits <= x.MinimumStockLevel),
            thresholds.Count(x => x.AvailableUnits == 0));
    }
}
