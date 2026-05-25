# Graph Report - PantryToPlate.Core  (2026-04-09)

## Corpus Check
- Corpus is ~3,394 words - fits in a single context window. You may not need a graph.

## Summary
- 76 nodes · 108 edges · 10 communities detected
- Extraction: 99% EXTRACTED · 1% INFERRED · 0% AMBIGUOUS · INFERRED: 1 edges (avg confidence: 0.7)
- Token cost: 0 input · 0 output

## God Nodes (most connected - your core abstractions)
1. `AppDbContext` - 13 edges
2. `RecipeService` - 13 edges
3. `IRecipeService` - 12 edges
4. `PantryViewModel` - 11 edges
5. `ShoppingListViewModel` - 11 edges
6. `BaseViewModel` - 10 edges
7. `HomeViewModel` - 10 edges
8. `RecipeDetailViewModel` - 10 edges
9. `RelayCommand` - 10 edges
10. `Recipe` - 9 edges

## Surprising Connections (you probably didn't know these)
- `PantryViewModel` --semantically_similar_to--> `ShoppingListViewModel`  [INFERRED] [semantically similar]
  PantryToPlate.Core/ViewModels/PantryViewModel.cs → PantryToPlate.Core/ViewModels/ShoppingListViewModel.cs
- `RecipeService` --references--> `AppDbContext`  [EXTRACTED]
  PantryToPlate.Core/Services/RecipeService.cs → PantryToPlate.Core/Data/AppDbContext.cs
- `AppDbContext` --references--> `PantryItem`  [EXTRACTED]
  PantryToPlate.Core/Data/AppDbContext.cs → PantryToPlate.Core/Models/PantryItem.cs
- `PantryViewModel` --references--> `AppDbContext`  [EXTRACTED]
  PantryToPlate.Core/ViewModels/PantryViewModel.cs → PantryToPlate.Core/Data/AppDbContext.cs
- `ShoppingListViewModel` --references--> `AppDbContext`  [EXTRACTED]
  PantryToPlate.Core/ViewModels/ShoppingListViewModel.cs → PantryToPlate.Core/Data/AppDbContext.cs

## Hyperedges (group relationships)
- **MVVM Pattern ViewModels** — baseviewmodel_baseviewmodel, relaycommand_relaycommand, homeviewmodel_homeviewmodel, recipedetailviewmodel_recipedetailviewmodel, recipeeditorviewmodel_recipeeditorviewmodel, pantryviewmodel_pantryviewmodel, shoppinglistviewmodel_shoppinglistviewmodel [EXTRACTED 0.95]
- **EF Core Data Layer** — appdbcontext_appdbcontext, databaseseeder_databaseseeder, recipe_recipe, ingredient_ingredient, recipeingredient_recipeingredient, pantryitem_pantryitem, shoppinglistitem_shoppinglistitem [EXTRACTED 0.95]
- **Recipe Availability Flow** — recipeservice_recipeservice, pantryitem_pantryitem, recipeingredient_recipeingredient, recipe_recipe [EXTRACTED 0.90]

## Communities

### Community 0 - "EF Core Data Layer"
Cohesion: 0.17
Nodes (7): AppDbContext, DatabaseSeeder, DbContext, Ingredient, Recipe, RecipeIngredient, ShoppingListItem

### Community 1 - "Recipe Service Contract"
Cohesion: 0.13
Nodes (3): IRecipeService, IRecipeService, RecipeService

### Community 2 - "Home & Recipe List VM"
Cohesion: 0.18
Nodes (3): HomeViewModel, ICommand, RelayCommand

### Community 3 - "MVVM Base Infrastructure"
Cohesion: 0.28
Nodes (4): BaseViewModel, BaseViewModel, INotifyPropertyChanged, RecipeEditorViewModel

### Community 4 - "Navigation & Recipe Detail"
Cohesion: 0.25
Nodes (2): INavigationService, RecipeDetailViewModel

### Community 5 - "Pantry Management"
Cohesion: 0.29
Nodes (2): PantryItem, PantryViewModel

### Community 6 - "Shopping List"
Cohesion: 0.4
Nodes (1): ShoppingListViewModel

### Community 7 - "Test Service"
Cohesion: 1.0
Nodes (1): TestService

### Community 8 - "Assembly Metadata"
Cohesion: 1.0
Nodes (0): 

### Community 9 - "Global Usings"
Cohesion: 1.0
Nodes (0): 

## Knowledge Gaps
- **1 isolated node(s):** `TestService`
  These have ≤1 connection - possible missing edges or undocumented components.
- **Thin community `Test Service`** (2 nodes): `TestService.cs`, `TestService`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Assembly Metadata`** (1 nodes): `PantryToPlate.Core.AssemblyInfo.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.
- **Thin community `Global Usings`** (1 nodes): `PantryToPlate.Core.GlobalUsings.g.cs`
  Too small to be a meaningful cluster - may be noise or needs more connections extracted.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `RecipeService` connect `Recipe Service Contract` to `EF Core Data Layer`, `Pantry Management`?**
  _High betweenness centrality (0.206) - this node is a cross-community bridge._
- **Why does `IRecipeService` connect `Recipe Service Contract` to `EF Core Data Layer`, `Home & Recipe List VM`, `MVVM Base Infrastructure`, `Navigation & Recipe Detail`?**
  _High betweenness centrality (0.196) - this node is a cross-community bridge._
- **Why does `AppDbContext` connect `EF Core Data Layer` to `Recipe Service Contract`, `Pantry Management`, `Shopping List`?**
  _High betweenness centrality (0.191) - this node is a cross-community bridge._
- **What connects `TestService` to the rest of the system?**
  _1 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Recipe Service Contract` be split into smaller, more focused modules?**
  _Cohesion score 0.13 - nodes in this community are weakly interconnected._