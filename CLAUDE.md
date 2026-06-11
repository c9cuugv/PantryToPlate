# PantryToPlate

.NET MAUI recipe/pantry app — manage ingredients, match recipes to what's in your pantry, auto-deduct on cook.

## Commands

```bash
# Build all
dotnet build

# Test (xUnit, runs on net10.0 directly)
dotnet test PantryToPlate.Tests/

# Run on Mac
dotnet build PantryToPlate/ -t:Run -f net10.0-maccatalyst

# Run on Android (emulator must be running)
dotnet build PantryToPlate/ -t:Run -f net10.0-android
```

## Architecture

```
PantryToPlate.Core/       ← Models, Data (EF Core/SQLite), Services  [net10.0 classlib]
PantryToPlate/            ← MAUI app — Views, ViewModels, Converters  [multi-targeted]
PantryToPlate.Tests/      ← xUnit tests — only tests Core logic       [net10.0]
PantryToPlate.sln
```

**Core is separate because MAUI's multi-targeting breaks xUnit references.** All testable logic lives in Core; MAUI only has UI + DI wiring.

## Key Files

| File | Purpose |
|------|---------|
| `PROGRESS.md` | Source of truth — update after every task |
| `PantryToPlate.Core/Data/AppDbContext.cs` | EF Core DbContext, all relationships |
| `PantryToPlate.Core/Data/DatabaseSeeder.cs` | Seeds 36 ingredients + 25 recipes on first run |
| `PantryToPlate/MauiProgram.cs` | DI registration, DB init + seed on startup |
| `PantryToPlate/AppShell.xaml` | Tab nav (Home/Pantry/Shopping List) + route for RecipeDetailPage |

## Gotchas

- **No CommunityToolkit.Mvvm** — was removed. ViewModels use manual `INotifyPropertyChanged`. Don't add it back.
- **EF Core 8.0.10 on net10.0** — intentional, do not upgrade without testing migrations.
- **XAML SourceGen enabled** (`MauiXamlInflator=SourceGen`) — XAML compiles at build time. Runtime-only APIs won't work.
- **SQLite DB path** — resolved at runtime per platform via `FileSystem.AppDataDirectory`. Never hardcode a path.
- **MAUI project is untestable directly** — don't add xUnit refs to `PantryToPlate/`. Test through `PantryToPlate.Core` only.
- **MAUI workloads (HDD dotnet)** — installed at `HDD/dotnet-sdk` with `maui-maccatalyst`. To use: `export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="HDD/nuget-cache"`. Restore order matters: restore MAUI first, then re-restore Core standalone, then build with `--no-restore`.
- **macCatalyst requires full Xcode.app** — not Command Line Tools. Install Xcode from App Store, then `sudo xcode-select -s /Applications/Xcode.app/Contents/Developer`. CLT alone (even 26.1) will not work.
- **PROGRESS.md has duplicate "In Progress" sections** — it's messy but intentional per-task tracking. Don't reformat it.

## Token Optimization (MCP)

Prefer these over Bash+cat for large outputs:

```
ctx_batch_execute   → explore project + search in one call
ctx_execute_file    → analyze a file without flooding context
ctx_search          → follow-up queries on already-indexed output
obsidian-cli        → if notes/designs live in a vault
```

Use `Read` only when you need to `Edit` the file immediately after.

## Workflow

1. Check `PROGRESS.md` before starting any task — it tracks what's done.
2. All new logic → `PantryToPlate.Core/`, not in MAUI project.
3. New Views need: ViewModel, View (XAML), DI registration in `MauiProgram.cs`, route in `AppShell.xaml`.
4. After any task: update `PROGRESS.md` status.

# context-mode — MANDATORY routing rules

You have context-mode MCP tools available. These rules are NOT optional — they protect your context window from flooding. A single unrouted command can dump 56 KB into context and waste the entire session.

## BLOCKED commands — do NOT attempt these

### curl / wget — BLOCKED
Any Bash command containing `curl` or `wget` is intercepted and replaced with an error message. Do NOT retry.
Instead use:
- `ctx_fetch_and_index(url, source)` to fetch and index web pages
- `ctx_execute(language: "javascript", code: "const r = await fetch(...)")` to run HTTP calls in sandbox

### Inline HTTP — BLOCKED
Any Bash command containing `fetch('http`, `requests.get(`, `requests.post(`, `http.get(`, or `http.request(` is intercepted and replaced with an error message. Do NOT retry with Bash.
Instead use:
- `ctx_execute(language, code)` to run HTTP calls in sandbox — only stdout enters context

### WebFetch — BLOCKED
WebFetch calls are denied entirely. The URL is extracted and you are told to use `ctx_fetch_and_index` instead.
Instead use:
- `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` to query the indexed content

## REDIRECTED tools — use sandbox equivalents

### Bash (>20 lines output)
Bash is ONLY for: `git`, `mkdir`, `rm`, `mv`, `cd`, `ls`, `npm install`, `pip install`, and other short-output commands.
For everything else, use:
- `ctx_batch_execute(commands, queries)` — run multiple commands + search in ONE call
- `ctx_execute(language: "shell", code: "...")` — run in sandbox, only stdout enters context

### Read (for analysis)
If you are reading a file to **Edit** it → Read is correct (Edit needs content in context).
If you are reading to **analyze, explore, or summarize** → use `ctx_execute_file(path, language, code)` instead. Only your printed summary enters context. The raw file content stays in the sandbox.

### Grep (large results)
Grep results can flood context. Use `ctx_execute(language: "shell", code: "grep ...")` to run searches in sandbox. Only your printed summary enters context.

## Tool selection hierarchy

1. **GATHER**: `ctx_batch_execute(commands, queries)` — Primary tool. Runs all commands, auto-indexes output, returns search results. ONE call replaces 30+ individual calls.
2. **FOLLOW-UP**: `ctx_search(queries: ["q1", "q2", ...])` — Query indexed content. Pass ALL questions as array in ONE call.
3. **PROCESSING**: `ctx_execute(language, code)` | `ctx_execute_file(path, language, code)` — Sandbox execution. Only stdout enters context.
4. **WEB**: `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` — Fetch, chunk, index, query. Raw HTML never enters context.
5. **INDEX**: `ctx_index(content, source)` — Store content in FTS5 knowledge base for later search.

## Subagent routing

When spawning subagents (Agent/Task tool), the routing block is automatically injected into their prompt. Bash-type subagents are upgraded to general-purpose so they have access to MCP tools. You do NOT need to manually instruct subagents about context-mode.

## Output constraints

- Keep responses under 500 words.
- Write artifacts (code, configs, PRDs) to FILES — never return them as inline text. Return only: file path + 1-line description.
- When indexing content, use descriptive source labels so others can `ctx_search(source: "label")` later.

## ctx commands

| Command | Action |
|---------|--------|
| `ctx stats` | Call the `ctx_stats` MCP tool and display the full output verbatim |
| `ctx doctor` | Call the `ctx_doctor` MCP tool, run the returned shell command, display as checklist |
| `ctx upgrade` | Call the `ctx_upgrade` MCP tool, run the returned shell command, display as checklist |
