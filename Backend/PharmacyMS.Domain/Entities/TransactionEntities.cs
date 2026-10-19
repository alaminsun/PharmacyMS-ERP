namespace PharmacyMS.Domain.Entities;

public sealed class StockBatch : EntityBase
{
    public int MedicineId { get; set; }
    public Medicine Medicine { get; set; } = null!;
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int QuantityOnHand { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class Purchase : EntityBase
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime PurchasedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public decimal Discount { get; set; }
    public decimal GrandTotal { get; set; }
    public List<PurchaseLine> Lines { get; set; } = [];
}
public sealed class PurchaseLine : EntityBase
{
    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public int StockBatchId { get; set; }
    public StockBatch StockBatch { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class Sale : EntityBase
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public DateTime SoldAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public decimal Discount { get; set; }
    public decimal GrandTotal { get; set; }
    public List<SaleLine> Lines { get; set; } = [];
}
public sealed class SaleLine : EntityBase
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int StockBatchId { get; set; }
    public StockBatch StockBatch { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
