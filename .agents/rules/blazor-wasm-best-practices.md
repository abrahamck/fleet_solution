# Blazor WASM — Developer Best Practices

> Rules derived from production issues observed in FleetNexus. Apply to every
> Razor component and WASM project setting touched in this workspace.

---

## 1. Always implement `IDisposable` on components that hold state

**Rule:** Any Razor component that stores a `List<T>`, subscribes to events, or
injects a scoped service **must** implement `IDisposable` and clear its
references in `Dispose()`.

**One-liner:** _If a component has fields, it needs `@implements IDisposable` and a `Dispose()` that clears them — otherwise navigating away leaks memory permanently into the WASM heap._

```razor
@implements IDisposable

@code {
    private List<VehicleDto> _vehicles = new();

    public void Dispose()
    {
        _vehicles.Clear();
    }
}
```

---

## 2. Never use a computed property for filtered/sorted lists

**Rule:** `private IEnumerable<T> Filtered => _list.Where(...)` re-runs LINQ on
**every render**. For any list used in a loop (`@foreach`), cache the result
in a backing field and refresh it only when filters or the source list change.

**One-liner:** _A LINQ property on a component is a hidden render-time allocation bomb — always cache filtered results in a `List<T>` field and call `Refresh()` explicitly._

```csharp
// ❌ Runs on every StateHasChanged()
private IEnumerable<VehicleDto> FilteredVehicles => _vehicles.Where(...);

// ✅ Recalculated only when data or filters change
private List<VehicleDto> _filteredVehicles = new();

private void RefreshFilteredVehicles()
{
    _filteredVehicles = _vehicles.Where(...).ToList();
}
```

---

## 3. Avoid setting `Emcc*` or native build flags unless AOT is explicitly required

**Rule:** In modern .NET (8, 9, 10), Blazor WebAssembly dynamically grows memory
at runtime up to the browser limit. Setting `EmccMaximumHeapSize` or other native
compiler flags triggers `WasmBuildNative`, requiring the `wasm-tools` workload and
Python 3 in the build environment, which causes CI/CD container build failures and
exponentially slower compile times. Rely on the default runtime pack.

**One-liner:** _Do not set `Emcc*` properties in Blazor WASM csproj files — the default runtime dynamically expands memory, and custom Emscripten flags force unnecessary native re-linking and break CI builds._

---

## 4. Self-host third-party fonts and icon CSS (no CDN storage access)

**Rule:** CDN-loaded CSS that attempts browser storage access (cookies,
localStorage) is blocked by tracking-prevention in Edge and Firefox, producing
console noise in prod. Prefer `libman.json` to pull assets into
`wwwroot/lib/` at build time.

**One-liner:** _Anything from jsDelivr/unpkg that a browser's tracking prevention can block should be self-hosted via LibMan, not hot-linked from a CDN._

---

## 5. Split large components before they exceed ~300 lines

**Rule:** Components over 300 lines of Razor markup (excluding `@code`) are
candidates for extraction. Large single-file components make it easy to
accumulate state leaks and hide render-performance issues.

**One-liner:** _A 1000-line Razor file is a memory-leak and review-debt waiting to happen — extract tabs, dialogs, and cards into child components with their own scoped state and lifecycle._

---

## 6. Use `Task.WhenAll` correctly — await, then `.Result`

**Rule:** Accessing `.Result` on a `Task` returned from `Task.WhenAll` is safe
only after the outer `await` has completed. Accessing `.Result` before `await`
will deadlock in synchronous contexts and obscures intent.

**One-liner:** _After `await Task.WhenAll(taskA, taskB)`, reading `taskA.Result` is fine — but never skip the `await` and go straight to `.Result`._

```csharp
// ✅ Correct — tasks are guaranteed complete before .Result
await Task.WhenAll(listTask, kpiTask);
if (listTask.Result != null) _contacts = listTask.Result;
```
