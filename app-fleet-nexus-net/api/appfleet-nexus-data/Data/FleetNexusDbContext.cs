using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using appfleet_nexus_data.Models;
using appfleet_nexus_data.Tenancy;

namespace appfleet_nexus_data.Data;

public class FleetNexusDbContext : DbContext
{
    private readonly ITenantContextAccessor _tenantAccessor;

    // We allow a nullable accessor to support design-time tools if they don't provide one, 
    // though ideally the DI container provides it.
    public FleetNexusDbContext(
        DbContextOptions<FleetNexusDbContext> options,
        ITenantContextAccessor? tenantAccessor = null) : base(options)
    {
        _tenantAccessor = tenantAccessor!; // Will be null at design time if not registered
    }

    public DbSet<Carrier> Carriers { get; set; }
    
    // New DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Existing Carrier config
        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.ToTable("fmcsa_census");
            entity.HasKey(e => e.DotNumber);
        });

        // Composite key for TenantUser
        modelBuilder.Entity<TenantUser>()
            .HasKey(tu => new { tu.UserId, tu.TenantId });

        // Global query filters for tenant isolation + soft delete
        // If _tenantAccessor is null (e.g. design time), we fallback to Guid.Empty to avoid NRE.
        modelBuilder.Entity<Contact>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);
        
        modelBuilder.Entity<Vehicle>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(ct);
    }

    private void ApplyAuditInfo()
    {
        if (_tenantAccessor == null) return;

        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;
        var userId = _tenantAccessor.CurrentUserId;
        var tenantId = _tenantAccessor.CurrentTenantId;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.TenantId = tenantId;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedDate = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedDate = now;
                    break;
                case EntityState.Deleted:
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedDate = now;
                    break;
            }
        }
    }
}
