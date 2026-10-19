namespace PharmacyMS.Application.DTOs;

// SalesTotal and PurchasesTotal are NET amounts saved with the transactions.
// Subtotals and discounts expose how the totals were calculated; no historical prices are overwritten.
public sealed record ReportDto(DateTime FromUtc, DateTime ToUtc, int SaleCount, int PurchaseCount,
    decimal SalesTotal, decimal PurchasesTotal, decimal SalesSubtotal, decimal SalesDiscount,
    decimal PurchasesSubtotal, decimal PurchasesDiscount);

public sealed record SaleReportLineDto(int StockBatchId, string MedicineCode, string MedicineName,
    int Quantity, decimal UnitPrice, decimal LineTotal);
public sealed record SaleReportDetailDto(int SaleId, string InvoiceNumber, DateTime SoldAtUtc,
    decimal Subtotal, decimal Discount, decimal GrandTotal, IReadOnlyList<SaleReportLineDto> Items);
