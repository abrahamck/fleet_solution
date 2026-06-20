using Microsoft.EntityFrameworkCore;
using appfleet_nexus_data.Models;

namespace appfleet_nexus_data.Data;

public class FleetNexusDbContext : DbContext
{
    public FleetNexusDbContext(DbContextOptions<FleetNexusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Carrier> Carriers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.ToTable("fmcsa_census");
            entity.HasKey(e => e.DotNumber);
        });
    }
}
