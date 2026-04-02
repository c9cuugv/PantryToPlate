# Pantry-to-Plate — Design Spec
> Optimized for AI agent implementation. Read top-to-bottom. Each screen section is self-contained.

---

## How to Read This File

- Each screen has: **purpose**, **components**, **behavior**, and **data it reads/writes**
- Arrows in the flow mean: user action → next screen
- All local data is SQLite via EF Core. No network calls.

---

## App Flow (Linear)

```
[Home Screen]
    │
    │  user taps a recipe card
    ▼
[Recipe Detail Screen]
    │
    │  user taps "Got it! I'm cooking this"
    ▼
[Confirmation Modal]
    │
    │  user taps "Yes"
    ▼
[Auto Deduction Logic]  ← background, no screen
    │               │
    │               └──→ Shopping List updated
    ▼
[Home Screen]  ← returns here after deduction
```

---

## Screen 1 — Home

**Purpose:** Show every recipe the user can cook right now based on current pantry stock.

**Layout:**
- Full-width list of recipe cards
- Bottom tab bar: `Home` | `Pantry` | `Shopping List`

**Each recipe card shows:**
- Recipe name (bold)
- Short ingredient summary line e.g. `Uses: Chicken, Tomatoes, Pasta`

**Eligibility rule (implement exactly this):**
```
A recipe is shown ONLY IF:
  for every RecipeIngredient in recipe:
    PantryItem.QuantityInStock >= RecipeIngredient.QuantityRequired
    AND Ingredient.IsStaple == false
```

**Reads from DB:** `Recipes`, `RecipeIngredients`, `PantryItems`, `Ingredients`
**Writes to DB:** nothing

**On tap:** navigate to Recipe Detail Screen, pass `recipeId`

---

## Screen 2 — Recipe Detail

**Purpose:** Show full ingredient checklist and cooking instructions for one recipe.

**Layout:**
- Recipe name (heading)
- Ingredient checklist (scrollable list)
- Cooking instructions (below checklist, collapsible)
- "Got it! I'm cooking this" button (pinned to bottom)

**Each ingredient row shows:**
```
[✅ or ❌]  [Ingredient Name]  [QuantityRequired] [Unit]

✅ = PantryItem.QuantityInStock >= RecipeIngredient.QuantityRequired
❌ = insufficient stock
```

**Rules:**
- Never show ingredients where `Ingredient.IsStaple == true`
- Staples assumed always available (oil, salt, pepper, water)
- Show all non-staple ingredients regardless of tick/cross status

**Reads from DB:** `Recipes`, `RecipeIngredients`, `PantryItems`, `Ingredients`
**Writes to DB:** nothing

**On button tap:** open Confirmation Modal

---

## Screen 3 — Confirmation Modal

**Purpose:** One-step check before committing the cook action.

**Layout:**
- Modal overlay (center of screen)
- Text: `"Do you have all the ingredients ready?"`
- Two buttons: `Yes` (primary) and `Cancel` (secondary)

**On "Yes":** trigger `CookRecipeAsync(recipeId)` then dismiss modal and return to Home
**On "Cancel":** dismiss modal, stay on Recipe Detail

**Reads from DB:** nothing
**Writes to DB:** nothing (deduction happens in logic layer below)

---

## Logic Layer — CookRecipeAsync

**Trigger:** user taps "Yes" on Confirmation Modal
**This is a background operation. No screen is shown.**

**Exact steps:**
```
1. Load recipe with all RequiredIngredients (include navigation property)
2. For each RequiredIngredient:
   a. Find matching PantryItem by IngredientId
   b. Subtract: PantryItem.QuantityInStock -= RecipeIngredient.QuantityRequired
   c. If PantryItem.QuantityInStock <= 0:
        - Set PantryItem.QuantityInStock = 0
        - Check if IngredientId already exists in ShoppingList
        - If NOT exists: add new ShoppingListItem { IngredientId = ... }
3. SaveChangesAsync() — one single DB commit for everything
```

**Do NOT:**
- Ask the user how much they used
- Show any prompt or dialog during deduction
- Add partially-consumed items to the shopping list (only add when stock hits 0)

**Reads from DB:** `Recipes`, `RecipeIngredients`, `PantryItems`, `ShoppingList`
**Writes to DB:** `PantryItems` (update quantities), `ShoppingList` (insert depleted items)

