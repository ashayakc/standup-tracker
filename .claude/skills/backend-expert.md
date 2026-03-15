Act as the Backend Developer defined in CLAUDE.md. You are a C# .NET 9 Minimal API expert for this project. When consulted, apply the patterns below. Work only in `/backend`.

## Minimal API Endpoint Pattern

### Endpoint group structure
Endpoints live in static classes under `Endpoints/` with an `IEndpointRouteBuilder` extension method. Group related routes under a common prefix with `MapGroup`.

```csharp
using StandupTracker.Api.Models;
using StandupTracker.Api.Services;

namespace StandupTracker.Api.Endpoints;

public static class FeatureEndpoints
{
    public static IEndpointRouteBuilder MapFeatureEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/features").WithTags("Features");

        // GET — list all
        group.MapGet("/", (FeatureStore store) =>
            TypedResults.Ok(store.GetAll()));

        // GET — by id
        group.MapGet("/{id:guid}", (Guid id, FeatureStore store) =>
        {
            var item = store.GetById(id);
            return item is not null
                ? Results.Ok(item)
                : Results.NotFound();
        });

        // POST — create
        group.MapPost("/", (CreateFeatureRequest request, FeatureStore store) =>
        {
            // Validate first (see Validation section below)
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest("Name is required.");

            var item = store.Add(request);
            return Results.Created($"/api/features/{item.Id}", item);
        });

        // PATCH — partial update
        group.MapPatch("/{id:guid}/complete", (Guid id, FeatureStore store) =>
        {
            var updated = store.Complete(id);
            return updated is not null
                ? Results.Ok(updated)
                : Results.NotFound();
        });

        return app;
    }
}
```

### Registering in Program.cs
```csharp
// Program.cs — keep it minimal
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FeatureStore>();
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapFeatureEndpoints();  // one line per endpoint group

app.Run();

public partial class Program { }  // required for WebApplicationFactory in tests
```

### Key rules
- Use `TypedResults.Ok()` for successful typed responses
- Use `Results.BadRequest()`, `Results.NotFound()`, `Results.Created()` for status codes
- Inject services directly as endpoint parameters — no constructor injection
- Route prefix: always under `/api/`
- Place static routes (`/weekly-summary`) before parameterized routes (`/{id:guid}`) to avoid conflicts

## In-Memory Store Thread Safety

### ConcurrentDictionary as the single source of truth
Every store is a singleton registered with `AddSingleton<T>()`, backed by `ConcurrentDictionary<Guid, T>`.

```csharp
public class FeatureStore
{
    private readonly ConcurrentDictionary<Guid, Feature> _items = new();

    // READ — snapshot to list, safe for concurrent reads
    public IReadOnlyList<Feature> GetAll()
    {
        return _items.Values
            .OrderByDescending(e => e.CreatedAt)
            .ToList();  // snapshot — safe to enumerate
    }

    // READ — single item
    public Feature? GetById(Guid id)
    {
        return _items.GetValueOrDefault(id);
    }

    // WRITE — add new entry
    public Feature Add(CreateFeatureRequest request)
    {
        var item = new Feature(
            Guid.NewGuid(),
            request.Name,
            false,
            DateTime.UtcNow);

        _items[item.Id] = item;  // indexer is atomic
        return item;
    }

    // WRITE — update via `with` expression (records are immutable)
    public Feature? Complete(Guid id)
    {
        if (!_items.TryGetValue(id, out var item))
            return null;

        var updated = item with { IsComplete = true };
        _items[id] = updated;  // atomic replace
        return updated;
    }
}
```

### Thread safety rules
| Operation | Approach |
|-----------|----------|
| Read all | `.Values.ToList()` — creates a snapshot, safe to enumerate |
| Read one | `TryGetValue` or `GetValueOrDefault` — atomic |
| Add | Indexer `_items[id] = value` — atomic |
| Update | `TryGetValue` → `with` expression → indexer replace — safe for this app's concurrency level |
| Delete | `TryRemove(id, out _)` — atomic |

### What NOT to do
- Never use `Dictionary<K,V>` — not thread-safe
- Never use `IMemoryCache` for primary storage — use it only for computed/cached views if needed
- Never lock around ConcurrentDictionary — it handles its own locking
- Never mutate a record in place — always use `with` to create a new instance

### Internal test helpers
Use `internal` methods with `InternalsVisibleTo` for test-only methods that need to control state (e.g., setting `CreatedAt`):

```csharp
// In store — internal, not exposed to API
internal Feature AddWithDate(CreateFeatureRequest request, DateTime createdAt)
{
    var item = new Feature(Guid.NewGuid(), request.Name, false, createdAt);
    _items[item.Id] = item;
    return item;
}
```

```xml
<!-- In .csproj -->
<ItemGroup>
  <InternalsVisibleTo Include="StandupTracker.Api.Tests" />
</ItemGroup>
```

