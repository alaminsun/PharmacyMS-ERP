using System.Data;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PharmacyMS.Application.Interfaces;
using PharmacyMS.Domain.Entities;

namespace PharmacyMS.Infrastructure.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser,IdentityRole,string>(options),IApplicationDbContext
{
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Generic> Generics => Set<Generic>();
    public DbSet<Shelf> Shelves => Set<Shelf>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<StockBatch> StockBatches => Set<StockBatch>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseLine> PurchaseLines => Set<PurchaseLine>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleLine> SaleLines => Set<SaleLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<AuditEvent>(b=>{
            b.ToTable("AuditEvents"); b.HasKey(x=>x.Id);
            b.Property(x=>x.ActorId).HasMaxLength(450).IsRequired();
            b.Property(x=>x.ActorEmail).HasMaxLength(256).IsRequired();
            b.Property(x=>x.Action).HasMaxLength(10).IsRequired();
            b.Property(x=>x.Path).HasMaxLength(240).IsRequired();
            b.Property(x=>x.TraceId).HasMaxLength(100).IsRequired();
            b.HasIndex(x=>x.OccurredAtUtc);
            b.HasIndex(x=>x.ActorEmail);
        });
    }

    public async Task<IAppTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,CancellationToken ct)
        => new EfAppTransaction(await Database.BeginTransactionAsync(isolationLevel,ct));

    private sealed class EfAppTransaction(IDbContextTransaction transaction) : IAppTransaction
    {
        public Task CommitAsync(CancellationToken ct)=>transaction.CommitAsync(ct);
        public ValueTask DisposeAsync()=>transaction.DisposeAsync();
    }
}
