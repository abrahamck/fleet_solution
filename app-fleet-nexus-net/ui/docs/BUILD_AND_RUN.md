# Build and Run Guide - Frontend UI

This guide explains how to compile, configure, run, and troubleshoot the Blazor WebAssembly frontend client.

## Quick Start

### Step 1: Compile the Solution
Open your terminal in the `ui` root directory:
```bash
cd ui
dotnet build appfleet-nexus-ui.sln
```

### Step 2: Configure Client API Endpoints
Make sure that the backend API URL inside `appfleet-nexus-ui/wwwroot/appsettings.json` matches your backend's active port (default `http://localhost:5067`).

### Step 3: Run the App
Navigate to the Blazor project directory and start the service:
```bash
cd appfleet-nexus-ui
dotnet run
```

Expect output similar to:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5297
```

Open a web browser and navigate to the address shown in the output to access the client interface.

---

## Troubleshooting

### CORS Connection Refused
If the UI displays an error failing to fetch carriers:
1. Ensure the Backend API is fully running.
2. Confirm the host address config in `wwwroot/appsettings.json` matches the backend endpoint exactly.
3. Open the browser dev tools (F12 console tab) to check for CORS block issues.
