# Build and Run Guide - Backend API

This guide explains how to compile, configure, run, and troubleshoot the Backend API service.

## Quick Start

### Step 1: Compile the Solution
Open your terminal in the `api` root directory:
```bash
cd api
dotnet build appfleet-nexus-api.sln
```

### Step 2: Configure Environment Settings
Verify the settings in `appfleet-nexus-api/appsettings.json` (specifically the PostgreSQL ConnectionStrings value) before starting.

### Step 3: Run the API
Navigate to the API web project directory and start the service:
```bash
cd appfleet-nexus-api
dotnet run
```

Expect output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5067
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7298
```

---

## Logging
Logs are automatically written to two sinks via Serilog:
* **Console Output**: Real-time diagnostic console logging.
* **File Output**: Written rolling daily inside the `logs/` directory.

---

## Troubleshooting

### Port 5067/7298 Already in Use
If the port is occupied:
1. Identify the blocking process on Windows:
   ```cmd
   netstat -ano | findstr :5067
   ```
2. Kill the process by PID:
   ```cmd
   taskkill /PID <PID> /F
   ```
3. Alternatively, run using an explicit URL config:
   ```bash
   dotnet run --urls "http://localhost:5080"
   ```
