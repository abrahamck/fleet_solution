# Architecture Design & Implementation: Alpha Roadblock Registration

This document records the architectural decisions, database schema design, and implementation details for the simplified landing page Alpha roadblock registration flow.

## 1. Landing Page Design Choices

To maximize user conversion and simplify the landing experience:
- **Streamlined Visual Layout**: Replaced the feature voter roadmap and registration forms with a clean, centered title and subheadline.
- **SPA Stepper Pattern**: Implemented a 4-step Single Page Application (SPA) flow in [Home.razor](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Home.razor):
  - **Step 1 (Intro)**: Simple call to action leading to the roadblock selection.
  - **Step 2 (Roadblocks)**: Prompts the question *"What is your biggest fleet roadblock?"* and presents the 10 options.
  - **Step 3 (Email Submission)**: Collects the user's business email address for the lifetime free tier lock-in.
  - **Step 4 (Success State)**: Confirms the registration and lists their inputs.
- **Multi-Select Capability**: Allowed users to select multiple cards instead of forcing a single choice, providing more comprehensive onboarding signal data.
- **Custom Roadblock Text Input**: Dynamically expands a description box if the user selects the "Other Challenge" option.

---

## 2. Database Architecture Decisions

For early feedback and signups:
- **Flat Table Structure**: Implemented a single table `alpha_registrations` containing all user responses. This avoids relational joins or complex junction tables, making it straightforward to query and export.
- **Native PostgreSQL Arrays (`text[]`)**: Stored the selected roadblocks as a Postgres string array (`text[]`). It maps natively to `List<string>` in C# / Entity Framework Core without requiring custom JSON converters or separate relational schema overhead.

### Schema Design (`alpha_registrations` table)

| Column Name | Data Type | Constraints | Description |
| :--- | :--- | :--- | :--- |
| **id** | `uuid` | Primary Key | Unique identifier. |
| **email** | `varchar(256)` | Not Null, Index | Lowcase-normalized user business email. |
| **roadblocks** | `text[]` | Not Null | List of selected roadblocks. |
| **custom_roadblock** | `text` | Nullable | Detail description if "Other" roadblock was selected. |
| **created_at** | `timestamptz` | Not Null, Default: Now, Index | Registration submission timestamp. |

---

## 3. Implementation Details

### Data Model ([AlphaRegistration.cs](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/api/appfleet-nexus-data/Models/AlphaRegistration.cs))
Defined the model inside the data project:
```csharp
namespace AppFleetNexus.Data.Models;

public class AlphaRegistration
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public List<string> Roadblocks { get; set; } = new();
    public string? CustomRoadblock { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### DbContext Configuration ([FleetNexusDbContext.cs](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/api/appfleet-nexus-data/Data/FleetNexusDbContext.cs))
Registered the DbSet and configured naming convention / indexes inside `OnModelCreating`:
```csharp
public DbSet<AlphaRegistration> AlphaRegistrations { get; set; }

// mapped inside OnModelCreating
modelBuilder.Entity<AlphaRegistration>(entity =>
{
    entity.ToTable("alpha_registrations");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
    entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
    entity.HasIndex(e => e.Email);
    entity.HasIndex(e => e.CreatedAt);
});
```

### API Endpoint ([AlphaController.cs](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/api/appfleet-nexus-api/Controllers/AlphaController.cs))
Exposed `POST /api/alpha/register` supporting anonymous access with input checks:
```csharp
[AllowAnonymous]
[ApiController]
[Route("api/alpha")]
public class AlphaController : ControllerBase
{
    private readonly FleetNexusDbContext _dbContext;

    public AlphaController(FleetNexusDbContext dbContext) { ... }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAlphaRequest request)
    {
        // 1. Validations on Email format, non-empty roadblocks, and custom text.
        // 2. Map and add to database.
        // 3. SaveChangesAsync.
    }
}
```

### Blazor UI HTTP Request Integration ([Home.razor](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/ui/appfleet-nexus-ui/Pages/Home.razor))
Frontend injected with `HttpClient Http` making async calls to post JSON payloads to the API:
```csharp
var response = await Http.PostAsJsonAsync("api/alpha/register", request);
if (response.IsSuccessStatusCode)
{
    // Transition to success screen and save state locally
}
else
{
    // Render validation error message returned from the API controller
}
```
