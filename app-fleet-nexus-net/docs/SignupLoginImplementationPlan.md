# Implementation Plan: Signup / Login Workflow

This document outlines the high-level plan for implementing user signup, login, and token-based state management within the Blazor WebAssembly frontend (`appfleet-nexus-ui`).

## Objectives
- **Seamless User Experience**: Quick and easy signup flow with automatic login (immediate redirect to home/dashboard) upon completion.
- **State Management**: Persist JWT access/refresh tokens in browser `localStorage` and integration with Blazor's built-in authentication system.
- **Visual Design**: Sleek, glassmorphic layout using modern typography and micro-animations to match high-end design principles.
- **API Connectivity**: Inject JWT Bearer tokens automatically to outgoing HTTP API calls for secure endpoint access.

---

## User Registration Schema
The signup flow only requires the user's **First Name**, **Last Name**, **Email Address**, and **Password** (confirmed twice). Custom usernames are not supported or required by the Supabase backend authentication model.

---

## Architectural Workflow
```mermaid
sequenceDiagram
    actor User
    participant BlazorApp as Blazor WASM Client
    participant API as Backend API
    participant DB as PostgreSQL (Supabase)

    User->>BlazorApp: Fills out Signup Form
    BlazorApp->>API: POST /api/auth/signup (with Email, Password, Name)
    API->>DB: Provision auth.users + public.users + tenant
    DB-->>API: Success (User & Tenant Created)
    API-->>BlazorApp: Return AuthResponse (AccessToken, RefreshToken, User ID)
    Note over BlazorApp: Save tokens to LocalStorage
    Note over BlazorApp: Update AuthenticationStateProvider
    BlazorApp->>User: Redirect to Home (Logged In)
```

---

## Mobile-Friendly Design Guidelines
To ensure a high-fidelity mobile experience, the implementation will strictly follow these UX guidelines:
1. **Responsive Layouts**: Use Bootstrap 5 grid columns (e.g. `col-md-6 col-12`) so that inputs (like First/Last Name) stack vertically on mobile viewports but display side-by-side on larger screens.
2. **Keyboard Optimization**: Use proper attributes (`type="email"`, `inputmode="email"`, `autocapitalize="none"`, and `autocorrect="off"`) for form inputs to request standard, clean mobile keyboard layouts.
3. **Touch Target Sizing**: All input boxes and action buttons will have a height of at least `48px` to provide easy tap targets.
4. **Prevent Auto-Zooming**: Ensure standard form inputs have a `font-size` of `16px` (or `1rem`) to prevent iOS browsers from automatically zooming in when focusing fields, which disrupts the screen layout.
5. **Form Sizing Limits**: Apply `width: 100%; max-width: 480px;` and clear container padding so that the auth card scales gracefully from small mobile viewports (e.g. iPhone SE) up to desktop.
6. **Mobile Collapsible Menu Integration**: Dynamically handle navigation state and hide/show actions inside the responsive hamburger menu.

---

## Reusable Design Pattern: Multi-Step Input Wizard
For any user interaction flow that requires collecting multiple inputs (e.g. driver onboarding, vehicle profiling, contact entry, or settings configuration), FleetNexus uses a unified **Multi-Step Input Wizard Pattern** to optimize mobile and desktop usability.

### Core Pillars of the Pattern

1. **Incremental Disclosure (Wizard Flow)**
   - Distribute complex questionnaires or forms into sequential steps rather than presenting all fields at once.
   - Limit each step to **one input item** (or a single cohesive logical group) to minimize cognitive load, especially on small touch screens.

2. **No-Label Progress Tracking (Sleek Colored Dots)**
   - Use a minimalistic, horizontal step progress bar comprised of empty dots connected by thin lines.
   - Avoid cluttered step numbers or wordy step titles in the header indicator.
   - Dots dynamically update:
     - **Active**: Brand highlight color (e.g. blue), scaled larger, with a soft glow ring.
     - **Completed**: Success color (e.g. green).
     - **Pending**: Neutral inactive grey.

3. **Unified Simplified Form Component**
   - Bind inputs using a clean, reusable wrapper component containing exactly:
     - **Title**: Action prompt (e.g. *"What is your email address?"*).
     - **Label Description**: Subtitle explaining context (e.g. *"We will use this address to log you in."*).
     - **Single Input Field**: Auto-focused, styled text input.
     - **Dynamic Error Placement**: Inline validation warnings directly below the control.

4. **Progressive validation**
   - Validate only the current step's active properties before advancing.
   - Prevent the user from proceeding if the active inputs fail model validation rules.

5. **Adaptive Navigation Controls**
   - Provide explicit **Previous** and **Next** buttons at the bottom of the card:
     - The **Previous** button is disabled on Step 1.
     - The **Next** button dynamically validates the active properties.
     - On the final step, the **Next** button text transforms into **Submit**.
   - Listen for "Enter" keyups on inputs to automatically trigger the next action or final submission.

6. **Automatic Focus Management**
   - When entering or returning to a step, automatically focus the primary input field of that step to enable fast typing.

---

## Implementation Phases

### Phase 1: Authentication Infrastructure
- Add NuGet package `Microsoft.AspNetCore.Components.Authorization` to the client project.
- Create `CustomAuthenticationStateProvider` inheriting from standard `AuthenticationStateProvider` to manage and parse tokens.
- Add client-side validation models for login and signup.
- Register services in `Program.cs`.

### Phase 2: Shell Layout & Navigation
- Wrap the main application in `App.razor` with `CascadingAuthenticationState`.
- Update `NavMenu.razor` to dynamically show/hide routes depending on authorization state.
- Create a header bar in `MainLayout.razor` displaying the logged-in user profile or quick actions.

### Phase 3: Login Page (`/login`)
- Implement the Login UI using a glassmorphic card design.
- Handle forms submission and error display.

### Phase 4: Signup Page (`/signup`) — 5-Step Wizard
- Implement a 5-step Signup Wizard (Step 1: Email, Step 2: First Name, Step 3: Last Name, Step 4: Password, Step 5: Reenter Password) using the simplified `AuthInput` component (Label + Input Box + Title).
- Display visual step dots progress indicators, intermediate validation per step, and autofocus transitions.
- Render "Previous" and "Next" buttons, where "Previous" is disabled at Step 1, and "Next" changes to "Submit" in the final Step 5.
- Support auto-login on successful registration.

### Phase 5: Verification & End-to-End Testing
- Ensure proper token handling, token expiration logic, UI responsiveness, and correct redirection.
