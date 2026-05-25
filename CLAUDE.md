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
