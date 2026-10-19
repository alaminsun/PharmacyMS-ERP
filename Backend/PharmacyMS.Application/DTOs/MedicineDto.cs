namespace PharmacyMS.Application.DTOs;

public sealed record MedicineDto(int Id, string Code, string Name, string? Strength,
    int? CategoryId, string? CategoryName, int? GenericId, string? GenericName,
    int? ShelfId, string? ShelfName, int? SupplierId, string? SupplierName,
    decimal BuyingPrice, decimal SellingPrice, bool IsActive, int MinimumStockLevel);

public sealed record MedicineInput(string Code, string Name, string? Strength,
    int? CategoryId, int? GenericId, int? ShelfId, int? SupplierId,
    decimal BuyingPrice, decimal SellingPrice, bool IsActive = true, int MinimumStockLevel = 10);
