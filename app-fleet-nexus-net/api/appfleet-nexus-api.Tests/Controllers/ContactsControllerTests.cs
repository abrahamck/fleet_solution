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
using AppFleetNexus.Api.Models;
using AppFleetNexus.Api.Tests.Mocks;
using AppFleetNexus.Data.Data;
using AppFleetNexus.Data.Models;
using Xunit;

namespace AppFleetNexus.Api.Tests.Controllers;

/// <summary>
/// Integration tests for ContactsController.
/// Uses SQLite in-memory with TestTenantContextAccessor.
/// Covers: TEST-001 through TEST-007, TEST-009, TEST-012 through TEST-015.
/// </summary>
public class ContactsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FleetNexusDbContext> _contextOptions;
    private readonly TestTenantContextAccessor _tenantAccessor;
    private readonly IMemoryCache _cache;

    public ContactsControllerTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<FleetNexusDbContext>()
            .UseSqlite(_connection)
            .Options;

        _tenantAccessor = new TestTenantContextAccessor();
        _cache = new MemoryCache(new MemoryCacheOptions());

        using var context = new FleetNexusDbContext(_contextOptions, _tenantAccessor);
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
        _cache.Dispose();
    }

    private FleetNexusDbContext CreateContext() =>
        new FleetNexusDbContext(_contextOptions, _tenantAccessor);

    private ContactsController CreateController(FleetNexusDbContext ctx) =>
        new ContactsController(ctx, _tenantAccessor, _cache, NullLogger<ContactsController>.Instance);

    private async Task SeedTenantAsync(Guid tenantId, string name)
    {
        using var ctx = CreateContext();
        ctx.Tenants.Add(new Tenant { Id = tenantId, Name = name, CreatedDate = DateTime.UtcNow, IsActive = true });
        await ctx.SaveChangesAsync();
    }

    private ContactUpsertRequest ValidContactRequest(string firstName = "John", string lastName = "Doe") =>
        new ContactUpsertRequest
        {
            FirstName = firstName,
            LastName = lastName,
            ContactType = "Driver",
            Status = "Active",
            Phones = new List<ContactPhoneDto>
            {
                new ContactPhoneDto { PhoneNumber = "555-0100", Label = "Primary", IsPrimary = true }
            },
            Addresses = new List<ContactAddressDto>
            {
                new ContactAddressDto
                {
                    AddressLine1 = "100 Main St",
                    City = "Austin",
                    State = "TX",
                    PostalCode = "78701",
                    IsPrimary = true
                }
            }
        };

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-001: Create contact succeeds with valid data (AC-001, AC-002)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContact_ValidRequest_ReturnsCreated()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;

        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.CreateContact(ValidContactRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var dto = Assert.IsType<ContactDetailDto>(created.Value);
        Assert.Equal("John", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Equal("Driver", dto.ContactType);
        Assert.Single(dto.Phones);
        Assert.Single(dto.Addresses);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-002: Create contact fails with 0 phones (I-001 / AC-003)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContact_NoPhones_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;

        var request = ValidContactRequest();
        request.Phones.Clear();

        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.CreateContact(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-003: Create contact fails with 0 addresses (I-001 / AC-004)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContact_NoAddresses_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;

        var request = ValidContactRequest();
        request.Addresses.Clear();

        using var ctx = CreateContext();
        var controller = CreateController(ctx);

        var result = await controller.CreateContact(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-004: Tenant UniqueId uniqueness check (AC-006)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateContact_DuplicateUniqueId_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        // Create first contact with unique ID
        var request1 = ValidContactRequest("Jane", "Smith");
        request1.UniqueId = "EMP-001";

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(request1);
        }

        // Try to create second contact with same unique ID
        var request2 = ValidContactRequest("Bob", "Jones");
        request2.UniqueId = "EMP-001";

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(request2);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-005: Get contacts returns only current tenant contacts (AC-013)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContacts_OnlyReturnsCurrentTenantContacts()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        await SeedTenantAsync(tenantA, "Tenant A");
        await SeedTenantAsync(tenantB, "Tenant B");

        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        // Seed contact for Tenant A
        _tenantAccessor.CurrentTenantId = tenantA;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Alice", "Alpha"));
        }

        // Seed contact for Tenant B
        _tenantAccessor.CurrentTenantId = tenantB;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Bob", "Beta"));
        }

        // Query as Tenant A
        _tenantAccessor.CurrentTenantId = tenantA;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.GetContacts(null, null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var contacts = Assert.IsAssignableFrom<System.Collections.IEnumerable>(ok.Value);
            var list = contacts.Cast<object>().ToList();
            Assert.Single(list);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-006: Get contact by ID returns 404 for cross-tenant access (AC-013)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactById_CrossTenantAccess_ReturnsNotFound()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        await SeedTenantAsync(tenantA, "Tenant A");
        await SeedTenantAsync(tenantB, "Tenant B");

        _tenantAccessor.CurrentUserId = Guid.NewGuid();
        _tenantAccessor.CurrentTenantId = tenantB;

        Guid contactBId;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(ValidContactRequest("Bob", "Beta"));
            var created = Assert.IsType<CreatedAtActionResult>(result);
            contactBId = ((ContactDetailDto)created.Value!).Id;
        }

        // Query as Tenant A — should not find Tenant B's contact
        _tenantAccessor.CurrentTenantId = tenantA;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.GetContactById(contactBId);
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-007: Delete contact blocked when sole vehicle contact (I-005 / AC-015)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteContact_SoleVehicleContact_ReturnsConflict()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        Guid contactId;
        Guid vehicleId;

        // Create contact
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(ValidContactRequest("John", "Doe"));
            var created = Assert.IsType<CreatedAtActionResult>(result);
            contactId = ((ContactDetailDto)created.Value!).Id;
        }

        // Create a vehicle and assign this contact as the sole contact
        using (var ctx = CreateContext())
        {
            var vehicle = new Vehicle { UnitNumber = "V-001", Status = "Active" };
            ctx.Vehicles.Add(vehicle);
            await ctx.SaveChangesAsync();
            vehicleId = vehicle.Id;

            ctx.VehicleContacts.Add(new VehicleContact
            {
                VehicleId = vehicleId,
                ContactId = contactId,
                AssociationRole = "Driver",
                IsPrimary = true,
                AssignedDate = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        // Try to delete the contact — should be blocked
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.DeleteContact(contactId);
            Assert.IsType<ConflictObjectResult>(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-009: Update contact maintains required invariants (AC-009)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateContact_RemoveAllPhones_ReturnsBadRequest()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        Guid contactId;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(ValidContactRequest());
            var created = Assert.IsType<CreatedAtActionResult>(result);
            contactId = ((ContactDetailDto)created.Value!).Id;
        }

        // Update without any phones
        var updateRequest = ValidContactRequest();
        updateRequest.Phones.Clear();

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.UpdateContact(contactId, updateRequest);
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-012: Soft delete removes contact from default query (AC-014)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteContact_PerformsSoftDelete()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        Guid contactId;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(ValidContactRequest());
            var created = Assert.IsType<CreatedAtActionResult>(result);
            contactId = ((ContactDetailDto)created.Value!).Id;
        }

        // Delete
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var deleteResult = await controller.DeleteContact(contactId);
            Assert.IsType<OkObjectResult>(deleteResult);
        }

        // Should not be found via normal query
        using (var ctx = CreateContext())
        {
            var found = await ctx.Contacts.FindAsync(contactId);
            Assert.Null(found);
        }

        // Should exist with IsDeleted=true when ignoring filters
        using (var ctx = CreateContext())
        {
            var deleted = await ctx.Contacts.IgnoreQueryFilters()
                .SingleOrDefaultAsync(c => c.Id == contactId);
            Assert.NotNull(deleted);
            Assert.True(deleted!.IsDeleted);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-013: Get contact detail returns full contact point collections (AC-005, AC-007)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactById_ReturnsFullDetail()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        var request = ValidContactRequest("Maria", "Sanchez");
        request.Emails = new List<ContactEmailDto>
        {
            new ContactEmailDto { EmailAddress = "maria@example.com", Label = "Work", IsPrimary = true }
        };

        Guid contactId;
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.CreateContact(request);
            var created = Assert.IsType<CreatedAtActionResult>(result);
            contactId = ((ContactDetailDto)created.Value!).Id;
        }

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.GetContactById(contactId);

            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ContactDetailDto>(ok.Value);
            Assert.Equal("Maria", dto.FirstName);
            Assert.Single(dto.Phones);
            Assert.Single(dto.Emails);
            Assert.Equal("maria@example.com", dto.Emails[0].EmailAddress);
            Assert.Single(dto.Addresses);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-014: Contact summary KPIs return correct counts (AC-012)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactSummary_ReturnsCorrectKpis()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        // Create 2 active drivers
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Driver", "One"));
        }
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Driver", "Two"));
        }

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.GetContactSummary();

            var ok = Assert.IsType<OkObjectResult>(result);
            var kpi = Assert.IsType<ContactKpiDto>(ok.Value);
            Assert.Equal(2, kpi.TotalContacts);
            Assert.Equal(2, kpi.ActiveDrivers);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // TEST-015: Search filter returns correct results (AC-001, AC-002)
    // ─────────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContacts_SearchFilter_ReturnsMatchingContacts()
    {
        var tenantId = Guid.NewGuid();
        await SeedTenantAsync(tenantId, "Tenant A");
        _tenantAccessor.CurrentTenantId = tenantId;
        _tenantAccessor.CurrentUserId = Guid.NewGuid();

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Alice", "Smith"));
        }
        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            await controller.CreateContact(ValidContactRequest("Bob", "Jones"));
        }

        using (var ctx = CreateContext())
        {
            var controller = CreateController(ctx);
            var result = await controller.GetContacts("Alice", null, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var contacts = ok.Value as System.Collections.IEnumerable;
            Assert.NotNull(contacts);
            Assert.Single(contacts!.Cast<object>().ToList());
        }
    }
}
