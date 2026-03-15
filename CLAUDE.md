# Standup Tracker

Daily standup tracker — log yesterday's work, today's plan, and blockers.

## Project Structure

```
standup-tracker/
  backend/
    StandupTracker.Api/        Main API project
      Models/                  Record types for domain models and DTOs
      Endpoints/               Static classes with endpoint route mappings
      Services/                In-memory data store and business logic
      Program.cs               App entry point and middleware config
    StandupTracker.Api.Tests/  xUnit test project for the API
  frontend/
    src/app/
      components/              Standalone Angular components
      services/                Injectable services (API calls, state)
      models/                  TypeScript interfaces and types
```

## Running Services

### Backend
```bash
cd backend/StandupTracker.Api
dotnet run
# Default: http://localhost:5000
```

### Frontend
```bash
cd frontend
npm install
npx ng serve
# Default: http://localhost:4200 (proxies API calls to :5000)
```

## Test Commands

### Backend
```bash
cd backend/StandupTracker.Api.Tests
dotnet test
```

### Frontend
```bash
cd frontend
npx ng test          # unit tests
npx ng lint          # ESLint
```

## Formatting / Linting

### Backend
```bash
cd backend
dotnet format
```

### Frontend
```bash
cd frontend
npx ng lint
npx prettier --write "src/**/*.{ts,html,css}"
```

## Backend Conventions (C# / .NET Minimal API)

- Use **Minimal API** style: `app.MapGet(...)`, `app.MapPost(...)`, etc. — no controllers
- Use **record types** for all models and DTOs: `record StandupEntry(Guid Id, string Yesterday, string Today, string? Blockers, DateTime CreatedAt);`
- **In-memory storage only**: use `ConcurrentDictionary<Guid, T>` or `IMemoryCache` — no database
- Group endpoints into static classes with `IEndpointRouteBuilder` extension methods
- Use `TypedResults` for responses
- Use `builder.Services` for DI registration
- **CORS**: must be enabled in `Program.cs` to allow requests from `http://localhost:4200`
- **API route prefix**: all endpoints under `/api/` (e.g., `/api/standups`, `/api/standups/{id}`)
- **Test project**: use xUnit with `Microsoft.AspNetCore.Mvc.Testing` for integration tests

## Frontend Conventions (Angular + Tailwind)

- **Standalone components only** — every component uses `standalone: true`
- **Signals for state** — use `signal()`, `computed()`, `effect()` for component state
- Use `inject()` function instead of constructor injection
- Use new control flow: `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`)
- Use `HttpClient` via injectable services for API calls
- Use **Tailwind CSS** for all styling
- **Proxy config**: use `proxy.conf.json` to proxy `/api/*` requests to `http://localhost:5000` during dev

## Do NOT Use

- **No EF Core** — no `Microsoft.EntityFrameworkCore` packages
- **No SQLite** — no database files or providers of any kind
- **No MVC controllers** — no `[ApiController]`, no `ControllerBase`
- **No NgModules** — no `@NgModule`, no `.module.ts` files
- **No Angular Material** — Tailwind only
- **No RxJS for local state** — signals for component state; RxJS is fine for HTTP and async streams

## Git

- Repo: https://github.com/ashayakc/standup-tracker.git
- Main branch: `main`
- Create feature branches off `main` (e.g., `feat/add-standup-form`)
- Use `/commitpush` slash command to stage, review, commit, and push changes
