# ADR-002: CSV Export Feature

**Status:** Accepted
**Date:** 2026-03-15
**Role:** Solution Architect
**Issue:** [#1 — Add export to CSV feature](https://github.com/ashayakc/standup-tracker/issues/1)

## Context

Users want to export their standup history to CSV so they can share it with their manager. The export must include all standup fields and the filename must include the current date.

## Decision

### API Contract

**Endpoint:** `GET /api/standups/export`
**Method:** GET (read-only, returns CSV file)

**Response:** `200 OK` — CSV file download

**Headers:**
- `Content-Type: text/csv`
- `Content-Disposition: attachment; filename="standups-2026-03-15.csv"`

The filename uses the format `standups-{yyyy-MM-dd}.csv` where the date is the current UTC date at time of export.

**CSV Format:**

```csv
Id,Yesterday,Today,Blockers,BlockerResolved,CreatedAt
<guid>,"Did X","Will do Y","Blocked on Z",false,2026-03-15T09:00:00Z
```

**CSV rules:**
- First row is the header
- Fields containing commas, quotes, or newlines are enclosed in double quotes
- Double quotes within fields are escaped as `""`
- Entries ordered newest first (consistent with GET /api/standups)
- Empty response (no entries) still returns the header row

### Backend Changes

**New endpoint** in `StandupEndpoints`:
- `GET /api/standups/export` — calls `store.GetAll()`, formats as CSV, returns file result with appropriate headers.

**No new models.** No new store methods. Reuses existing `GetAll()`.

CSV generation is done inline in the endpoint using `StringBuilder` — no external CSV library needed for this simple flat structure.

### Frontend Changes

**New service method** in `StandupService`:
- `getExportUrl(): string` — returns `'/api/standups/export'` (the URL for direct browser download).

**App component update**:
- Add an "Export CSV" button next to the Daily/Weekly toggle in the header area.
- On click, trigger a file download by navigating to the export URL.
- The browser handles the download natively via the Content-Disposition header.

**No new component needed.** The export button lives in `AppComponent` template.

### Test Plan

**Backend integration tests** (xUnit + `Microsoft.AspNetCore.Mvc.Testing`):

| Test | Description |
|---|---|
| `Export_ReturnsCsvContentType` | GET /api/standups/export returns `text/csv` content type |
| `Export_ReturnsHeaderRow_WhenNoEntries` | Response body contains the CSV header row even with empty store |
| `Export_ReturnsAllEntries_AsCsv` | Add 2 entries, verify CSV contains header + 2 data rows |
| `Export_EscapesCommasInFields` | Entry with comma in text field is properly quoted |
| `Export_ContentDispositionIncludesDate` | Response has `Content-Disposition: attachment; filename="standups-*.csv"` |

**Frontend:**
- No unit test needed — the export button is a simple anchor/click handler with no complex logic.
- Verified via `ng build` (compilation check).

### Role Assignments

| Role | Scope |
|---|---|
| Backend Developer | CSV endpoint, integration tests |
| Frontend Developer | Export button in AppComponent, service URL method |
| Architecture Expert | Review CSV format matches frontend expectations, CLAUDE.md compliance |

## Consequences

- **No new dependencies** — CSV is simple enough to build with StringBuilder
- **No new models** — reuses existing StandupEntry and GetAll()
- **Browser-native download** — no Blob/JS manipulation needed on the frontend
- **Date in filename** — uses server UTC date, not client date, for consistency
