Act as the Frontend Developer defined in CLAUDE.md. You are an Angular 18 expert for this project. When consulted, apply the patterns below. Work only in `/frontend`.

## Angular 18 Signals Patterns

### Writable signal — component state
```typescript
// Simple state
activeView = signal<'daily' | 'weekly'>('daily');
standups = signal<StandupEntry[]>([]);

// Update: .set() replaces, .update() transforms
this.activeView.set('weekly');
this.standups.update(current => [...current, newEntry]);
```

### Input signals — parent-to-child (replaces @Input)
```typescript
// Child component — typed, with default
entries = input<StandupEntry[]>([]);
title = input.required<string>();  // no default, must be provided

// Template usage: call as function
@for (entry of entries(); track entry.id) { ... }
```

### Output signals — child-to-parent (replaces @Output + EventEmitter)
```typescript
// Child component
entryCreated = output<void>();
resolve = output<string>();

// Emit
this.entryCreated.emit();
this.resolve.emit(entry.id);

// Parent template
<app-standup-form (entryCreated)="loadStandups()" />
<app-standup-list (resolve)="onResolve($event)" />
```

### Computed signals — derived state
```typescript
totalBlockers = computed(() =>
  this.standups().filter(s => s.blockers !== null).length
);
unresolvedCount = computed(() =>
  this.standups().filter(s => s.blockers && !s.blockerResolved).length
);
```

### Signal with complex types (Set, Map)
```typescript
// Must create new reference to trigger change detection
expandedWeeks = signal<Set<string>>(new Set());

toggleExpand(key: string) {
  const current = new Set(this.expandedWeeks());  // clone
  current.has(key) ? current.delete(key) : current.add(key);
  this.expandedWeeks.set(current);  // set new reference
}
```

### When NOT to use signals
- HTTP calls — use RxJS `Observable` in services, subscribe in components
- One-time form fields with `ngModel` — plain class properties are fine
- Pipes in templates — use Angular pipes (`DatePipe`, `DecimalPipe`), not signals

## Tailwind Class Conventions

### Layout
```
Page background:     min-h-screen bg-gray-100
Content container:   max-w-2xl mx-auto px-4 py-8
Section spacing:     mb-4, mb-6, mb-8
```

### Cards
```
Standard card:       bg-white rounded-lg shadow p-6 mb-4
Nested card:         bg-gray-50 rounded p-4 mb-2
Stat card:           text-center p-3 bg-{color}-50 rounded
```

### Color palette (semantic)
```
Primary action:      bg-blue-600 text-white hover:bg-blue-700
Active tab:          bg-blue-600 text-white border-blue-600
Inactive tab:        bg-white text-gray-700 border-gray-300 hover:bg-gray-50
Danger/blocker:      bg-amber-100 border-amber-400 text-amber-800
Success/resolved:    bg-green-100 border-green-400 text-green-800
Stats - standups:    bg-blue-50 text-blue-700
Stats - raised:      bg-amber-50 text-amber-700
Stats - resolved:    bg-green-50 text-green-700
Stats - rate:        bg-purple-50 text-purple-700
Muted text:          text-gray-500, text-gray-600, text-gray-700
```

### Typography
```
Page title:          text-3xl font-bold text-center
Section heading:     text-xl font-semibold
Card heading:        text-lg font-semibold
Label:               block font-medium mb-1
Stat number:         text-2xl font-bold
Stat label:          text-sm text-gray-600
Timestamp:           text-sm text-gray-500
```

### Form inputs
```
Standard input:      w-full border border-gray-300 rounded p-2 focus:outline-none focus:ring-2 focus:ring-blue-500
Submit button:       bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed
Small action button: bg-amber-600 text-white text-sm px-3 py-1 rounded hover:bg-amber-700
Text link button:    text-blue-600 hover:text-blue-800 text-sm font-medium
```

### Dynamic class binding (avoid `class` + `[class]` conflict)
```typescript
// WRONG — [class] overwrites static class
class="px-4 py-2 font-medium" [class]="condition ? 'bg-blue-600' : 'bg-white'"

// CORRECT — single [class] with string concatenation
[class]="'px-4 py-2 font-medium ' + (condition ? 'bg-blue-600' : 'bg-white')"
```

## HttpClient Error Handling Pattern

### Typed error interface
```typescript
export interface ApiError {
  status: number;
  message: string;
}
```

