using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Infrastructure.Data.Configurations;

public sealed class StockBatchConfiguration : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> b)
    {
        b.ToTable("StockBatches",t=>t.HasCheckConstraint("CK_StockBatches_Quantity","[QuantityOnHand] >= 0"));
        b.HasKey(x=>x.Id);
        b.Property(x=>x.BatchNumber).HasMaxLength(50).IsRequired();
        b.Property(x=>x.ExpiryDate).HasColumnType("date");
        b.Property(x=>x.RowVersion).IsRowVersion();
        b.HasOne(x=>x.Medicine).WithMany(x=>x.StockBatches)
            .HasForeignKey(x=>x.MedicineId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x=>new{x.MedicineId,x.BatchNumber,x.ExpiryDate}).IsUnique();
    }
}
public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> b)
    {
        b.ToTable("Purchases"); b.HasKey(x=>x.Id);
        b.Property(x=>x.InvoiceNumber).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Notes).HasMaxLength(500);
        b.Property(x=>x.Discount).HasPrecision(18,2);
        b.Property(x=>x.GrandTotal).HasPrecision(18,2);
        b.HasIndex(x=>x.InvoiceNumber).IsUnique();
        b.HasOne(x=>x.Supplier).WithMany().HasForeignKey(x=>x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x=>x.Lines).WithOne(x=>x.Purchase).HasForeignKey(x=>x.PurchaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class PurchaseLineConfiguration : IEntityTypeConfiguration<PurchaseLine>
{
    public void Configure(EntityTypeBuilder<PurchaseLine> b)
    {
        b.ToTable("PurchaseLines"); b.HasKey(x=>x.Id);
        b.Property(x=>x.UnitPrice).HasPrecision(18,2);
        b.Property(x=>x.LineTotal).HasPrecision(18,2);
        b.HasOne(x=>x.StockBatch).WithMany().HasForeignKey(x=>x.StockBatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> b)
    {
        b.ToTable("Sales"); b.HasKey(x=>x.Id);
        b.Property(x=>x.InvoiceNumber).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Notes).HasMaxLength(500);
        b.Property(x=>x.Discount).HasPrecision(18,2);
        b.Property(x=>x.GrandTotal).HasPrecision(18,2);
        b.HasIndex(x=>x.InvoiceNumber).IsUnique();
        b.HasOne(x=>x.Customer).WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.HasMany(x=>x.Lines).WithOne(x=>x.Sale).HasForeignKey(x=>x.SaleId).OnDelete(DeleteBehavior.Restrict);
    }
}
public sealed class SaleLineConfiguration : IEntityTypeConfiguration<SaleLine>
{
    public void Configure(EntityTypeBuilder<SaleLine> b)
    {
        b.ToTable("SaleLines"); b.HasKey(x=>x.Id);
        b.Property(x=>x.UnitPrice).HasPrecision(18,2);
        b.Property(x=>x.LineTotal).HasPrecision(18,2);
        b.HasOne(x=>x.StockBatch).WithMany().HasForeignKey(x=>x.StockBatchId).OnDelete(DeleteBehavior.Restrict);
    }
}
