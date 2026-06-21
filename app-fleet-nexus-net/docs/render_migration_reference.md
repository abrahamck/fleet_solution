# Node.js to .NET Render Migration: Reference Document

This document captures the design choices made, issues encountered, and resolutions applied while migrating the `fleet_solution` application from a Node.js backend to a .NET 10 architecture on Render.com. 

---

## 🏗️ Design Choices

### 1. Dockerizing Both API and UI Separately
* **Context**: Render's "Hobby Plan" has a limit of 750 free Web Service hours per month. We initially considered deploying the Blazor UI as a Free Static Site (which doesn't consume hours) and only the API as a Docker Web Service.
* **Decision**: We opted to Dockerize **both** the API and the UI as separate Web Services.
* **Reasoning**: You explicitly noted that the project is still in development with very low traffic. Fully containerizing both applications provides a cleaner, production-ready environment that perfectly mirrors industry-standard microservice deployments, prioritizing architectural cleanliness over immediate free-tier micro-optimizations.

### 2. Serving the Blazor WebAssembly UI via Nginx
* **Context**: A Blazor WebAssembly app can be served by an ASP.NET Core backend or as pure static files.
* **Decision**: We used a multi-stage Dockerfile that builds the .NET app, but uses `nginx:alpine` as the final base image to serve the compiled `wwwroot` files.
* **Reasoning**: This keeps the UI container incredibly lightweight and decoupled from the API. The final container image doesn't require the .NET runtime to be installed, optimizing memory usage and startup time on Render.

### 3. Environment-Specific Configuration Pattern
* **Context**: The UI code (`Carriers.razor` and `Program.cs`) contained a hardcoded local API URL (`http://localhost:5067`).
* **Decision**: We removed the hardcoded URL, configured `HttpClient` to read `ApiBaseUrl` dynamically from the configuration, and established an `appsettings.json` (containing the Render production URL) alongside an `appsettings.Development.json` (containing the localhost URL).
* **Reasoning**: This allows the exact same Docker image to function securely in production while letting local `dotnet run` commands naturally fall back to local development servers without requiring any manual code changes before commits.

### 4. Database Schema Management Strategy (EF Core Migrations)
* **Context**: We needed a mechanism to track, deploy, and version database tables (like `Tenants`, `Users`, `Vehicles`) consistently across local and production Supabase databases.
* **Decision**: We chose **Entity Framework (EF) Core Code-First Migrations**, embedding the `db.Database.Migrate()` command directly into the API's `Program.cs` startup pipeline.
* **Reasoning**: 
  - **Single Source of Truth**: The C# code acts as the ultimate authority. Changing a model in C# instantly produces the matching SQL migration.
  - **Zero-Touch Production Deployments**: By adding the migration logic to `Program.cs`, Render handles database updates automatically on every commit without needing complex GitHub Actions CI/CD pipelines.
* **Alternatives Considered & Rejected**:
  - **Manual Supabase UI Editing**: We rejected this because it completely breaks source control. Creating tables manually in the UI leads to "drift" where local code and production tables don't match, causing silent crashes.
  - **Raw SQL Scripts (e.g., DbUp, Flyway)**: We rejected this because it forces developers to write raw PostgreSQL scripts *and* write the C# models, duplicating effort. EF Core handles both simultaneously.
  - **Pre-Deploy Scripts in Render**: We rejected this because Render's free/hobby tier handles simple web services better than complex multi-stage deployment scripts, making `Program.cs` execution cleaner and faster.

---

## 🔧 Issues & Resolutions

### Issue 1: Database Initialization String Error
* **Symptom**: After deploying the API, accessing endpoints returned an HTTP 500 error: `Format of the initialization string does not conform to specification starting at index 0`.
* **Root Cause**: The `.NET` `Npgsql` database driver natively expects an ADO.NET formatted connection string (e.g., `Host=...;Password=...;`), but the environment variable in Render was populated with a Node.js "URI" style connection string (e.g., `postgresql://...`).
* **Resolution**: Replaced the Render environment variable `ConnectionStrings__PostgreSQL` with the dedicated `.NET` formatted connection string obtained from the Supabase dashboard.

