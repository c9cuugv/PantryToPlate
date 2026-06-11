# PantryToPlate — Codex Agents

This project uses the wshobson/agents plugin marketplace.
All agents and skills live in `~/.codex/` (installed globally).

## How to use

Reference any agent by name via the `@` syntax or use skills via `/skill` commands.

Key agents for this project:
- `csharp-pro` — C#/.NET development including MAUI and EF Core
- `mobile-developer` — mobile UI patterns and cross-platform
- `test-automator` — xUnit test generation
- `code-reviewer` — code review with security focus
- `database-architect` — EF Core/SQLite schema design

## Quick start

```
# Open in the project directory and reference agents naturally:
"Use csharp-pro to refactor this ViewModel"
"Have code-reviewer audit the data access layer"
```

See `~/agents/docs/plugins.md` for full catalog (83 plugins).
