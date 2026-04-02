# Pantry-to-Plate
## Product Requirements Document
**Version:** 1.0 (Phase 1 — MVP)
**Platform:** iOS & Android (cross-platform)
**Status:** Pre-Development
**Date:** April 2026

---

## 1. Overview

Pantry-to-Plate is a smart, offline-first mobile meal planner and automated grocery tracker. The app solves the everyday problem of deciding what to cook for dinner by suggesting recipes based on ingredients the user already has at home. When the user commits to cooking a meal, the app automatically deducts the consumed ingredients from their digital pantry and adds the depleted items to a "Next Week" shopping list, making restocking effortless.

The philosophy of this product is simplicity and low friction. There are no accounts, no cloud sync, and no manual quantity logging beyond the initial pantry setup.

---

## 2. Goals & Non-Goals

### Goals
- Suggest dinner recipes based on the user's current pantry inventory.
- Show a visual ingredient checklist with tick marks for each recipe.
- Automatically deduct used ingredients from the pantry upon cooking confirmation.
- Auto-generate a weekly shopping list from depleted ingredients.
- Work entirely offline with zero account creation required.
- Support partial ingredient usage (e.g., 4 of 12 tomatoes).

### Non-Goals (Phase 1)
- Cloud sync or multi-device support.
- User accounts or social features.
- Custom recipe creation by the user.
- Exact weight-based inventory tracking (units used: pieces, whole, grams).
- Spice and pantry staple tracking (oil, salt, pepper, water are always assumed available).
- Nutritional information or calorie counting.

---

## 3. User Stories

**US-01**
As a user, I want to see a list of recipes I can make tonight based on what I have in my pantry, so I do not have to think about what to cook.

**US-02**
As a user, I want to tap a recipe and see a checklist of required ingredients with tick marks showing what I already have, so I can quickly confirm I am ready to cook.

**US-03**
As a user, I want to tap "Got it! I'm cooking this" to confirm my meal, after which the app silently deducts the used ingredients from my pantry without asking me any follow-up questions.

**US-04**
As a user, I want depleted ingredients to automatically appear on my "Next Week" shopping list so I know exactly what to buy to make the dish again.

**US-05**
As a user, I want to manage my pantry inventory by adding items with quantities, so the app knows what I have available.

---

## 4. Core Features & Requirements

### 4.1 Home Screen — What Can I Make Tonight?
- On launch, the home screen immediately displays a curated list of recipes the user can make based on their current pantry.
- A recipe is shown only if all non-staple ingredients required are in stock with sufficient quantity.
- Each recipe card shows the dish name and a brief descriptor (e.g., "Uses: Chicken, Tomatoes, Pasta").

### 4.2 Recipe Detail View
- Displays the dish name, a full ingredient checklist, and cooking instructions.
- Each ingredient row shows a ✅ green tick if the item is in the pantry in sufficient quantity, or a ❌ red cross if not.
- Common staples (oil, salt, pepper, water) are never shown in the checklist.
- A prominent **"Got it! I'm cooking this"** confirmation button is displayed at the bottom.

### 4.3 Cooking Confirmation Flow
- Tapping the confirmation button triggers a modal prompt: *"Do you have all the ingredients ready?"*
- Upon confirmation, the app silently deducts each ingredient's required quantity from the pantry.
- The app does **NOT** ask the user how much they used — it reads the exact quantity from the recipe data and deducts it automatically.
- If a pantry item's stock drops to zero or below after deduction, the item is flagged as depleted.

### 4.4 Partial Quantity Support
- Ingredients are tracked with decimal quantities and a unit (e.g., pieces, grams, whole).
- Example: If the pantry has 12 tomatoes and a recipe uses 4, the pantry updates to 8 tomatoes remaining.
- Example: If the pantry has 1 cabbage and a recipe uses 1, the pantry updates to 0 and the cabbage is added to the shopping list.
- Only fully depleted or over-consumed items are added to the shopping list.

### 4.5 Next Week Shopping List
- A dedicated tab shows all ingredients that have been depleted after cooking confirmation.
- Items are listed with their ingredient name and quantity needed to restore stock for making the same dish again.
- The user can manually check off items as they purchase them.
- Duplicates are prevented — if an item is already on the list, it is not added again.

### 4.6 Pantry Management
- A pantry screen allows the user to add, edit, and remove grocery items.
- Each item has a name, quantity, and unit (pieces, whole, grams, etc.).
- Simple toggle/quick-add for common items to reduce friction.
- Staple items (oil, salt, pepper, water) are never tracked or required.

### 4.7 Recipe Database
- Phase 1 ships with a hardcoded, pre-populated database of 20–30 simple recipes.
- Each recipe includes: name, instructions, and a list of required ingredients with exact quantity and unit.
- No custom recipe creation in Phase 1.

---

## 5. UI / UX Flow

### Screen 1: Home — What Can I Make Tonight?
- List of recipe cards filtered by current pantry.
- Bottom tab bar: **Home | Pantry | Shopping List**.

### Screen 2: Recipe Detail
- Ingredient checklist with ✅/❌ icons per item.
- Cooking instructions section (collapsible).
- **"Got it! I'm cooking this"** CTA button.
- Confirmation modal before finalizing.

