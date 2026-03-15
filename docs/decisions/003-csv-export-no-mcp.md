# ADR-003: CSV Export Feature

**Status:** Accepted
**Date:** 2026-03-15
**Role:** Solution Architect

## Context

Users need to export their standup entries as a CSV file for reporting, sharing with managers, or importing into spreadsheets. The export should include all standup entries in chronological order.

## Decision

### API Contract

**Endpoint:** `GET /api/standups/export`
**Method:** GET (read-only, generates CSV from existing data)
**Content-Type:** `text/csv`
**Content-Disposition:** `attachment; filename="standups.csv"`

**Response:** `200 OK` — CSV file with headers and all entries, newest first.

**CSV Columns:**

| Column | Source Field | Format |
|--------|-------------|--------|
| Id | `id` | GUID string |
| Date | `createdAt` | `yyyy-MM-dd HH:mm:ss` (UTC) |
| Yesterday | `yesterday` | Quoted if contains commas/quotes |
| Today | `today` | Quoted if contains commas/quotes |
| Blockers | `blockers` | Empty string if null |
| Blocker Resolved | `blockerResolved` | `Yes` / `No` |

**Example CSV:**
```
Id,Date,Yesterday,Today,Blockers,Blocker Resolved
a1b2c3d4-...,2026-03-15 09:00:00,Fixed bugs,Write tests,Waiting on API keys,No
e5f6g7h8-...,2026-03-14 09:00:00,Code review,Deploy to staging,,Yes
```

**Edge cases:**
- Empty store: returns CSV with only the header row
- Fields containing commas or double quotes: RFC 4180 compliant quoting (wrap in double quotes, escape internal quotes by doubling)

### Backend Changes

**New method** in `StandupStore`:
- `GetAllForExport()` — returns all entries ordered by `CreatedAt` descending (reuses existing `GetAll()`)

**New endpoint** in `StandupEndpoints`:
- `GET /api/standups/export` — calls `store.GetAll()`, formats as CSV string, returns with `text/csv` content type

**CSV generation:** Inline in the endpoint or a small static helper method in the endpoint class. No external CSV library needed — the data is simple and well-controlled.

### Frontend Changes

**New service method** in `StandupService`:
- `exportCsv(): void` — triggers a file download by opening a new window/link to `/api/standups/export`

**App component update:**
- Add an "Export CSV" button in the daily view, next to or below the existing controls
- Styled as a secondary action button with Tailwind

### Test Plan

**Backend integration tests** (xUnit):

| Test | Description |
|------|-------------|
| `ExportCsv_ReturnsHeaderOnly_WhenNoEntries` | GET returns `200` with CSV containing only the header row |
| `ExportCsv_ReturnsCorrectContentType` | Response Content-Type is `text/csv` |
| `ExportCsv_ReturnsAllEntries` | CSV contains all stored entries |
| `ExportCsv_HandlesCommasInFields` | Fields with commas are properly quoted |
| `ExportCsv_HandlesNullBlockers` | Null blockers render as empty string |

**Frontend:** `ng build` must pass. No additional unit tests required for a simple download link.

## Role Assignments

| Role | Scope |
|------|-------|
| Backend Developer | CSV endpoint, CSV formatting, integration tests |
| Frontend Developer | Service method, export button in app component |

## Consequences

- **No new dependencies** — CSV is generated with string operations
- **No new models** — reuses existing `StandupEntry`
- **File download** — browser handles the download via Content-Disposition header
- **RFC 4180 compliance** — proper quoting ensures compatibility with Excel, Google Sheets, etc.
