# PantryToPlate (MVP)

PantryToPlate is a .NET MAUI sample app designed to turn kitchen inventory into recipe-based meal planning. The app helps users track pantry stock, view recipes they can make with current ingredients, and maintain a shopping list for missing items.

## What it includes
- `PantryToPlate/`: .NET MAUI cross-platform UI
- `PantryToPlate.Core/`: shared domain logic (Models, Data, Services)
- `PantryToPlate.Tests/`: xUnit tests for business logic (RecipeService, etc.)

## Core features
1. Recipe service: available recipes, recipe lookup, cooking/stock deduction
2. Pantry tracking: add/remove items, quantity and unit management
3. Shopping list: toggle purchased, clear purchased
4. Seeded set of staple and non-staple ingredients + recipes

## Architecture
- MVVM (CommunityToolkit.Mvvm)
- EF Core SQLite for local data persistence
- Shell navigation with 3 tabs:
  - Home (recipe discovery)
  - Pantry (inventory management)
  - Shopping List

## Folder structure
- `Models`: Ingredient, Recipe, RecipeIngredient, PantryItem, ShoppingListItem
- `Data`: `AppDbContext`, `DatabaseSeeder`
- `Services`: `IRecipeService`, `RecipeService`
- `ViewModels`: `HomeViewModel`, `RecipeDetailViewModel`, `PantryViewModel`, `ShoppingListViewModel`
- `Converters`: `BoolToCheckIconConverter`, `InverseBoolConverter`, `IngredientsToSummaryConverter`, `BoolToStrikethroughConverter`
- `Views`: `MainPage`, `RecipeDetailPage`, `PantryPage`, `ShoppingListPage`

## Run locally
1. `dotnet restore`
2. `dotnet test PantryToPlate.Tests/PantryToPlate.Tests.csproj`
3. Use Visual Studio 2022/2023 with MAUI workload and a valid SDK as needed

## Progress tracking
- `PROGRESS.md` is the single source of truth for task status.
- Update it with [x], [~], [ ] and change `Last Updated` each task.

## Notes
- The current environment may require JDK 21 for Android builds (`dotnet test` for full solution may fail with JDK 23 warning).