### Issue 2: Nginx Returning 404 for UI Assets (.wasm / .json)
* **Symptom**: The Blazor UI loaded the HTML progress bar, but failed to load entirely because the browser Network tab showed HTTP 404 errors for all `.wasm` files and `appsettings.json`.
* **Root Cause**: Starting in .NET 8, `dotnet publish` aggressively compresses static assets into `.gz` (Gzip) and `.br` (Brotli) formats, omitting the uncompressed files entirely to save space. Nginx, by default, did not know to look for these `.gz` versions and failed to find the requested uncompressed files. Additionally, the Nginx `try_files` rule ended with `=404`, which breaks SPA client-side routing.
* **Resolution**: Updated `ui/nginx.conf` by adding `gzip_static on;`, instructing Nginx to seamlessly serve the pre-compressed `.gz` files. We also removed the `=404` fallback, ensuring Nginx redirects unknown routes to `index.html` so Blazor can process them.

### Issue 3: Persistent UI Errors Due to Browser Caching
* **Symptom**: After applying the Nginx fix, the UI worked perfectly in Private Browsing, but the standard browser window continued to fail.
* **Root Cause**: Blazor WebAssembly relies heavily on browser caching (via Cache API/Service Workers) to ensure fast load times. The browser had aggressively cached the failed state and broken files from the initial unsuccessful deployment.
* **Resolution**: Performed an "Empty Cache and Hard Reload" via the browser's Developer Tools (and optionally cleared site data) to wipe the slate clean and force a fresh fetch from Render.

### Issue 4: Migrating the Custom Domain
* **Symptom**: Needing to transition a Hostinger-purchased domain from the old Node.js service to the new .NET UI.
* **Root Cause**: Render enforces a 1:1 mapping between domains and services.
* **Resolution**: Removed the domain from the old Node.js Render settings and attached it to the new .NET UI settings. Clarified that Hostinger DNS updates are only necessary if using a `CNAME` for a subdomain, as Root Domains rely on Render's static Anycast IP which remains identical across services.

### Issue 5: EF Core Migrations Failing on Pre-existing Tables
* **Symptom**: Running `dotnet ef database update` locally threw a PostgreSQL error: `42P07: relation "fmcsa_census" already exists`.
* **Root Cause**: Because this was the very first EF Core migration, the tool assumed the database was completely empty and generated a `CREATE TABLE` script for the legacy `fmcsa_census` table. However, since the database was previously managed externally and already contained this table, PostgreSQL rejected the creation attempt.
* **Resolution**: Manually opened the generated `[Timestamp]_MultiTenantSecurityModel.cs` migration file and deleted the `migrationBuilder.CreateTable` code block for `fmcsa_census`. This instructs EF Core to leave the pre-existing table untouched and focus exclusively on generating the new application-specific tables (`Tenants`, `Users`, etc.).

### Issue 6: EF Core Migrations Failing (Missing CLI Tool & Design Package)
* **Symptom**: Attempting to run `dotnet ef migrations add` returned a `command not found` error.
* **Root Cause**: Two components were missing: The `dotnet-ef` CLI tool was not installed on the system, and the API project lacked the `Microsoft.EntityFrameworkCore.Design` NuGet package required to execute design-time scaffolding.
* **Resolution**: The `dotnet-ef` tool was installed globally via bash (`dotnet tool install --global dotnet-ef`). Then, the `Design` package was added to `appfleet-nexus-api.csproj` (`dotnet add package Microsoft.EntityFrameworkCore.Design`) to enable the command to compile and generate the migrations.

---

## 🔒 Environment Secrets on Render

For production deployments, the following environment variables **must** be configured on Render for the API service:

| Variable Name | Appsettings Path | Purpose | Format / Value |
| --- | --- | --- | --- |
| `ConnectionStrings__PostgreSQL` | `ConnectionStrings:PostgreSQL` | Connection string to Supabase PostgreSQL DB | ADO.NET Key-Value pair format |
| `Supabase__Url` | `Supabase:Url` | URL of your Supabase project instance | `https://<your-project>.supabase.co` |
| `Supabase__AnonKey` | `Supabase:AnonKey` | Public API Anonymous Key | Supabase Anon Key JWT |
| `Supabase__ServiceRoleKey` | `Supabase:ServiceRoleKey` | SuperUser service role key (bypasses RLS) | Supabase Service Role Key JWT |
| `Supabase__JwtSecret` | `Supabase:JwtSecret` | JWT signing secret used for token validation | Supabase JWT Secret (HS256) |

These environment variables will override the default placeholder values in `appsettings.json` during hosting execution.
