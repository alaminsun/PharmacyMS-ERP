using Microsoft.EntityFrameworkCore;
using PharmacyMS.Domain.Entities;
using System.Data;

namespace PharmacyMS.Application.Interfaces;

public interface IAppTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}

public interface IApplicationDbContext
{
    DbSet<Medicine> Medicines { get; }
    DbSet<Category> Categories { get; }
    DbSet<Generic> Generics { get; }
    DbSet<Shelf> Shelves { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Customer> Customers { get; }
    DbSet<StockBatch> StockBatches { get; }
    DbSet<Purchase> Purchases { get; }
    DbSet<PurchaseLine> PurchaseLines { get; }
    DbSet<Sale> Sales { get; }
    DbSet<SaleLine> SaleLines { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IAppTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct);
}
