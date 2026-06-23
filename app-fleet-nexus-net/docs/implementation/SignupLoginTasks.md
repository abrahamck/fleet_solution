# Detailed Implementation Tasks: Signup / Login Workflow

This document provides step-by-step task specifications for implementing the Signup/Login workflow in the `appfleet-nexus-ui` Blazor WebAssembly project.

> [!NOTE]
> This implementation establishes the **Multi-Step Input Wizard Pattern** detailed in [SignupLoginImplementationPlan.md](file:///c:/Learn/fleet_solution/app-fleet-nexus-net/docs/SignupLoginImplementationPlan.md). This pattern must be followed for any feature involving multiple input items.

---

## Checklist & Progress Tracker

- [x] **Phase 1: Dependencies & Authentication Abstractions**
  - [x] Add `Microsoft.AspNetCore.Components.Authorization` package to `appfleet-nexus-ui.csproj`.
  - [x] Create `UserSession` model and token validation models in a new `Models/AuthModels.cs` file.
  - [x] Create `CustomAuthenticationStateProvider.cs` in `Authentication/` to handle token storage and parsing.
  - [x] Create reusable component `Components/AuthInput.razor` to encapsulate inputs, icons, and error states.
  - [x] Register authentication services in `Program.cs`.
  - [x] Add global `@using` statements in `_Imports.razor`.

- [x] **Phase 2: UI & Shell Layout Integration**
  - [x] Import Google Fonts (Inter) and Bootstrap Icons in `index.html`.
  - [x] Update `App.razor` to wrap the router with `<CascadingAuthenticationState>` and use `<AuthorizeRouteView>`.
  - [x] Add modern glassmorphic, interactive form styling, validation states, and spinner classes to `app.css`.
  - [x] Update `MainLayout.razor` and `MainLayout.razor.css` to show/hide user state (Email / Logout button) in a top header.
  - [x] Update `NavMenu.razor` and `NavMenu.razor.css` to conditionally render menus based on user auth status.

- [x] **Phase 3: Login Screen (`/login`)**
  - [x] Create `Pages/Login.razor`.
  - [x] Design a glassmorphic login card with email and password fields.
  - [x] Connect the login form to the `api/auth/login` endpoint.
  - [x] Store session tokens and redirect to `/` on success.

- [x] **Phase 4: Signup Screen (`/signup`) — 5-Step Wizard**
  - [x] Create `Pages/Signup.razor` with 5-step wizard state management.
  - [x] Design a progress tracker with step dots for 5 steps.
  - [x] Integrate simplified `AuthInput` component with Label, Input Box, and Title.
  - [x] Enable dynamic step-by-step validation via DataAnnotations.
  - [x] Implement input autofocus transitions and previous/next navigation buttons (Next turns to Submit on Step 5, Previous disabled on Step 1).
  - [x] Automatically log in the user on successful signup and redirect to `/`.

- [x] **Phase 5: Verification & Testing**
  - [x] Compile and run both API and UI projects.
  - [x] Verify form validation UI alerts.
  - [x] Validate token persistence in localStorage and auto-append token to subsequent requests.

---

## Detailed Task Details

### 1. NuGet Packages & Project configuration
- Project file: `ui/appfleet-nexus-ui/appfleet-nexus-ui.csproj`
- Add:
  ```xml
  <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="10.0.9" />
  ```
- Run `dotnet restore` to fetch the package.

### 2. Authentication State Provider & Session Models
- Create folder `ui/appfleet-nexus-ui/Authentication/`.
- File: `ui/appfleet-nexus-ui/Authentication/CustomAuthenticationStateProvider.cs`
- Logic details:
  - Inherit from `AuthenticationStateProvider`.
  - Inject `IJSRuntime` and `HttpClient`.
  - Store session in local storage under the key `"auth_session"`.
  - Define `UserSession` record:
    ```csharp
    public record UserSession(string AccessToken, string RefreshToken, string Email, Guid UserId);
    ```
  - Parse the JWT claims in a helper method (`ParseClaimsFromJwt`) without adding token libraries.
  - Apply `HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token)` when user is authenticated, and set to `null` on logout.

### 3. Service Registration
- File: `ui/appfleet-nexus-ui/Program.cs`
- Add:
  ```csharp
  builder.Services.AddAuthorizationCore();
  builder.Services.AddScoped<CustomAuthenticationStateProvider>();
  builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
  ```

### 4. Layout & Styles
- **`index.html`**:
  - Add `<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css">`
  - Add Google Fonts `<link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet">`
- **`app.css`**:
  - Add classes for `.auth-container` (flex centering, responsive padding).
  - Add `.auth-card` (glassmorphism effect, `width: 100%`, `max-width: 480px`).
  - Add `.auth-input` (height of `48px` minimum, `font-size: 16px` to prevent iOS zoom, smooth focus transitions).
  - Add `.auth-btn` (height of `48px` minimum, touch feedback transitions, loading state spinner support).
- **`MainLayout.razor`**:
  - Add header wrapper with `<AuthorizeView>` to display the logged in user's profile dropdown/email and a Logout button.
- **`NavMenu.razor`**:
  - Render core features only inside `<AuthorizeView>` or render specific auth pages for guests. Ensure mobile sidebar toggle closes when selecting links.

### 5. Login Page (`Pages/Login.razor`)
- Route: `@page "/login"`
- Form Model: `LoginModel`
- Fields:
  - Email: `type="email"`, `inputmode="email"`, `autocapitalize="none"`, `autocorrect="off"`.
  - Password: `type="password"`.
- Submits `LoginRequest` via JSON POST to `/api/auth/login`.
- Triggers `CustomAuthenticationStateProvider.LoginAsync(...)` and navigates to `/`.

### 6. Signup Page (`Pages/Signup.razor`)
- Route: `@page "/signup"`
- Form Model: `SignupModel`
- 5-Step Wizard Layout:
  - **Step 1: Email**: captures and validates Email (autofocused, email type/inputmode).
  - **Step 2: First Name**: captures and validates First Name.
  - **Step 3: Last Name**: captures and validates Last Name.
  - **Step 4: Password**: captures and validates Password.
  - **Step 5: Reenter Password**: captures and validates Confirm Password.
- Navigation Rules:
  - "Previous" button goes back one step, disabled on Step 1.
  - "Next" button goes forward one step, validated dynamically on click.
  - "Next" changes to "Submit" on Step 5, triggering registration call and auto-login on success.
- Dynamic Validation: uses standard DataAnnotations on each step change to prevent forwarding if inputs are invalid.
- Focus Transitions: automatically triggers focus calls to the active step's input box.
- Submits `SignupRequest` to `/api/auth/signup`.
- Triggers `CustomAuthenticationStateProvider.LoginAsync(...)` using returned tokens and redirects to `/`.