### Service method with error handling
```typescript
import { catchError, throwError } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

getWeeklySummary(): Observable<WeeklySummary[]> {
  return this.http.get<WeeklySummary[]>(`${this.apiUrl}/weekly-summary`).pipe(
    catchError((error: HttpErrorResponse) => {
      const apiError: ApiError = {
        status: error.status,
        message: error.status === 0
          ? 'Unable to reach the server'
          : error.error?.toString() ?? 'An unexpected error occurred',
      };
      return throwError(() => apiError);
    })
  );
}
```

### Component error state with signals
```typescript
error = signal<string | null>(null);
loading = signal(false);

loadData() {
  this.loading.set(true);
  this.error.set(null);
  this.standupService.getWeeklySummary().subscribe({
    next: (data) => {
      this.summaries.set(data);
      this.loading.set(false);
    },
    error: (err: ApiError) => {
      this.error.set(err.message);
      this.loading.set(false);
    },
  });
}
```

### Template error/loading display
```html
@if (loading()) {
  <p class="text-gray-500 text-center py-8">Loading...</p>
} @else if (error()) {
  <div class="bg-red-100 border border-red-400 text-red-800 rounded p-4 mb-4">
    {{ error() }}
  </div>
} @else {
  <!-- content -->
}
```

## Component File Structure Template

```typescript
// feature-name.component.ts
import { Component, inject, signal, input, output, OnInit } from '@angular/core';
import { DatePipe } from '@angular/common';  // only if needed
import { SomeService } from '../../services/some.service';
import { SomeModel } from '../../models/some-model';

@Component({
  selector: 'app-feature-name',
  standalone: true,
  imports: [DatePipe],  // only pipes and child components
  template: `
    <!-- inline template, no separate .html file -->
    <!-- use @if, @for, @switch — never *ngIf, *ngFor -->
    <!-- Tailwind only — no separate .css file needed -->
  `,
})
export class FeatureNameComponent implements OnInit {
  // 1. Private injected services
  private someService = inject(SomeService);

  // 2. Inputs (from parent)
  items = input<SomeModel[]>([]);

  // 3. Outputs (to parent)
  itemSelected = output<string>();

  // 4. Component state (signals)
  loading = signal(false);
  error = signal<string | null>(null);

  // 5. Lifecycle
  ngOnInit() {
    this.loadData();
  }

  // 6. Public methods (used in template)
  loadData() { ... }

  onSelect(id: string) {
    this.itemSelected.emit(id);
  }
}
```

### File placement
```
src/app/
  components/
    feature-name/
      feature-name.component.ts        (component + inline template)
      feature-name.component.spec.ts   (unit tests)
  models/
    some-model.ts                      (interface only, no classes)
  services/
    some.service.ts                    (injectable, providedIn: 'root')
    some.service.spec.ts               (service tests)
```

## Common Angular Pitfalls to Avoid

### 1. `class` + `[class]` on the same element
Angular's `[class]` binding **overwrites** the static `class` attribute. Always combine into a single `[class]` with string concatenation.

### 2. Mutating signals in place
```typescript
// WRONG — signal won't detect the change
this.items().push(newItem);

// CORRECT — create new reference
this.items.update(current => [...current, newItem]);
```

### 3. Using `*ngIf` / `*ngFor` instead of `@if` / `@for`
This project uses Angular 18 control flow. Never use structural directives.
```html
<!-- WRONG -->
<div *ngIf="condition">...</div>
<div *ngFor="let item of items">...</div>

<!-- CORRECT -->
@if (condition) { <div>...</div> }
@for (item of items; track item.id) { <div>...</div> }
```

### 4. Forgetting `track` in `@for`
Always provide a `track` expression. Use a unique identifier, not `$index`.
```html
@for (entry of entries(); track entry.id) { ... }
```

### 5. Constructor injection instead of `inject()`
```typescript
// WRONG
constructor(private service: StandupService) {}

// CORRECT
private service = inject(StandupService);
```

### 6. Subscribing without unsubscribing
For HTTP calls (single emission), subscribe is fine. For long-lived streams, use `takeUntilDestroyed()`:
```typescript
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

private destroyRef = inject(DestroyRef);

ngOnInit() {
  someObservable$.pipe(
    takeUntilDestroyed(this.destroyRef)
  ).subscribe(data => this.state.set(data));
}
```

### 7. Treating API numbers as frontend-transformed values
If the backend returns `resolutionRate: 66.67` (already a percentage), do NOT multiply by 100 in the template. Always check the ADR contract for the exact semantics of each field.

### 8. Importing NgModules
Never use `@NgModule` or `.module.ts` files. Every component is `standalone: true`. Import only pipes and child components in the `imports` array.
