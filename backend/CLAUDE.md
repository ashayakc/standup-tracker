# Backend Conventions (C# / .NET 9 Minimal API)

This file contains all C#-specific conventions for the backend. See the root `CLAUDE.md` for project overview, roles, shared domain model, and cross-cutting rules.

## Conventions

- Use **Minimal API** style: `app.MapGet(...)`, `app.MapPost(...)`, etc. — no controllers
- Use **record types** for all models and DTOs: `record StandupEntry(Guid Id, string Yesterday, string Today, string? Blockers, DateTime CreatedAt);`
- **In-memory storage only**: use `ConcurrentDictionary<Guid, T>` or `IMemoryCache` — no database
- Group endpoints into static classes with `IEndpointRouteBuilder` extension methods
- Use `TypedResults` for responses
- Use `builder.Services` for DI registration
- **CORS**: must be enabled in `Program.cs` to allow requests from `http://localhost:4200`
- **API route prefix**: all endpoints under `/api/` (e.g., `/api/standups`, `/api/standups/{id}`)

## Endpoint Patterns

Endpoints are grouped into static classes with extension methods on `IEndpointRouteBuilder`:

```csharp
public static class StandupEndpoints
{
    public static IEndpointRouteBuilder MapStandupEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/standups", (StandupStore store) => { ... });
        app.MapPost("/api/standups", (CreateStandupRequest request, StandupStore store) => { ... });
        return app;
    }
}
```

## Store Patterns

- Singleton `StandupStore` registered via DI
- Uses `ConcurrentDictionary<Guid, StandupEntry>` for thread-safe in-memory storage
- No database, no EF Core, no SQLite

## Testing

- Use **xUnit** with `Microsoft.AspNetCore.Mvc.Testing` for integration tests
- **InternalsVisibleTo**: test project has access to `internal` members via `.csproj` config
- Test command: `cd backend/StandupTracker.Api.Tests && dotnet test`

## Do NOT Use

- **No EF Core** — no `Microsoft.EntityFrameworkCore` packages
- **No SQLite** — no database files or providers of any kind
- **No MVC controllers** — no `[ApiController]`, no `ControllerBase`
