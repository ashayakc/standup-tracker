# Frontend Conventions (Angular 18 + Tailwind CSS)

This file contains all Angular/TypeScript-specific conventions for the frontend. See the root `CLAUDE.md` for project overview, roles, shared domain model, and cross-cutting rules.

## Conventions

- **Standalone components only** — every component uses `standalone: true`
- **Signals for state** — use `signal()`, `computed()`, `effect()` for component state
- Use `inject()` function instead of constructor injection
- Use new control flow: `@if`, `@for`, `@switch` (not `*ngIf`, `*ngFor`)
- Use `HttpClient` via injectable services for API calls
- Use **Tailwind CSS** for all styling — no Angular Material
- **Proxy config**: use `proxy.conf.json` to proxy `/api/*` requests to `http://localhost:5000` during dev

## Component Patterns

- All components are standalone (`standalone: true` in decorator)
- State is managed with signals, not RxJS subjects
- Dependency injection via `inject()` function, not constructor params

```typescript
@Component({
  selector: 'app-example',
  standalone: true,
  imports: [CommonModule],
  template: `...`
})
export class ExampleComponent {
  private readonly service = inject(ExampleService);
  items = signal<Item[]>([]);
  loading = signal(false);
}
```

## Tailwind Usage

- All styling via Tailwind utility classes
- No Angular Material, no component libraries
- Responsive design with Tailwind breakpoints

## Testing

- Unit tests: `cd frontend && npx ng test`
- Linting: `cd frontend && npx ng lint`
- Formatting: `npx prettier --write "src/**/*.{ts,html,css}"`

## Do NOT Use

- **No NgModules** — no `@NgModule`, no `.module.ts` files
- **No Angular Material** — Tailwind only
- **No RxJS for local state** — signals for component state; RxJS is fine for HTTP and async streams
