Follow these steps in order:

1. Stage all changed files (skip secrets, .env files, and large binaries). Show a summary of what was staged.

2. Review staged changes against CLAUDE.md conventions (C# naming, standalone components, no EF Core/SQLite, no hardcoded URLs, error handling). Report violations as a numbered list with filename references.

3. If the review found any suggestions:
   - Show the suggestions and ask: "Fix these issues or proceed without fixing?"
   - If I say fix, ask which specific issues to fix (by number). Fix only those, then re-stage and re-review.
   - If I say proceed, continue to the next step.

4. Create a commit with a concise message describing the changes, and push to the current branch on origin.

5. Report the commit hash and branch name when done.
