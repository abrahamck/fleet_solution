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
    builder.Services.AddControllers();
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
        ?? throw new InvalidOperationException("Connection string 'PostgreSQL' not found.");
    
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
    app.MapControllers();

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
