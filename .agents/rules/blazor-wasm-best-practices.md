# Blazor WASM — High-Signal Developer Rules

> Core rules derived from actual production and CI/CD issues in FleetNexus. 
> Apply to all Razor components and project configurations in this workspace.

---

## 1. Always wrap modals, dialogs, and nested child components in `@if`

**Rule:** Declaring a child component tag (e.g. `<VehicleDialog ... />`) unconditionally in Razor markup forces Blazor to instantiate that component eagerly in the `RenderTree`, even if internal HTML elements are hidden via `IsVisible="false"`. If components cross-reference each other (e.g. VehicleDialog embeds ContactDialog and vice versa), unconditional declarations trigger an **infinite circular component tree instantiation loop**, crashing the Mono WebAssembly runtime at render time (`Assertion: should not be reached at class-accessors.c:93`).

**One-liner:** _Never declare child dialogs or modals unconditionally — always wrap them in `@if (isOpen && !isChildInlineMode)` so they are instantiated only when active._

```razor
<!-- ❌ WRONG: Instantiates ContactDialog eagerly on parent render; causes infinite recursion if ContactDialog references VehicleDialog -->
<ContactDialog IsVisible="_showInlineContactModal" IsInlineMode="true" />

<!-- ✅ CORRECT: Component is only instantiated when active and guarded against recursive inline instantiation -->
@if (!IsInlineMode && _showInlineContactModal)
{
    <ContactDialog IsVisible="_showInlineContactModal"
                   IsInlineMode="true"
                   OnSave="HandleInlineContactSaved"
                   OnCancel="CloseInlineContactDialog" />
}
```

---

## 2. Do not set `Emcc*` or native compiler flags in the `.csproj`

**Rule:** In modern .NET (8, 9, 10), Blazor WebAssembly dynamically grows memory at runtime up to the browser limit (2GB+). Setting `EmccMaximumHeapSize` or other Emscripten flags activates `WasmBuildNative`, requiring the `wasm-tools` workload and Python 3 in the build environment. This breaks minimal Docker SDK container builds and inflates build times from seconds to several minutes. Always rely on the default precompiled runtime pack.

**One-liner:** _Never set `Emcc*` properties in Blazor WASM `.csproj` files — the runtime dynamically expands memory, and native compiler flags break CI/CD Docker builds._

```xml
<!-- ❌ WRONG: Triggers Emscripten native compilation pipeline in CI/CD -->
<EmccMaximumHeapSize>536870912</EmccMaximumHeapSize>

<!-- ✅ CORRECT: Keep standard PropertyGroup; runtime expands memory automatically -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

---

## 3. Implement `IDisposable` ONLY to unsubscribe from external service events

**Rule:** Component fields (such as `List<T>`, local strings, or view models) are automatically garbage-collected when the component is unmounted. Calling `.Clear()` or `null`-ing fields in `Dispose()` is useless boilerplate. However, if a component subscribes to C# events on an injected scoped/singleton service (e.g. `AppState.OnChange += HandleChange` or `NavigationManager.LocationChanged += HandleLocation`), the service holds a strong reference to the component. You **must** implement `IDisposable` and unsubscribe with `-=` to prevent memory leaks.

**One-liner:** _Only implement `IDisposable` when subscribing to events on injected services — never add empty `Dispose()` methods to clear local collection fields._

```razor
@implements IDisposable
@inject AppState State

@code {
    protected override void OnInitialized()
    {
        State.OnChange += StateHasChanged;
    }

    public void Dispose()
    {
        // ✅ Essential: breaks the strong reference from the service to this component
        State.OnChange -= StateHasChanged;
    }
}
```

