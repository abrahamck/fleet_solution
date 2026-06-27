using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using AppFleetNexus.Api.Controllers;
using AppFleetNexus.Api.Tests.Mocks;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;
using Xunit;

namespace AppFleetNexus.Api.Tests.Controllers;

public class VehiclesControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FleetNexusDbContext> _contextOptions;
    private readonly TestTenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;

    public VehiclesControllerTests()
    {
        // Setup SQLite In-Memory database
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<FleetNexusDbContext>()
            .UseSqlite(_connection)
            .Options;

        _tenantAccessor = new TestTenantContextAccessor();
        _cache = new MemoryCache(new MemoryCacheOptions());

        // Create the schema in the test database
        using var context = new FleetNexusDbContext(_contextOptions, _tenantAccessor);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        _cache.Dispose();
    }

    private FleetNexusDbContext CreateContext()
    {
        return new FleetNexusDbContext(_contextOptions, _tenantAccessor);
    }

    private async Task SeedTenantAsync(Guid tenantId, string name)
    {
        using var context = CreateContext();
        context.Tenants.Add(new Tenant
        {
            Id = tenantId,
            Name = name,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetVehicles_OnlyReturnsCurrentTenantVehicles()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await SeedTenantAsync(tenantA, "Tenant A");
        await SeedTenantAsync(tenantB, "Tenant B");

        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        using (var context = CreateContext())
        {
            // Seed a vehicle for Tenant A and save immediately under Tenant A's audit context
            _tenantAccessor.CurrentTenantId = tenantA;
            context.Vehicles.Add(new Vehicle
            {
                UnitNumber = "V-TenantA",
                Status = "Active"
            });
            await context.SaveChangesAsync();

            // Seed a vehicle for Tenant B and save immediately under Tenant B's audit context
            _tenantAccessor.CurrentTenantId = tenantB;
            context.Vehicles.Add(new Vehicle
            {
                UnitNumber = "V-TenantB",
                Status = "Active"
            });
            await context.SaveChangesAsync();
        }

        // Act
        _tenantAccessor.CurrentTenantId = tenantA; // Authenticated as Tenant A
        using (var context = CreateContext())
        {
            var controller = new VehiclesController(context, _tenantAccessor, _cache, NullLogger<VehiclesController>.Instance);
            var result = await controller.GetVehicles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var vehicles = Assert.IsAssignableFrom<IEnumerable<Vehicle>>(okResult.Value);
            
            Assert.Single(vehicles);
            Assert.Equal("V-TenantA", vehicles.First().UnitNumber);
        }
    }

    [Fact]
    public async Task GetVehicleById_RestrictsCrossTenantAccess()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid vehicleBId;

        await SeedTenantAsync(tenantA, "Tenant A");
        await SeedTenantAsync(tenantB, "Tenant B");

        // Seed Tenant B's vehicle
        _tenantAccessor.CurrentTenantId = tenantB;
        using (var context = CreateContext())
        {
            var vehicleB = new Vehicle
            {
                UnitNumber = "102B",
                Status = "Active"
            };
            context.Vehicles.Add(vehicleB);
            await context.SaveChangesAsync();
            vehicleBId = vehicleB.Id;
        }

        // Act
        _tenantAccessor.CurrentTenantId = tenantA; // Switch to Tenant A
        using (var context = CreateContext())
        {
            var controller = new VehiclesController(context, _tenantAccessor, _cache, NullLogger<VehiclesController>.Instance);
            var result = await controller.GetVehicleById(vehicleBId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }

    [Fact]
    public async Task CreateVehicle_ValidatesUniqueUnitNumber()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        await SeedTenantAsync(tenantA, "Tenant A");

        _tenantAccessor.CurrentTenantId = tenantA;

        using (var context = CreateContext())
        {
            context.Vehicles.Add(new Vehicle
            {
                UnitNumber = "101",
                Status = "Active"
            });
            await context.SaveChangesAsync();
        }

        // Act
        using (var context = CreateContext())
        {
            var controller = new VehiclesController(context, _tenantAccessor, _cache, NullLogger<VehiclesController>.Instance);
            var request = new VehicleUpsertRequest
            {
                UnitNumber = "101",
                Status = "Active"
            };

            var result = await controller.CreateVehicle(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }
    }

    [Fact]
    public async Task CreateVehicle_AllowsDuplicateUnitNumberOnDifferentTenants()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await SeedTenantAsync(tenantA, "Tenant A");
        await SeedTenantAsync(tenantB, "Tenant B");

        // Seed Unit 101 for Tenant B
        _tenantAccessor.CurrentTenantId = tenantB;
        using (var context = CreateContext())
        {
            context.Vehicles.Add(new Vehicle
            {
                UnitNumber = "101",
                Status = "Active"
            });
            await context.SaveChangesAsync();
        }

        // Act & Assert: Create Unit 101 under Tenant A
        _tenantAccessor.CurrentTenantId = tenantA;
        using (var context = CreateContext())
        {
            var controller = new VehiclesController(context, _tenantAccessor, _cache, NullLogger<VehiclesController>.Instance);
            var request = new VehicleUpsertRequest
            {
                UnitNumber = "101",
                Status = "Active"
            };

            var result = await controller.CreateVehicle(request);
            
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var vehicle = Assert.IsType<Vehicle>(createdResult.Value);
            Assert.Equal("101", vehicle.UnitNumber);
        }
    }

    [Fact]
    public async Task DeleteVehicle_PerformsSoftDelete()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        await SeedTenantAsync(tenantA, "Tenant A");

        _tenantAccessor.CurrentTenantId = tenantA;
        Guid vehicleId;

        using (var context = CreateContext())
        {
            var vehicle = new Vehicle
            {
                UnitNumber = "105",
                Status = "Active"
            };
            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync();
            vehicleId = vehicle.Id;
        }

        // Act: Delete the vehicle
        using (var context = CreateContext())
        {
            var controller = new VehiclesController(context, _tenantAccessor, _cache, NullLogger<VehiclesController>.Instance);
            var result = await controller.DeleteVehicle(vehicleId);
            Assert.IsType<OkObjectResult>(result);
        }

        // Assert: Query normally (it shouldn't be found due to Query Filters)
        using (var context = CreateContext())
        {
            var found = await context.Vehicles.FindAsync(vehicleId);
            Assert.Null(found);
        }

        // Assert: Query database ignoring filters (it should be soft-deleted in the database)
        using (var context = CreateContext())
        {
            var softDeletedVehicle = await context.Vehicles
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(v => v.Id == vehicleId);

            Assert.NotNull(softDeletedVehicle);
            Assert.True(softDeletedVehicle.IsDeleted);
        }
    }
}
