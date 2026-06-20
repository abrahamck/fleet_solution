# FleetNexus API Documentation

This directory contains the documentation for the FleetNexus backend API, a modern Minimal APIs service built with .NET 10 and PostgreSQL.

## Architecture

* **Framework**: ASP.NET Core 10 with Minimal APIs
* **Database**: PostgreSQL with Entity Framework Core 10 (Npgsql)
* **Logging**: Serilog (console and file rolling log output)
* **Pattern**: Repository pattern for data access abstraction

---

## Database Setup

Run the following SQL script to create the required database, table schema, and populate initial seed data:

```sql
CREATE DATABASE fleet_nexus_db;

-- Connect to the database
\c fleet_nexus_db

-- Create the fmcsa_census table
CREATE TABLE fmcsa_census (
    dot_number BIGINT PRIMARY KEY,
    legal_name VARCHAR(255),
    phy_city VARCHAR(100),
    phy_state VARCHAR(50),
    nbr_power_unit INTEGER
);

-- Insert sample seed data
INSERT INTO fmcsa_census (dot_number, legal_name, phy_city, phy_state, nbr_power_unit)
VALUES
    (1000001, 'ABC Logistics Inc', 'Chicago', 'IL', 250),
    (1000002, 'XYZ Transport LLC', 'Dallas', 'TX', 180),
    (1000003, 'Premium Freight Co', 'Los Angeles', 'CA', 320),
    (1000004, 'FastTrack Shipping', 'Houston', 'TX', 145),
    (1000005, 'National Fleet Services', 'Atlanta', 'GA', 275),
    (1000006, 'Regional Carriers Inc', 'Denver', 'CO', 95),
    (1000007, 'Express Logistics Network', 'Phoenix', 'AZ', 210),
    (1000008, 'Midwest Transportation', 'Minneapolis', 'MN', 155),
    (1000009, 'Pacific Northwest Freight', 'Seattle', 'WA', 120),
    (1000010, 'Sunbelt Hauling', 'Tampa', 'FL', 190);
```

### Update Database Connection

Edit `appsettings.json` or `appsettings.Development.json` under `/api/appfleet-nexus-api/` and configure the connection string:

```json
"ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=fleet_nexus_db;Username=postgres;Password=YOUR_PASSWORD"
}
```

---

## API Endpoints

The API is exposed at `http://localhost:5067` (HTTP) and `https://localhost:7298` (HTTPS) by default.

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **GET** | `/api/carriers/top` | Get top 100 carriers with non-null power units |
| **GET** | `/api/carriers/` | Get all carriers |
| **GET** | `/api/carriers/{dotNumber}` | Get a specific carrier by DOT number |
| **GET** | `/api/carriers/count` | Get total count of carriers |
| **GET** | `/health` | API health check status |

### Example Response (`/api/carriers/top`)

```json
{
  "success": true,
  "count": 1,
  "data": [
    {
      "dotNumber": 1000003,
      "legalName": "Premium Freight Co",
      "city": "Los Angeles",
      "state": "CA",
      "numberOfPowerUnits": 320
    }
  ]
}
```

---

## Development Notes

### Adding New Database Models
1. Add your entity class file inside `appfleet-nexus-data/Models/`.
2. Map the table name and property column mappings inside `OnModelCreating` in [FleetNexusDbContext.cs](file:///c:/Learn/fleet_solution/appfleet-nexus-net/api/appfleet-nexus-data/Data/FleetNexusDbContext.cs).
3. If the columns are optional in the database, ensure properties are declared as nullable (e.g. `string?`, `int?`) so the materializer handles nulls correctly.

### Database Migrations
If using EF Core migrations:
```bash
cd api/appfleet-nexus-data
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
