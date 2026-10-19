using MediatR;
using Microsoft.EntityFrameworkCore;
using PharmacyMS.Application.Common;
using PharmacyMS.Application.DTOs;
using PharmacyMS.Application.Interfaces;

namespace PharmacyMS.Application.Features.Reports.Queries;

public sealed record GetReportQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<ReportDto>;
public sealed class GetReportHandler(IApplicationDbContext db) : IRequestHandler<GetReportQuery, ReportDto>
{
    public async Task<ReportDto> Handle(GetReportQuery request, CancellationToken ct)
    {
        ReportRangeValidator.Validate(request.FromUtc, request.ToUtc);
        var sales = db.Sales.AsNoTracking().Where(x => x.SoldAtUtc >= request.FromUtc && x.SoldAtUtc < request.ToUtc);
        var purchases = db.Purchases.AsNoTracking().Where(x => x.PurchasedAtUtc >= request.FromUtc && x.PurchasedAtUtc < request.ToUtc);

        var salesTotal = await sales.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0m;
        var purchasesTotal = await purchases.SumAsync(x => (decimal?)x.GrandTotal, ct) ?? 0m;
        var salesDiscount = await sales.SumAsync(x => (decimal?)x.Discount, ct) ?? 0m;
        var purchasesDiscount = await purchases.SumAsync(x => (decimal?)x.Discount, ct) ?? 0m;

        return new ReportDto(request.FromUtc, request.ToUtc,
            await sales.CountAsync(ct), await purchases.CountAsync(ct),
            salesTotal, purchasesTotal,
            salesTotal + salesDiscount, salesDiscount,
            purchasesTotal + purchasesDiscount, purchasesDiscount);
    }
}

public sealed record GetSalesReportDetailsQuery(DateTime FromUtc, DateTime ToUtc)
    : IRequest<IReadOnlyList<SaleReportDetailDto>>;
public sealed class GetSalesReportDetailsHandler(IApplicationDbContext db)
    : IRequestHandler<GetSalesReportDetailsQuery, IReadOnlyList<SaleReportDetailDto>>
{
    public async Task<IReadOnlyList<SaleReportDetailDto>> Handle(GetSalesReportDetailsQuery request, CancellationToken ct)
    {
        ReportRangeValidator.Validate(request.FromUtc, request.ToUtc);
        // Saved line-level prices are the authoritative historical values, not the medicine's CURRENT selling price.
        var sales = await db.Sales.AsNoTracking()
            .Where(x => x.SoldAtUtc >= request.FromUtc && x.SoldAtUtc < request.ToUtc)
            .OrderBy(x => x.SoldAtUtc).ThenBy(x => x.Id)
            .Select(x => new
            {
                x.Id, x.InvoiceNumber, x.SoldAtUtc, x.Discount, x.GrandTotal,
                Lines = x.Lines.Select(line => new SaleReportLineDto(
                    line.StockBatchId, line.StockBatch.Medicine.Code, line.StockBatch.Medicine.Name,
                    line.Quantity, line.UnitPrice, line.LineTotal)).ToList()
            }).ToListAsync(ct);

        return sales.Select(x => new SaleReportDetailDto(x.Id, x.InvoiceNumber, x.SoldAtUtc,
            x.Lines.Sum(line => line.LineTotal), x.Discount, x.GrandTotal, x.Lines)).ToList();
    }
}

internal static class ReportRangeValidator
{
    public static void Validate(DateTime fromUtc, DateTime toUtc)
    {
        if (toUtc <= fromUtc || toUtc - fromUtc > TimeSpan.FromDays(366))
            throw new ApiException(400, "Report range must be positive and at most 366 days; ToUtc is exclusive.");
    }
}
