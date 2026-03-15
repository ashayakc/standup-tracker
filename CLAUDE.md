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
      components/
        standup-form/          Form to create new standup entries
        standup-list/          List of daily standup entries
        weekly-summary/        Weekly aggregated view with blocker stats
      services/                Injectable services (API calls, state)
      models/                  TypeScript interfaces and types
  docs/
    decisions/                 Architecture Decision Records (ADRs)
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
- **InternalsVisibleTo**: test project has access to `internal` members via `.csproj` config

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

## Roles

### Frontend Developer
- Works only in `/frontend` folder
- Angular 18 standalone components, TypeScript strict mode
- Focuses on UX, component design, HttpClient calls
- Never touches backend files

### Backend Developer
- Works only in `/backend` folder
- C# .NET 9 minimal API, no EF Core
- Focuses on endpoints, models, in-memory store
- Never touches frontend files

### Doc Expert
- Works across entire codebase read-only
- Writes XML comments for C#, JSDoc for TypeScript
- Updates README.md and any .md files
- Never writes business logic

### Solution Architect
- Entry point for all new features — always consulted first
- Owns the overall design decision before any code is written
- Defines the contract between frontend and backend (API shape, field names, HTTP methods, response format)
- Assigns work to Frontend Developer and Backend Developer roles
- Reviews output from all other roles for consistency
- Has veto power — if something violates the architecture, it gets flagged before merge
- Never writes implementation code
- Produces a short ADR (Architecture Decision Record) for every non-trivial feature as a `.md` file in `/docs/decisions/`

### Architecture Expert
- Reviews cross-cutting concerns
- Checks frontend/backend contract alignment
- Verifies CLAUDE.md conventions are followed
- Never writes code, only reviews and advises

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/standups` | List all entries, newest first |
| POST | `/api/standups` | Create entry (yesterday, today, blockers) |
| PATCH | `/api/standups/{id}/resolve` | Resolve a blocker |
| GET | `/api/standups/weekly-summary` | Weekly aggregated stats with entries |

## ADRs

Architecture Decision Records live in `/docs/decisions/`. Every non-trivial feature gets one.

| ADR | Feature |
|-----|---------|
| [001-weekly-summary](docs/decisions/001-weekly-summary.md) | Weekly Summary with blocker stats |

## Git

- Repo: https://github.com/ashayakc/standup-tracker.git
- Main branch: `main`
- Create feature branches off `main` (e.g., `feat/add-standup-form`)
- Use `/commitpush` slash command to stage, review, commit, and push changes
