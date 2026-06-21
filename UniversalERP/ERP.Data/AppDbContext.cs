using ERP.Core.Entities;
using ERP.Core.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        // --- VERİTABANI TABLOLARI ---
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ServiceOrder> ServiceOrders { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<ProjectTask> ProjectTasks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }  // YENİ

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            var tenantId = _tenantProvider.GetTenantId();

            foreach (var entry in entries)
            {
                var prop = entry.Entity.GetType().GetProperty("TenantId");
                if (prop == null) continue;
                var currentValue = prop.GetValue(entry.Entity);
                if (currentValue == null || (currentValue is int intVal && intVal == 0))
                {
                    if (tenantId != 0)
                        prop.SetValue(entry.Entity, tenantId);
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal hassasiyet
            modelBuilder.Entity<Sale>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServiceOrder>().Property(p => p.ServiceFee).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ServiceOrder>().Property(p => p.PartCost).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<AppUser>().Property(p => p.Salary).HasColumnType("decimal(18,2)");

            // RowVersion (Optimistic Concurrency)
            modelBuilder.Entity<Product>().Property(p => p.RowVersion).IsRowVersion();

            // AuditLog — tenant filtresi YOK, SuperAdmin tümünü görebilir
            modelBuilder.Entity<AuditLog>().HasKey(a => a.Id);

            // Multi-Tenant Global Query Filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId() && !p.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId() && !p.IsDeleted);
            modelBuilder.Entity<ServiceOrder>().HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId() && !p.IsDeleted);
            modelBuilder.Entity<Sale>().HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId());
            modelBuilder.Entity<ProjectTask>().HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId() && !p.IsDeleted);
        }
    }
}
