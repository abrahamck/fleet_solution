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
    
    // Core DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<TenantUser> TenantUsers { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<AlphaRegistration> AlphaRegistrations { get; set; }

    // Contact Management DbSets (FEATURE-001)
    public DbSet<ContactPhone>   ContactPhones    { get; set; }
    public DbSet<ContactEmail>   ContactEmails    { get; set; }
    public DbSet<ContactAddress> ContactAddresses { get; set; }
    public DbSet<VehicleContact> VehicleContacts  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Existing Carrier config ───
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

        modelBuilder.Entity<AlphaRegistration>(entity =>
        {
            entity.ToTable("alpha_registrations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Contact Management table mappings (FEATURE-001 — ADR-023)
        modelBuilder.Entity<ContactPhone>().ToTable("contact_phones");
        modelBuilder.Entity<ContactEmail>().ToTable("contact_emails");
        modelBuilder.Entity<ContactAddress>().ToTable("contact_addresses");
        modelBuilder.Entity<VehicleContact>().ToTable("vehicle_contacts");

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

        // VehicleContact → Vehicle (M:M join)
        modelBuilder.Entity<VehicleContact>()
            .HasOne(vc => vc.Vehicle)
            .WithMany(v => v.ContactAssignments)
            .HasForeignKey(vc => vc.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // VehicleContact → Contact (M:M join)
        modelBuilder.Entity<VehicleContact>()
            .HasOne(vc => vc.Contact)
            .WithMany(c => c.VehicleAssignments)
            .HasForeignKey(vc => vc.ContactId)
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

        // Partial unique index on contacts.unique_id per tenant (ADR-017)
        modelBuilder.Entity<Contact>()
            .HasIndex(c => new { c.TenantId, c.UniqueId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false AND \"UniqueId\" IS NOT NULL");

        // Filtered index on vehicles by tenant (active records only)
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // Unique unit_number per tenant (active records only)
        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => new { v.TenantId, v.UnitNumber })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // ContactPhone indexes — owner lookup
        modelBuilder.Entity<ContactPhone>()
            .HasIndex(p => new { p.OwnerType, p.OwnerId })
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<ContactPhone>()
            .HasIndex(p => p.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // ContactEmail indexes — owner lookup
        modelBuilder.Entity<ContactEmail>()
            .HasIndex(e => new { e.OwnerType, e.OwnerId })
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<ContactEmail>()
            .HasIndex(e => e.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // ContactAddress indexes — owner lookup
        modelBuilder.Entity<ContactAddress>()
            .HasIndex(a => new { a.OwnerType, a.OwnerId })
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<ContactAddress>()
            .HasIndex(a => a.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // VehicleContact indexes
        modelBuilder.Entity<VehicleContact>()
            .HasIndex(vc => vc.VehicleId)
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<VehicleContact>()
            .HasIndex(vc => vc.ContactId)
            .HasFilter("\"IsDeleted\" = false");

        modelBuilder.Entity<VehicleContact>()
            .HasIndex(vc => vc.TenantId)
            .HasFilter("\"IsDeleted\" = false");

        // ─── Default values ───
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Status)
            .HasDefaultValue("Active");

        modelBuilder.Entity<ContactAddress>()
            .Property(a => a.Country)
            .HasDefaultValue("USA");

        // ─── Global query filters for tenant isolation + soft delete ───
        // If _tenantAccessor is null (e.g. design time), we fallback to Guid.Empty to avoid NRE.
        modelBuilder.Entity<Contact>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);
        
        modelBuilder.Entity<Vehicle>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);

        // Contact Management global query filters (FEATURE-001)
        modelBuilder.Entity<ContactPhone>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);

        modelBuilder.Entity<ContactEmail>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);

        modelBuilder.Entity<ContactAddress>()
            .HasQueryFilter(e => e.TenantId == (_tenantAccessor != null ? _tenantAccessor.CurrentTenantId : Guid.Empty) && !e.IsDeleted);

        modelBuilder.Entity<VehicleContact>()
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