### Screen 3: Pantry
- Grid or list of current pantry items with quantities.
- Add / edit / remove items.

### Screen 4: Shopping List
- List of depleted ingredients auto-populated after cooking.
- Checkboxes to mark items as purchased.
- Clear list option.

---

## 6. Data Models

The following C# entity classes define the local SQLite database schema.

### Recipe
```csharp
public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<RecipeIngredient> RequiredIngredients { get; set; } = [];
}
```

### Ingredient (Master Catalog)
```csharp
public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsStaple { get; set; } // if true, never tracked or shown in checklist
}
```

### RecipeIngredient (Join Table)
```csharp
public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = string.Empty; // "pieces", "grams", "whole", etc.
}
```

### PantryItem
```csharp
public class PantryItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityInStock { get; set; }
    public string Unit { get; set; } = string.Empty;
}
```

### ShoppingListItem
```csharp
public class ShoppingListItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public bool IsPurchased { get; set; }
}
```

### Database Context
```csharp
public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<PantryItem> Pantry { get; set; }
    public DbSet<ShoppingListItem> ShoppingList { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "pantrytoplate.db");
        optionsBuilder.UseSqlite($"Filename={dbPath}");
    }
}
```

---

## 7. Tech Stack

| Layer | Tool | Reason |
|---|---|---|
| UI Design | Google Stitch | AI-powered rapid prototyping. Output used as visual blueprint; actual UI built in XAML. |
| Framework | .NET MAUI (.NET 8/9) | Single codebase compiles to native iOS and Android. Leverages existing C# knowledge. |
| Language | C# | Strongly-typed, fast to debug complex inventory matching logic. |
| UI Markup | XAML | Native .NET MAUI UI language; recreates the Stitch visual designs. |
| Database | SQLite (local) | Embedded, serverless, on-device storage. No cloud dependency. Fully offline. |
| ORM | Entity Framework Core | Manages all SQLite operations using C# LINQ. No raw SQL needed. |
| Architecture | MVVM + CommunityToolkit.Mvvm | Decouples UI from business logic. Auto-refreshes the UI on pantry changes. |
| IDE | Visual Studio 2026 / JetBrains Rider | Full MAUI toolchain with iOS/Android emulator support. |

---

## 8. Key Business Logic

### Recipe Suggestion Algorithm
- For each recipe, check every non-staple `RecipeIngredient` against the `PantryItem` table.
- A recipe is eligible if: for every required ingredient, `PantryItem.QuantityInStock >= RecipeIngredient.QuantityRequired`.
- Return all eligible recipes sorted alphabetically or by fewest missing items.

### Cook Confirmation Logic (`CookRecipeAsync`)
```csharp
public async Task CookRecipeAsync(int recipeId, AppDbContext db)
{
    var recipe = await db.Recipes
        .Include(r => r.RequiredIngredients)
        .FirstOrDefaultAsync(r => r.Id == recipeId);

    if (recipe == null) return;

    foreach (var reqIngredient in recipe.RequiredIngredients)
    {
        var pantryItem = await db.Pantry
            .FirstOrDefaultAsync(p => p.IngredientId == reqIngredient.IngredientId);

        if (pantryItem != null)
        {
            pantryItem.QuantityInStock -= reqIngredient.QuantityRequired;

            if (pantryItem.QuantityInStock <= 0)
            {
                pantryItem.QuantityInStock = 0;

                var existsOnList = await db.ShoppingList
                    .AnyAsync(s => s.IngredientId == reqIngredient.IngredientId);

                if (!existsOnList)
                {
                    db.ShoppingList.Add(new ShoppingListItem
                    {
                        IngredientId = reqIngredient.IngredientId
                    });
                }
            }
        }
    }

    await db.SaveChangesAsync();
}
```

---

## 9. Phase 1 Scope & Simplification Rules

- Track ingredients by quantity (decimal) + unit rather than exact weight or volume where not needed.
- Assume water, cooking oil, salt, and pepper are always available — never block a suggestion or appear on the shopping list.
- Ship with 20–30 hardcoded recipes seeded into the local SQLite database on first launch.
- No user accounts, no cloud sync, no backend server.
- No custom recipe creation by the user in Phase 1.
- Do not prompt the user to specify how much of an ingredient they used — always auto-deduct from recipe data.

---

## 10. Success Metrics (Post-Launch)

- **Daily Active Usage:** User opens the app and confirms at least one meal per week.
- **Shopping List Accuracy:** Depleted ingredients consistently appear on the next week shopping list without manual intervention.
- **Pantry Accuracy:** After 4 weeks of use, pantry inventory remains within reasonable accuracy without manual corrections.
- **App Crash Rate:** 0% crashes on the cooking confirmation flow.

---

## 11. Open Questions

- Should the shopping list survive across weeks, or reset after the user marks all items purchased?
- What is the minimum pantry item count before the app can make its first suggestion?
- How should the app handle the same ingredient appearing on the shopping list from multiple depleted recipes?
- Should the user be able to manually mark a recipe as cooked without going through the full confirmation flow (e.g., if they cooked it outside the app)?

---

*Pantry-to-Plate PRD v1.0 — Internal*
