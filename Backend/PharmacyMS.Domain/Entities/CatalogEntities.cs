namespace PharmacyMS.Domain.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public sealed class Category : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class Generic : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class Shelf : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
}

public sealed class Supplier : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public sealed class Customer : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}

public sealed class Medicine : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? GenericId { get; set; }
    public Generic? Generic { get; set; }
    public int? ShelfId { get; set; }
    public Shelf? Shelf { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int MinimumStockLevel { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public ICollection<StockBatch> StockBatches { get; set; } = new List<StockBatch>();
}