## Record Type Conventions

### Domain model — represents stored data
```csharp
namespace StandupTracker.Api.Models;

// One record per file, positional syntax, nullable where optional
public record StandupEntry(
    Guid Id,
    string Yesterday,
    string Today,
    string? Blockers,        // nullable = optional field
    bool BlockerResolved,
    DateTime CreatedAt);
```

### Request DTO — what the client sends
```csharp
// Only the fields the client provides — no Id, no CreatedAt
public record CreateStandupRequest(
    string Yesterday,
    string Today,
    string? Blockers);
```

### Aggregate/summary DTO — computed response
```csharp
// Contains computed fields + nested collections
public record WeeklySummary(
    DateTime WeekStart,
    DateTime WeekEnd,
    int StandupCount,
    int BlockersRaised,
    int BlockersResolved,
    double ResolutionRate,
    IReadOnlyList<StandupEntry> Entries);  // use IReadOnlyList for collections
```

### Record conventions
| Rule | Example |
|------|---------|
| PascalCase for type and properties | `StandupEntry`, `BlockerResolved` |
| Positional syntax (constructor params) | `record Foo(string Bar, int Baz);` |
| Nullable reference for optional fields | `string? Blockers` |
| `IReadOnlyList<T>` for collections | not `List<T>` in the record signature |
| One record per `.cs` file | `Models/StandupEntry.cs` |
| Use `with` for immutable updates | `entry with { BlockerResolved = true }` |
| No classes for models | records only — never `class StandupEntry` |

## Error Response Shape

This project uses simple string error messages via built-in `Results` methods. No custom error envelope.

### Patterns by status code
```csharp
// 400 Bad Request — validation failure
return Results.BadRequest("Yesterday and Today are required.");

// 404 Not Found — entity doesn't exist
return Results.NotFound();

// 201 Created — successful creation with location header
return Results.Created($"/api/standups/{entry.Id}", entry);

// 200 OK — successful read/update
return Results.Ok(data);
return TypedResults.Ok(data);  // preferred for typed responses
```

### What the frontend receives
```
400 → response body: "Yesterday and Today are required." (plain string)
404 → response body: empty
201 → response body: the created entity as JSON, Location header set
200 → response body: the entity or array as JSON
```

### Rules
- Return plain string messages for `BadRequest` — no custom error object
- Return the entity in the body for `Created` and `Ok`
- Return empty body for `NotFound`
- Use `TypedResults` for success responses (enables OpenAPI metadata)
- Use `Results` for error responses (allows mixed return types)

## Validation Pattern (No FluentValidation)

### Inline validation in endpoints
Validate at the endpoint level before calling the store. Keep it simple — guard clauses with early return.

```csharp
group.MapPost("/", (CreateStandupRequest request, StandupStore store) =>
{
    // Required string fields — check null, empty, whitespace
    if (string.IsNullOrWhiteSpace(request.Yesterday) || string.IsNullOrWhiteSpace(request.Today))
        return Results.BadRequest("Yesterday and Today are required.");

    var entry = store.Add(request);
    return Results.Created($"/api/standups/{entry.Id}", entry);
});
```

### Validation helper for multiple fields
When validation grows beyond 2-3 checks, extract to a static method in the same endpoint class:

```csharp
group.MapPost("/", (CreateFeatureRequest request, FeatureStore store) =>
{
    var error = ValidateCreateRequest(request);
    if (error is not null)
        return Results.BadRequest(error);

    var item = store.Add(request);
    return Results.Created($"/api/features/{item.Id}", item);
});

// Private static helper — stays in the endpoint class
private static string? ValidateCreateRequest(CreateFeatureRequest request)
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return "Name is required.";

    if (request.Name.Length > 200)
        return "Name must be 200 characters or fewer.";

    return null;  // valid
}
```

### Store-level guard clauses
The store validates business rules that depend on existing state:

```csharp
public StandupEntry? ResolveBlocker(Guid id)
{
    // Guard: entity must exist
    if (!_entries.TryGetValue(id, out var entry))
        return null;

    // Guard: business rule — can't resolve empty blocker
    if (string.IsNullOrWhiteSpace(entry.Blockers))
        return null;

    var updated = entry with { BlockerResolved = true };
    _entries[id] = updated;
    return updated;
}
```

### Validation rules
| Layer | Validates | Returns on failure |
|-------|-----------|-------------------|
| Endpoint | Input shape: required fields, format, length | `Results.BadRequest("message")` |
| Store | Business rules: existence, state transitions | `null` (endpoint maps to `NotFound`) |

### What NOT to do
- No FluentValidation — keep validation inline
- No `[Required]` attributes — this is Minimal API, not MVC
- No custom exception types — use return values, not exceptions
- No middleware-based validation — validate in the endpoint handler
- No `DataAnnotations` — no `[StringLength]`, `[Range]`, etc.
