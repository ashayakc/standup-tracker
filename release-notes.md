# Release Notes - Standup Tracker v1.0

**Release Date:** 2026-03-15

## Overview

Standup Tracker is a daily standup logging tool for tracking yesterday's work, today's plan, and blockers. It features a .NET 9 Minimal API backend with in-memory storage and an Angular 18 frontend styled with Tailwind CSS.

---

## Features

### Standup Entry Management
- **Create standup entries** with yesterday's work, today's plan, and optional blockers via `POST /api/standups`
- **List all entries** sorted newest-first via `GET /api/standups`
- **Resolve blockers** on individual entries via `PATCH /api/standups/{id}/resolve`
- Amber/green visual styling to distinguish active blockers from resolved entries

### Weekly Summary
- **Aggregated weekly view** via `GET /api/standups/weekly-summary` with ISO week grouping
- Blocker statistics per week (total, resolved, unresolved)
- Expand/collapse UI for drilling into individual entries
- Daily/Weekly tab toggle in the main interface

### Frontend
- Angular 18 standalone components with signals-based state management
- Tailwind CSS styling throughout (no Angular Material)
- Standup form component for creating entries
- Standup list component for viewing entries
- Weekly summary component with expand/collapse
- Proxy configuration for seamless local development (`localhost:4200` -> `localhost:5000`)

### Backend
- .NET 9 Minimal API (no controllers, no EF Core, no database)
- Thread-safe in-memory storage using `ConcurrentDictionary`
- Record types for all models and DTOs
- CORS enabled for frontend dev server

### Testing
- 8 backend integration tests using xUnit and `WebApplicationFactory`
- 9 frontend unit tests
- All tests passing

### Developer Tooling
- Claude Code slash commands: `/commitpush`, `/stage`, `/review`, `/start`, `/commit`, `/explain`
- `/frontend-expert` and `/backend-expert` skill commands with project-specific patterns and conventions
- Auto-format hook for C# files via `dotnet format`
- Architecture Decision Records in `/docs/decisions/`
- ADR-001: Weekly Summary design

---

## Commit History

| Commit | Description |
|--------|-------------|
| `78db6b8` | Initial project scaffold: .NET Minimal API backend + Angular 18 frontend |
| `efb5b02` | Add Claude Code slash commands, hooks, and frontend README |
| `dc38d8e` | Rename /push to /commitpush with stage-review-commit workflow |
| `1d613ef` | Fix hooks config to use correct Claude Code hooks schema |
| `2b0509b` | Add Weekly Summary feature with blocker stats and ADR |
| `5096393` | Add frontend-expert and backend-expert skill commands |

---

## Tech Stack

- **Backend:** C# / .NET 9 Minimal API
- **Frontend:** Angular 18 / TypeScript / Tailwind CSS v3
- **Testing:** xUnit (backend), Karma/Jasmine (frontend)
- **Storage:** In-memory (`ConcurrentDictionary`)
