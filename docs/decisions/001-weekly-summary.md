# ADR-001: Weekly Summary Feature

**Status:** Accepted
**Date:** 2026-03-15
**Role:** Solution Architect

## Context

The Standup Tracker currently shows a flat list of daily standup entries. Users need a way to see aggregated weekly views with blocker statistics to identify patterns and track team health over time.

## Decision

### API Contract

**Endpoint:** `GET /api/standups/weekly-summary`
**Method:** GET (read-only aggregate, no new storage)

**Response:** `200 OK` — array of `WeeklySummary`, newest week first.

```json
[
  {
    "weekStart": "2026-03-09T00:00:00Z",
    "weekEnd": "2026-03-15T23:59:59Z",
    "standupCount": 5,
    "blockersRaised": 3,
    "blockersResolved": 2,
    "resolutionRate": 66.67,
    "entries": [
      {
        "id": "...",
        "yesterday": "...",
        "today": "...",
        "blockers": "...",
        "blockerResolved": true,
        "createdAt": "2026-03-15T09:00:00Z"
      }
    ]
  }
]
```

**Field definitions:**

| Field | Type | Description |
|---|---|---|
| `weekStart` | `DateTime` | Monday 00:00:00 UTC (ISO 8601 week) |
| `weekEnd` | `DateTime` | Sunday 23:59:59 UTC |
| `standupCount` | `int` | Total standup entries in the week |
| `blockersRaised` | `int` | Entries where `blockers` is non-null/non-empty |
| `blockersResolved` | `int` | Entries where `blockerResolved == true` |
| `resolutionRate` | `double` | `(blockersResolved / blockersRaised) * 100`, rounded to 2 decimals. `0` when no blockers raised. |
| `entries` | `StandupEntry[]` | All entries for the week, newest first |

### Backend Changes

**New model** in `Models/WeeklySummary.cs`:

```csharp
public record WeeklySummary(
    DateTime WeekStart,
    DateTime WeekEnd,
    int StandupCount,
    int BlockersRaised,
    int BlockersResolved,
    double ResolutionRate,
    IReadOnlyList<StandupEntry> Entries);
```

**New method** in `StandupStore`:
- `GetWeeklySummary()` — groups entries by ISO week using `ISOWeek.GetWeekOfYear()`, computes aggregates, returns list of `WeeklySummary`.

**New endpoint** in `StandupEndpoints`:
- `GET /api/standups/weekly-summary` — calls `store.GetWeeklySummary()`, returns `Ok(result)`.

No new storage. No new dependencies. Computation runs over the existing `ConcurrentDictionary`.

### Frontend Changes

**New model** — `models/weekly-summary.ts`:

```typescript
export interface WeeklySummary {
  weekStart: string;
  weekEnd: string;
  standupCount: number;
  blockersRaised: number;
  blockersResolved: number;
  resolutionRate: number;
  entries: StandupEntry[];
}
```

**New service method** in `StandupService`:
- `getWeeklySummary(): Observable<WeeklySummary[]>` — calls `GET /api/standups/weekly-summary`.

**New component** — `components/weekly-summary/weekly-summary.component.ts`:
- Standalone component with signals for state
- Renders one card per week: stats header + expandable entries list
- Styled with Tailwind CSS

**App component update**:
- Add a view toggle (daily list vs. weekly summary) to `AppComponent`

### Test Plan

**Backend integration tests** (xUnit + `Microsoft.AspNetCore.Mvc.Testing`):

| Test | Description |
|---|---|
| `WeeklySummary_ReturnsEmptyArray_WhenNoEntries` | GET returns `200` with `[]` when store is empty |
| `WeeklySummary_ReturnsSingleWeek_WhenAllEntriesSameWeek` | Verify `standupCount`, `weekStart`, `weekEnd` for entries within one week |
| `WeeklySummary_GroupsByWeek_WhenEntriesSpanMultipleWeeks` | Entries across 2+ weeks produce separate summary objects, newest week first |
| `WeeklySummary_CountsBlockersRaised_WhenBlockersFieldNonNull` | Only entries with non-null/non-empty `blockers` count toward `blockersRaised` |
| `WeeklySummary_CountsBlockersResolved_WhenResolvedIsTrue` | Only entries with `blockerResolved == true` count toward `blockersResolved` |
| `WeeklySummary_ResolutionRateIsZero_WhenNoBlockersRaised` | Avoids divide-by-zero; returns `0` not NaN |
| `WeeklySummary_ResolutionRateCalculation_WhenMixedBlockers` | e.g. 2 resolved / 3 raised = `66.67` |
| `WeeklySummary_EntriesOrderedNewestFirst_WithinWeek` | Entries inside each week's `entries` array sorted by `createdAt` descending |

**Frontend unit tests** (Jasmine + Karma):

| Test | Description |
|---|---|
| `StandupService.getWeeklySummary` calls correct endpoint | Verify `GET /api/standups/weekly-summary` is called |
| `WeeklySummaryComponent` renders week cards | Given mock data, assert one card per week is rendered |
| `WeeklySummaryComponent` displays correct stats | Assert `standupCount`, `blockersRaised`, `blockersResolved`, `resolutionRate` are rendered |
| `WeeklySummaryComponent` shows empty state | When summary array is empty, display a "No data" message |
| `WeeklySummaryComponent` expands entries on click | Click a week card, assert entries list becomes visible |

## Role Assignments

| Role | Scope |
|---|---|
| Backend Developer | `WeeklySummary` record, `GetWeeklySummary()` method, endpoint, integration tests |
| Frontend Developer | `WeeklySummary` interface, service method, `WeeklySummaryComponent`, app layout update |
| Doc Expert | XML comments on backend types/methods, JSDoc on frontend interface/service |
| Architecture Expert | Review contract alignment between backend response and frontend interface |

## Consequences

- **No database impact** — summary is computed on the fly from in-memory store
- **Single API call** — entries embedded in the summary avoids N+1 requests from the frontend
- **Week boundary** — using ISO 8601 (Monday start) keeps it deterministic across locales
- **Resolution rate edge case** — returns `0` (not NaN/null) when no blockers exist in a week
