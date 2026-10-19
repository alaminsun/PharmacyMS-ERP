using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Infrastructure.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.ToTable("Categories"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Name).HasMaxLength(150).IsRequired();
        b.Property(x=>x.Description).HasMaxLength(500);
        b.HasIndex(x=>x.Name).IsUnique();
    }
}
public sealed class GenericConfiguration : IEntityTypeConfiguration<Generic>
{
    public void Configure(EntityTypeBuilder<Generic> b)
    {
        b.ToTable("Generics"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Code).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x=>x.Code).IsUnique();
    }
}
public sealed class ShelfConfiguration : IEntityTypeConfiguration<Shelf>
{
    public void Configure(EntityTypeBuilder<Shelf> b)
    {
        b.ToTable("Shelves"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Name).HasMaxLength(150).IsRequired();
        b.Property(x=>x.Number).HasMaxLength(50).IsRequired();
        b.HasIndex(x=>x.Number).IsUnique();
    }
}
public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> b)
    {
        b.ToTable("Suppliers"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Code).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Name).HasMaxLength(150).IsRequired();
        b.Property(x=>x.ShortName).HasMaxLength(50);
        b.Property(x=>x.Email).HasMaxLength(200);
        b.Property(x=>x.Phone).HasMaxLength(40);
        b.Property(x=>x.Address).HasMaxLength(300);
        b.Property(x=>x.Notes).HasMaxLength(500);
        b.HasIndex(x=>x.Code).IsUnique();
    }
}
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("Customers"); b.HasKey(x=>x.Id);
        b.Property(x=>x.Code).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Name).HasMaxLength(150).IsRequired();
        b.Property(x=>x.Email).HasMaxLength(200);
        b.Property(x=>x.Phone).HasMaxLength(40);
        b.Property(x=>x.Address).HasMaxLength(300);
        b.Property(x=>x.Notes).HasMaxLength(500);
        b.HasIndex(x=>x.Code).IsUnique();
    }
}
public sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> b)
    {
        b.ToTable("Medicines", t => {
            t.HasCheckConstraint("CK_Medicines_Prices","[BuyingPrice] >= 0 AND [SellingPrice] >= 0");
            t.HasCheckConstraint("CK_Medicines_MinimumStockLevel","[MinimumStockLevel] >= 0");
        });
        b.HasKey(x=>x.Id);
        b.Property(x=>x.Code).HasMaxLength(50).IsRequired();
        b.Property(x=>x.Name).HasMaxLength(200).IsRequired();
        b.Property(x=>x.Strength).HasMaxLength(100);
        b.Property(x=>x.BuyingPrice).HasPrecision(18,2);
        b.Property(x=>x.SellingPrice).HasPrecision(18,2);
        b.Property(x=>x.MinimumStockLevel).HasDefaultValue(10);
        b.HasIndex(x=>x.Code).IsUnique();
        b.HasOne(x=>x.Category).WithMany().HasForeignKey(x=>x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Generic).WithMany().HasForeignKey(x=>x.GenericId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Shelf).WithMany().HasForeignKey(x=>x.ShelfId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x=>x.Supplier).WithMany().HasForeignKey(x=>x.SupplierId).OnDelete(DeleteBehavior.Restrict);
    }
}
