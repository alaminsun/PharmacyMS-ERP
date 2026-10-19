namespace PharmacyMS.Application.DTOs;

public sealed record PurchaseItemInput(int MedicineId, string BatchNumber, DateOnly ExpiryDate, int Quantity, decimal UnitPrice);
public sealed record CreatePurchaseRequest(string InvoiceNumber, int SupplierId, string? Notes, decimal Discount, List<PurchaseItemInput> Items);
public sealed record SaleItemInput(int StockBatchId, int Quantity);
public sealed record CreateSaleRequest(string InvoiceNumber, int? CustomerId, string? Notes, decimal Discount, List<SaleItemInput> Items);
public sealed record PurchaseDto(int Id, string InvoiceNumber, int SupplierId, DateTime PurchasedAtUtc, decimal GrandTotal, int LineCount);
public sealed record SaleDto(int Id, string InvoiceNumber, int? CustomerId, DateTime SoldAtUtc, decimal GrandTotal, int LineCount);
public sealed record StockDto(int StockBatchId, int MedicineId, string MedicineCode, string MedicineName,
    string BatchNumber, DateOnly ExpiryDate, int QuantityOnHand, bool IsExpired);
public sealed record DashboardDto(int Medicines, int Suppliers, int Customers, int AvailableUnits, int ExpiredUnits,
    decimal TotalSales, decimal TotalPurchases, int LowStockMedicines, int OutOfStockMedicines);

public sealed record LowStockMedicineDto(int MedicineId, string MedicineCode, string MedicineName,
    int AvailableUnits, int MinimumStockLevel, string Status);
