# ADR-019: State-Aware Dual-Mode Root Routing (`/`)

* **Status**: Accepted
* **Date**: 2026-06-22

---

## Context & Problem Statement
The application root URL (`/`) needs to serve two fundamentally distinct user contexts: anonymous visitors who must see the public product introduction/waitlist onboarding wizard, and authenticated operators who need immediate access to their operational KPI dashboard.

---

## Decision
Implement **state-aware rendering** in `Home.razor` using Blazor's `<AuthorizeView>` component:
* `<NotAuthorized>`: Renders the 4-step Alpha Roadblock waitlist signup wizard.
* `<Authorized>`: Fetches and displays the interactive fleet KPI Dashboard.

---

## Alternatives Considered
* **Separate Route Paths**: Hosting the landing page at `/` and redirecting authenticated users to `/dashboard`.
* **Server-Side Route Rewriting / Middleware Redirection**: Detecting cookies/tokens at the web server layer to route requests.

---

## Consequences & Trade-offs
* **Positive Impacts**:
  * Unified, clean home URL (`/`) without complex redirect loops or route fragmentation.
  * Instantaneous client-side UI switching upon login or logout without full page reloads.
* **Risks & Trade-offs**: `Home.razor` contains both landing page layout and dashboard layout logic.
* **Migration Paths**: Not Available.
