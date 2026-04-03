> **AGENT INSTRUCTIONS (read before every action):**
> This file is the single source of truth for project progress.
> - **Update this file immediately** after completing or starting any task — before moving to the next one.
> - Mark completed tasks with `[x]`, in-progress with `[~]`, and pending with `[ ]`.
> - Update the "Last Updated" timestamp on every edit.
> - If the architecture changes (e.g., new projects added), update the Architecture section too.

---

# Pantry-to-Plate MVP — Progress Tracker

**Last Updated:** 2026-04-03

---

## Architecture Note

The plan originally put Models/Data/Services inside the MAUI project. Due to a framework incompatibility between MAUI (multi-targeted) and the xUnit test project (net10.0), a `PantryToPlate.Core` class library was introduced. All shared logic lives there; both the MAUI app and the test project reference it.

```
PantryToPlate.Core/       ← Models, Data, Services (net10.0 classlib)
PantryToPlate/            ← MAUI app (references Core)
PantryToPlate.Tests/      ← xUnit tests (references Core)
PantryToPlate.sln
```

---

## Completed

- [x] **Task 1 — Project Scaffolding**
  - MAUI project created (`PantryToPlate/`)
  - `PantryToPlate.Core` classlib created (net10.0) — houses Models, Data, Services
  - xUnit test project created (`PantryToPlate.Tests/`)
  - NuGet packages added: EF Core SQLite 8.0.10, CommunityToolkit.Mvvm 8.3.2
  - Solution file `PantryToPlate.sln` created with all 3 projects
  - Git repository initialized, initial commit made

- [x] **Task 2 — Data Models** (`PantryToPlate.Core/Models/`)
  - `Ingredient.cs`
  - `Recipe.cs`
  - `RecipeIngredient.cs`
  - `PantryItem.cs`
  - `ShoppingListItem.cs`

- [x] **Task 3 — AppDbContext** (`PantryToPlate.Core/Data/AppDbContext.cs`)
  - EF Core DbContext with SQLite
  - Relationship configuration for all entities

- [x] **Task 4 — DatabaseSeeder** (`PantryToPlate.Core/Data/DatabaseSeeder.cs`)
  - 6 staple ingredients (Water, Oil, Salt, Pepper, Sugar, Butter)
  - 30 non-staple ingredients
  - 25 seeded recipes

- [x] **Task 5 — RecipeService** (`PantryToPlate.Core/Services/`)
  - `IRecipeService.cs` interface
  - `RecipeService.cs` implementation (`GetAvailableRecipesAsync`, `GetRecipeByIdAsync`, `CookRecipeAsync`)
  - `PantryToPlate.Tests/Services/RecipeServiceTests.cs` — 7 xUnit tests written and verified passing

---

## In Progress

- [x] **Task 6 — ViewModels** (`PantryToPlate/ViewModels/`)
  - `HomeViewModel.cs`
  - `RecipeDetailViewModel.cs`
  - `PantryViewModel.cs`
  - `ShoppingListViewModel.cs`

---

## In Progress

- [~] **Task 7 — BoolToCheckIconConverter** (`PantryToPlate/Converters/BoolToCheckIconConverter.cs`)

- [ ] **Task 6 — ViewModels** (`PantryToPlate/ViewModels/`)
  - `HomeViewModel.cs`
  - `RecipeDetailViewModel.cs`
  - `PantryViewModel.cs`
  - `ShoppingListViewModel.cs`

- [ ] **Task 7 — BoolToCheckIconConverter** (`PantryToPlate/Converters/BoolToCheckIconConverter.cs`)

- [x] **Task 8 — MauiProgram + DI Registration** (`PantryToPlate/MauiProgram.cs`)
  - Register AppDbContext, RecipeService, all ViewModels and Views
  - DB init + seed on startup

---

## In Progress

- [x] **Task 9 — AppShell (Tab Navigation)** (`PantryToPlate/AppShell.xaml`)
  - 3 tabs: Home | Pantry | Shopping List
  - Route registered for RecipeDetailPage

---

## In Progress

- [x] **Task 10 — Home Page** (`PantryToPlate/Views/HomePage.xaml`)
  - Recipe list with tap-to-navigate
  - Empty state message
  - `InverseBoolConverter` + `IngredientsToSummaryConverter` registered in App.xaml

---

## In Progress

- [x] **Task 11 — Recipe Detail Page** (`PantryToPlate/Views/RecipeDetailPage.xaml`)
  - Ingredient checklist with ✅/❌ icons
  - Collapsible cooking instructions
  - "Got it! I'm cooking this" button + confirmation modal
  - Auto-deduct + navigate back to Home on confirm

---

## In Progress

- [x] **Task 12 — Pantry Page** (`PantryToPlate/Views/PantryPage.xaml`)
  - Add ingredient form (name, qty, unit)
  - Swipe-to-delete pantry items

---

## In Progress

- [x] **Task 13 — Shopping List Page** (`PantryToPlate/Views/ShoppingListPage.xaml`)
  - Checkbox toggle per item
  - Strikethrough on purchased items
  - Clear Purchased button
  - `BoolToStrikethroughConverter`

- [x] **Task 14 — Final Verification**
  - Full solution build: `0 Error(s)`
  - All tests pass
  - PRD requirements coverage check

- [x] **Task 15 — Recipe CRUD** (`PantryToPlate.Core/Services/`)
  - `IRecipeService.AddRecipeAsync`, `UpdateRecipeAsync`, `DeleteRecipeAsync`
  - `RecipeService` implementation
  - Added unit tests for add/update/delete recipes

- [x] **Task 16 — Recipe CRUD UI** (`PantryToPlate/Views/`, `PantryToPlate/ViewModels/`)
  - `HomePage` add button and create model command
  - `RecipeDetailPage` delete command + route handling
  - `RecipeEditorPage` and `RecipeEditorViewModel` for adding recipes
  - AppShell route registration for `RecipeEditorPage`
