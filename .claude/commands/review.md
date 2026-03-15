reviews staged git changes against CLAUDE.md conventions:
   - C# naming conventions (PascalCase types, camelCase locals)
   - Angular standalone components, no NgModules
   - No EF Core or SQLite references
   - No hardcoded URLs (must use Angular environment files)
   - Missing error handling on HttpClient calls
   - Report violations as numbered list with filename reference
   - If clean, say LGTM with a one-line summary