---

## Screen 4 — Pantry

**Purpose:** Let the user view and manage their current grocery inventory.

**Layout:**
- List of all PantryItems
- Each row: `[Ingredient Name]  [QuantityInStock] [Unit]`
- Add button (opens inline form or bottom sheet)
- Tap a row to edit quantity or delete item

**Add/Edit form fields:**
- Ingredient name (text)
- Quantity (decimal number input)
- Unit (picker: `pieces` | `grams` | `whole` | `ml` | `kg`)

**Rules:**
- Staple items (`IsStaple == true`) are never shown here
- Deleting a pantry item does NOT remove it from the shopping list

**Reads from DB:** `PantryItems`, `Ingredients`
**Writes to DB:** `PantryItems` (insert / update / delete)

---

## Screen 5 — Shopping List

**Purpose:** Show all ingredients that need to be restocked for next week.

**Layout:**
- List of ShoppingListItems
- Each row: `[Checkbox]  [Ingredient Name]`
- "Clear purchased" button (removes all where `IsPurchased == true`)

**Rules:**
- Items are auto-added by `CookRecipeAsync` when stock hits 0
- User can manually check off items as they buy them (`IsPurchased = true`)
- Duplicate prevention: same `IngredientId` cannot appear twice on the list
- Checking off does NOT restore pantry stock (user must manually update pantry after shopping)

**Reads from DB:** `ShoppingList`, `Ingredients`
**Writes to DB:** `ShoppingList` (update `IsPurchased`, delete cleared items)

---

## Data Model Summary

```
Ingredient
  Id          int (PK)
  Name        string
  IsStaple    bool       ← true = never tracked, never shown

Recipe
  Id          int (PK)
  Name        string
  Instructions string

RecipeIngredient           ← join table
  Id                int (PK)
  RecipeId          int (FK → Recipe)
  IngredientId      int (FK → Ingredient)
  QuantityRequired  decimal
  Unit              string

PantryItem
  Id               int (PK)
  IngredientId     int (FK → Ingredient)
  QuantityInStock  decimal
  Unit             string

ShoppingListItem
  Id           int (PK)
  IngredientId int (FK → Ingredient)
  IsPurchased  bool
```

---

## Hardcoded Staples (IsStaple = true — seed on first launch)

Always available. Never blocked. Never on shopping list.

```
Water, Cooking oil, Salt, Black pepper, Sugar, Butter
```

---

## DB Seed — Phase 1 Recipe Count

Seed exactly **25 recipes** on first app launch if DB is empty.
Each recipe needs at minimum 3 and at most 8 non-staple ingredients.
Use simple, globally recognizable dishes (pasta, rice, eggs, chicken, vegetables).

---

## Navigation Structure

```
Tab 1: Home          → Recipe Detail → Confirmation Modal
Tab 2: Pantry        → Add/Edit Item (inline or sheet)
Tab 3: Shopping List → (no sub-navigation)
```

Back navigation: standard platform back gesture on all sub-screens.

---

## Unit Handling Rules

```
Use the SAME unit string between RecipeIngredient and PantryItem for the same ingredient.
Comparison is always: PantryItem.QuantityInStock >= RecipeIngredient.QuantityRequired
No unit conversion is performed. Units must match exactly.
Allowed units: "pieces" | "grams" | "whole" | "ml" | "kg" | "liter"
```

---

## Error States

| Situation | Behavior |
|---|---|
| Pantry is empty | Home shows 0 recipes. Show empty state: "Add items to your pantry to get started." |
| No recipes match pantry | Home shows 0 recipes. Show empty state: "You don't have enough ingredients for any recipe right now." |
| Recipe not found by ID | Recipe Detail shows error state. Do not crash. |
| Shopping list is empty | Show empty state: "No items yet. Cook a meal to auto-fill this list." |

---

## What the AI Agent Should NOT Do

- Do not ask the user how much of an ingredient they used
- Do not track staple ingredients (oil, salt, pepper, water, sugar, butter)
- Do not add partially-used ingredients to the shopping list
- Do not require login or network access
- Do not create cloud sync or remote API calls
- Do not let users create custom recipes in Phase 1
- Do not perform unit conversion between mismatched units
