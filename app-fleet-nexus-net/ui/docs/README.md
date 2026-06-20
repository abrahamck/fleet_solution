# FleetNexus UI Documentation

This directory contains the documentation for the FleetNexus frontend client, built using Blazor WebAssembly on .NET 10.

## Architecture

* **Framework**: Blazor WebAssembly (.NET 10)
* **Styling**: Bootstrap 5 + Bootstrap Icons
* **Rendering Mode**: Interactive WebAssembly (Client-side)
* **API Communication**: Type-safe HTTP calls utilizing JSON DTOs

---

## Configuration

The API URL configuration lives inside `/ui/appfleet-nexus-ui/wwwroot/appsettings.json`.

```json
{
  "ApiBaseUrl": "http://localhost:5067"
}
```

Ensure this points to the active port of your running Backend API service.

---

## UI Pages

### 1. Home Page (`/`)
* Displays landing page layout introducing the FleetNexus system features.
* Action button routing visitors to the carriers analysis page.

### 2. Top Carriers Page (`/carriers`)
* Fetches the top 100 carriers sorted by number of power units.
* Built-in error alert with a "Try Again" reload action.
* Displays a table containing DOT Number, Legal Name, City, State, and Power Units count badges.

---

## Development Notes

### Creating New Pages
1. Add new `.razor` component file inside `/ui/appfleet-nexus-ui/Pages/`.
2. Declare path route at the top (e.g. `@page "/my-new-page"`).
3. If referencing API responses, define corresponding data DTOs at the bottom of the razor file or in a shared class library.
4. Add corresponding links within the NavMenu layout (`/ui/appfleet-nexus-ui/Layout/NavMenu.razor`).
