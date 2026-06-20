using Serilog;
using Microsoft.EntityFrameworkCore;
using appfleet_nexus_data.Data;
using appfleet_nexus_data.Repositories;

// Configure Serilog logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/appfleet-nexus-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Load local overrides (gitignored) — overrides appsettings.json values locally
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

    // Add Serilog to the logging pipeline
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddOpenApi();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazor", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Configure database connection
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQL") 
        ?? "Host=localhost;Port=5432;Database=fleet_nexus_db;Username=postgres;Password=your_password_change_me";
    
    builder.Services.AddDbContext<FleetNexusDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Register repositories
    builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowBlazor");

    // Map Carrier endpoints
    var group = app.MapGroup("/api/carriers")
        .WithName("Carriers");

    group.MapGet("/top", GetTopCarriers)
        .WithName("GetTopCarriers")
        .WithDescription("Get the top 100 carriers by number of power units");

    group.MapGet("/{dotNumber}", GetCarrierByDotNumber)
        .WithName("GetCarrierByDotNumber")
        .WithDescription("Get a carrier by DOT number");

    // Health check endpoint
    app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
        .WithName("HealthCheck");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Endpoint handlers
static async Task<IResult> GetTopCarriers(ICarrierRepository repository)
{
    try
    {
        var carriers = await repository.GetTopCarriersByPowerUnitsAsync(100);
        return Results.Ok(new { success = true, count = carriers.Count, data = carriers });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving carriers: {ex.Message}", statusCode: 500);
    }
}



static async Task<IResult> GetCarrierByDotNumber(long dotNumber, ICarrierRepository repository)
{
    try
    {
        var carrier = await repository.GetCarrierByDotNumberAsync(dotNumber);
        if (carrier is null)
            return Results.NotFound(new { success = false, message = "Carrier not found" });
        return Results.Ok(new { success = true, data = carrier });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error retrieving carrier: {ex.Message}", statusCode: 500);
    }
}
