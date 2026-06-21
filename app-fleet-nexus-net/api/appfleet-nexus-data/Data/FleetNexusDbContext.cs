using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AppFleetNexus.Data.Models;
using AppFleetNexus.Data.Tenancy;

namespace AppFleetNexus.Data.Data;

public class FleetNexusDbContext : DbContext
{
    private readonly ITenantContextAccessor? _tenantAccessor;

    // We allow a nullable accessor to support design-time tools if they don't provide one, 
    // though ideally the DI container provides it.
    public FleetNexusDbContext(
        DbContextOptions<FleetNexusDbContext> options,
        ITenantContextAccessor? tenantAccessor = null) : base(options)
    {
        _tenantAccessor = tenantAccessor;
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

        // ─── Table name mapping (snake_case to match Supabase/PostgreSQL convention) ───
        modelBuilder.Entity<Tenant>().ToTable("tenants");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<TenantUser>().ToTable("tenant_users");
        modelBuilder.Entity<Contact>().ToTable("contacts");
        modelBuilder.Entity<Vehicle>().ToTable("vehicles");

        // ─── TenantUser composite key ───
        modelBuilder.Entity<TenantUser>()
            .HasKey(tu => new { tu.UserId, tu.TenantId });

        // ─── Relationships ───

        // Contact → Tenant FK
        modelBuilder.Entity<Contact>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Vehicle → Tenant FK
        modelBuilder.Entity<Vehicle>()
            .HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(v => v.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Indexes ───

        // Unique email on users
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Single-tenant-per-user (can be dropped later for multi-tenant support)
        modelBuilder.Entity<TenantUser>()
            .HasIndex(tu => tu.UserId)
            .IsUnique();

        // Filtered index on contacts by tenant (active records only)
        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // Filtered index on vehicles by tenant (active records only)
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // Unique unit_number per tenant (active records only)
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => new { v.TenantId, v.UnitNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // ─── Default values ───
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasDefaultValue("Active");

        // ─── Global query filters for tenant isolation + soft delete ───
        // If _tenantAccessor is null (e.g. design time), we fallback to Guid.Empty to avoid NRE.
        modelBuilder.Entity<Contact>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);
        
        modelBuilder.Entity<Vehicle>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(ct);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
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
