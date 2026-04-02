# Pantry-to-Plate MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a fully offline .NET MAUI mobile app that suggests recipes from pantry stock, auto-deducts ingredients on cooking confirmation, and generates a weekly shopping list.

**Architecture:** MVVM with CommunityToolkit.Mvvm; all data in local SQLite via EF Core; services abstracted behind interfaces for testability; XAML views bind to ViewModels with zero code-behind logic.

**Tech Stack:** .NET MAUI (.NET 9), C#, XAML, SQLite, EF Core 8, CommunityToolkit.Mvvm 8, xUnit (tests)

---

## File Map

```
PantryToPlate/                          ← MAUI app project
├── PantryToPlate.csproj
├── MauiProgram.cs                      ← DI + DB init on startup
├── AppShell.xaml / .cs                 ← Tab bar (Home | Pantry | Shopping List)
├── Models/
│   ├── Ingredient.cs
│   ├── Recipe.cs
│   ├── RecipeIngredient.cs
│   ├── PantryItem.cs
│   └── ShoppingListItem.cs
├── Data/
│   ├── AppDbContext.cs                 ← EF Core DbContext
│   └── DatabaseSeeder.cs              ← 6 staples + 25 recipes on first launch
├── Services/
│   ├── IRecipeService.cs
│   └── RecipeService.cs               ← GetAvailable + CookRecipeAsync
├── ViewModels/
│   ├── HomeViewModel.cs
│   ├── RecipeDetailViewModel.cs
│   ├── PantryViewModel.cs
│   └── ShoppingListViewModel.cs
├── Views/
│   ├── HomePage.xaml / .cs
│   ├── RecipeDetailPage.xaml / .cs
│   ├── PantryPage.xaml / .cs
│   └── ShoppingListPage.xaml / .cs
└── Converters/
    └── BoolToCheckIconConverter.cs     ← true→"✅" false→"❌"

PantryToPlate.Tests/
├── PantryToPlate.Tests.csproj
└── Services/
    └── RecipeServiceTests.cs
```

---

## Task 1: Project Scaffolding

**Files:**
- Create: `PantryToPlate/PantryToPlate.csproj` (via dotnet CLI)
- Create: `PantryToPlate.Tests/PantryToPlate.Tests.csproj`

- [ ] **Step 1: Scaffold MAUI project**

Run in `D:/aff/evi/New project/pantry_to _plate/`:
```bash
dotnet new maui -n PantryToPlate
```
Expected: `PantryToPlate/` folder created with default MAUI template.

- [ ] **Step 2: Add NuGet packages to app project**

```bash
cd "PantryToPlate"
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.10
dotnet add package CommunityToolkit.Mvvm --version 8.3.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.10
```

- [ ] **Step 3: Create test project**

```bash
cd ".."
dotnet new xunit -n PantryToPlate.Tests
cd "PantryToPlate.Tests"
dotnet add reference "../PantryToPlate/PantryToPlate.csproj"
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.10
dotnet add package CommunityToolkit.Mvvm --version 8.3.2
```

- [ ] **Step 4: Create solution and add both projects**

```bash
cd ".."
dotnet new sln -n PantryToPlate
dotnet sln add PantryToPlate/PantryToPlate.csproj
dotnet sln add PantryToPlate.Tests/PantryToPlate.Tests.csproj
```

- [ ] **Step 5: Verify build**

```bash
dotnet build PantryToPlate.sln
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 6: Commit**

```bash
git init
git add .
git commit -m "chore: scaffold MAUI project + test project"
```

---

## Task 2: Data Models

**Files:**
- Create: `PantryToPlate/Models/Ingredient.cs`
- Create: `PantryToPlate/Models/Recipe.cs`
- Create: `PantryToPlate/Models/RecipeIngredient.cs`
- Create: `PantryToPlate/Models/PantryItem.cs`
- Create: `PantryToPlate/Models/ShoppingListItem.cs`

- [ ] **Step 1: Create `Models/Ingredient.cs`**

```csharp
namespace PantryToPlate.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsStaple { get; set; }
}
```

- [ ] **Step 2: Create `Models/Recipe.cs`**

```csharp
namespace PantryToPlate.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<RecipeIngredient> RequiredIngredients { get; set; } = [];
}
```

- [ ] **Step 3: Create `Models/RecipeIngredient.cs`**

```csharp
namespace PantryToPlate.Models;

public class RecipeIngredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = string.Empty;
}
```

- [ ] **Step 4: Create `Models/PantryItem.cs`**

```csharp
namespace PantryToPlate.Models;

public class PantryItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityInStock { get; set; }
    public string Unit { get; set; } = string.Empty;
}
```

- [ ] **Step 5: Create `Models/ShoppingListItem.cs`**

```csharp
namespace PantryToPlate.Models;

public class ShoppingListItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public bool IsPurchased { get; set; }
}
```

- [ ] **Step 6: Build to confirm no errors**

```bash
dotnet build PantryToPlate/PantryToPlate.csproj
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 7: Commit**

```bash
git add PantryToPlate/Models/
git commit -m "feat: add data models"
```

---

## Task 3: AppDbContext

**Files:**
- Create: `PantryToPlate/Data/AppDbContext.cs`

- [ ] **Step 1: Create `Data/AppDbContext.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Models;

namespace PantryToPlate.Data;

public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<PantryItem> Pantry { get; set; }
    public DbSet<ShoppingListItem> ShoppingList { get; set; }

    private readonly string _dbPath;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RequiredIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId);

        modelBuilder.Entity<PantryItem>()
            .HasOne(p => p.Ingredient)
            .WithMany()
            .HasForeignKey(p => p.IngredientId);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(s => s.Ingredient)
            .WithMany()
            .HasForeignKey(s => s.IngredientId);
    }
}
```

- [ ] **Step 2: Build to confirm no errors**

```bash
dotnet build PantryToPlate/PantryToPlate.csproj
```

- [ ] **Step 3: Commit**

```bash
git add PantryToPlate/Data/AppDbContext.cs
git commit -m "feat: add AppDbContext"
```

---

## Task 4: DatabaseSeeder (Staples + 25 Recipes)

**Files:**
- Create: `PantryToPlate/Data/DatabaseSeeder.cs`

- [ ] **Step 1: Create `Data/DatabaseSeeder.cs`**

```csharp
using PantryToPlate.Models;

namespace PantryToPlate.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Ingredients.FindAsync(1) != null) return; // already seeded

        // --- Staples (IsStaple = true) ---
        var staples = new[]
        {
            new Ingredient { Name = "Water",        IsStaple = true },
            new Ingredient { Name = "Cooking Oil",  IsStaple = true },
            new Ingredient { Name = "Salt",         IsStaple = true },
            new Ingredient { Name = "Black Pepper", IsStaple = true },
            new Ingredient { Name = "Sugar",        IsStaple = true },
            new Ingredient { Name = "Butter",       IsStaple = true },
        };
        db.Ingredients.AddRange(staples);
        await db.SaveChangesAsync();

        // --- Non-staple ingredients ---
        var ing = new Dictionary<string, Ingredient>();
        var names = new[]
        {
            "Chicken Breast","Pasta","Tomato","Onion","Garlic",
            "Rice","Egg","Potato","Carrot","Cabbage",
            "Ground Beef","Bread","Cheese","Milk","Flour",
            "Lemon","Spinach","Mushroom","Bell Pepper","Zucchini",
            "Tuna (can)","Kidney Beans","Canned Tomatoes","Broccoli","Cucumber",
            "Salmon","Shrimp","Soy Sauce","Ginger","Tofu",
        };
        foreach (var n in names)
        {
            var i = new Ingredient { Name = n, IsStaple = false };
            db.Ingredients.Add(i);
            ing[n] = i;
        }
        await db.SaveChangesAsync();

        // Helper
        static RecipeIngredient RI(Ingredient i, decimal qty, string unit) =>
            new() { Ingredient = i, IngredientId = i.Id, QuantityRequired = qty, Unit = unit };

        // --- 25 Recipes ---
        var recipes = new List<Recipe>
        {
            new() { Name = "Spaghetti Bolognese",
                Instructions = "Brown beef. Add canned tomatoes and onion. Simmer 20 min. Serve over pasta.",
                RequiredIngredients = [
                    RI(ing["Ground Beef"], 300, "grams"),
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Canned Tomatoes"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Fried Rice",
                Instructions = "Fry onion and garlic. Add cooked rice and egg. Stir-fry 5 min.",
                RequiredIngredients = [
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Chicken Stir-Fry",
                Instructions = "Slice chicken and vegetables. Stir-fry in oil with soy sauce 10 min.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 300, "grams"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Broccoli"], 200, "grams"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Omelette",
                Instructions = "Whisk eggs. Pour into pan. Add mushroom and cheese. Fold and serve.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Mushroom"], 100, "grams"),
                    RI(ing["Cheese"], 50, "grams"),
                ]},

            new() { Name = "Tomato Pasta",
                Instructions = "Cook pasta. Sauté garlic and tomatoes. Toss together. Season.",
                RequiredIngredients = [
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Tomato"], 3, "whole"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Potato Soup",
                Instructions = "Boil diced potato and carrot. Blend. Season.",
                RequiredIngredients = [
                    RI(ing["Potato"], 4, "whole"),
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Milk"], 200, "ml"),
                ]},

            new() { Name = "Scrambled Eggs on Toast",
                Instructions = "Scramble eggs in butter. Toast bread. Serve on top.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Bread"], 2, "whole"),
                ]},

            new() { Name = "Chicken Rice Bowl",
                Instructions = "Pan-fry chicken. Serve over steamed rice with soy sauce.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 250, "grams"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Soy Sauce"], 20, "ml"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Tuna Pasta",
                Instructions = "Cook pasta. Mix with drained tuna, lemon juice, and onion.",
                RequiredIngredients = [
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Tuna (can)"], 1, "whole"),
                    RI(ing["Lemon"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Vegetable Soup",
                Instructions = "Simmer carrot, potato, zucchini, and onion in water 25 min. Season.",
                RequiredIngredients = [
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Potato"], 2, "whole"),
                    RI(ing["Zucchini"], 1, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Beef Tacos",
                Instructions = "Cook ground beef with onion and garlic. Serve in bread with tomato.",
                RequiredIngredients = [
                    RI(ing["Ground Beef"], 250, "grams"),
                    RI(ing["Tomato"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Bread"], 4, "whole"),
                    RI(ing["Cheese"], 60, "grams"),
                ]},

            new() { Name = "Garlic Mushroom Toast",
                Instructions = "Sauté mushrooms and garlic in butter. Serve on toasted bread.",
                RequiredIngredients = [
                    RI(ing["Mushroom"], 200, "grams"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Bread"], 2, "whole"),
                ]},

            new() { Name = "Spinach and Egg Stir-Fry",
                Instructions = "Fry garlic in oil. Add spinach. Push aside, scramble eggs in pan. Mix together.",
                RequiredIngredients = [
                    RI(ing["Spinach"], 200, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Pancakes",
                Instructions = "Mix flour, egg, milk. Fry in butter. Serve with sugar.",
                RequiredIngredients = [
                    RI(ing["Flour"], 150, "grams"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Milk"], 200, "ml"),
                ]},

            new() { Name = "Grilled Salmon",
                Instructions = "Brush salmon with oil and lemon juice. Grill 12 min. Season.",
                RequiredIngredients = [
                    RI(ing["Salmon"], 300, "grams"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Shrimp Fried Rice",
                Instructions = "Fry garlic, add shrimp and rice. Season with soy sauce.",
                RequiredIngredients = [
                    RI(ing["Shrimp"], 200, "grams"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Garlic"], 2, "whole"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Egg"], 1, "whole"),
                ]},

            new() { Name = "Tofu Stir-Fry",
                Instructions = "Cube tofu. Fry with bell pepper, broccoli and soy sauce.",
                RequiredIngredients = [
                    RI(ing["Tofu"], 300, "grams"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Broccoli"], 150, "grams"),
                    RI(ing["Soy Sauce"], 30, "ml"),
                    RI(ing["Ginger"], 10, "grams"),
                ]},

            new() { Name = "Cabbage Stir-Fry",
                Instructions = "Shred cabbage. Fry with garlic, soy sauce. Serve over rice.",
                RequiredIngredients = [
                    RI(ing["Cabbage"], 1, "whole"),
                    RI(ing["Garlic"], 3, "whole"),
                    RI(ing["Soy Sauce"], 25, "ml"),
                    RI(ing["Rice"], 150, "grams"),
                ]},

            new() { Name = "Cheese Omelette",
                Instructions = "Whisk eggs with milk. Cook in butter. Fill with cheese. Fold.",
                RequiredIngredients = [
                    RI(ing["Egg"], 3, "whole"),
                    RI(ing["Cheese"], 60, "grams"),
                    RI(ing["Milk"], 50, "ml"),
                ]},

            new() { Name = "Bean Rice Bowl",
                Instructions = "Cook kidney beans with onion and tomato. Serve on rice.",
                RequiredIngredients = [
                    RI(ing["Kidney Beans"], 1, "whole"),
                    RI(ing["Rice"], 200, "grams"),
                    RI(ing["Onion"], 1, "whole"),
                    RI(ing["Canned Tomatoes"], 1, "whole"),
                    RI(ing["Garlic"], 2, "whole"),
                ]},

            new() { Name = "Zucchini Pasta",
                Instructions = "Sauté zucchini and garlic. Toss with pasta.",
                RequiredIngredients = [
                    RI(ing["Zucchini"], 2, "whole"),
                    RI(ing["Pasta"], 200, "grams"),
                    RI(ing["Garlic"], 2, "whole"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Carrot Ginger Soup",
                Instructions = "Simmer carrot and ginger in water. Blend smooth. Season.",
                RequiredIngredients = [
                    RI(ing["Carrot"], 4, "whole"),
                    RI(ing["Ginger"], 20, "grams"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Greek Salad",
                Instructions = "Chop tomato, cucumber, bell pepper. Toss with lemon juice.",
                RequiredIngredients = [
                    RI(ing["Tomato"], 3, "whole"),
                    RI(ing["Cucumber"], 1, "whole"),
                    RI(ing["Bell Pepper"], 1, "whole"),
                    RI(ing["Cheese"], 80, "grams"),
                    RI(ing["Lemon"], 1, "whole"),
                ]},

            new() { Name = "Potato and Egg Hash",
                Instructions = "Dice potatoes. Fry with onion until golden. Add eggs and scramble.",
                RequiredIngredients = [
                    RI(ing["Potato"], 3, "whole"),
                    RI(ing["Egg"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},

            new() { Name = "Chicken Soup",
                Instructions = "Simmer chicken with carrot, potato and onion 30 min. Season.",
                RequiredIngredients = [
                    RI(ing["Chicken Breast"], 250, "grams"),
                    RI(ing["Carrot"], 2, "whole"),
                    RI(ing["Potato"], 2, "whole"),
                    RI(ing["Onion"], 1, "whole"),
                ]},
        };

        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Build to confirm no errors**

```bash
dotnet build PantryToPlate/PantryToPlate.csproj
```

- [ ] **Step 3: Commit**

```bash
git add PantryToPlate/Data/
git commit -m "feat: add DatabaseSeeder with 6 staples + 25 recipes"
```

---

## Task 5: RecipeService (with Tests)

**Files:**
- Create: `PantryToPlate/Services/IRecipeService.cs`
- Create: `PantryToPlate/Services/RecipeService.cs`
- Create: `PantryToPlate.Tests/Services/RecipeServiceTests.cs`

- [ ] **Step 1: Write failing tests first**

Create `PantryToPlate.Tests/Services/RecipeServiceTests.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Data;
using PantryToPlate.Models;
using PantryToPlate.Services;

namespace PantryToPlate.Tests.Services;

public class RecipeServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly RecipeService _sut;

    public RecipeServiceTests()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _db = new AppDbContext(dbPath);
        _db.Database.EnsureCreated();
        _sut = new RecipeService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private async Task SeedBasicDataAsync()
    {
        var tomato = new Ingredient { Name = "Tomato", IsStaple = false };
        var pasta  = new Ingredient { Name = "Pasta",  IsStaple = false };
        var oil    = new Ingredient { Name = "Oil",    IsStaple = true  };
        _db.Ingredients.AddRange(tomato, pasta, oil);
        await _db.SaveChangesAsync();

        var recipe = new Recipe
        {
            Name = "Tomato Pasta",
            Instructions = "Cook it.",
            RequiredIngredients =
            [
                new RecipeIngredient { Ingredient = tomato, QuantityRequired = 2, Unit = "whole" },
                new RecipeIngredient { Ingredient = pasta,  QuantityRequired = 100, Unit = "grams" },
                new RecipeIngredient { Ingredient = oil,    QuantityRequired = 1, Unit = "whole" },
            ]
        };
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAvailableRecipes_ReturnsRecipe_WhenAllNonStapleIngredientsSufficient()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        var pasta  = _db.Ingredients.Single(i => i.Name == "Pasta");
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 3, Unit = "whole" });
        _db.Pantry.Add(new PantryItem { Ingredient = pasta,  QuantityInStock = 200, Unit = "grams" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAvailableRecipesAsync();

        Assert.Single(result);
        Assert.Equal("Tomato Pasta", result[0].Name);
    }

    [Fact]
    public async Task GetAvailableRecipes_ExcludesRecipe_WhenIngredientInsufficient()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        // pasta missing from pantry
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 3, Unit = "whole" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAvailableRecipesAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAvailableRecipes_IgnoresStapleIngredients()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        var pasta  = _db.Ingredients.Single(i => i.Name == "Pasta");
        // oil is staple — NOT added to pantry
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 5, Unit = "whole" });
        _db.Pantry.Add(new PantryItem { Ingredient = pasta,  QuantityInStock = 200, Unit = "grams" });
        await _db.SaveChangesAsync();

        var result = await _sut.GetAvailableRecipesAsync();

        Assert.Single(result); // oil being absent should NOT block the recipe
    }

    [Fact]
    public async Task CookRecipeAsync_DeductsIngredients()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        var pasta  = _db.Ingredients.Single(i => i.Name == "Pasta");
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 5, Unit = "whole" });
        _db.Pantry.Add(new PantryItem { Ingredient = pasta,  QuantityInStock = 300, Unit = "grams" });
        await _db.SaveChangesAsync();

        var recipe = _db.Recipes.First();
        await _sut.CookRecipeAsync(recipe.Id);

        var tomatoStock = _db.Pantry.Single(p => p.IngredientId == tomato.Id).QuantityInStock;
        var pastaStock  = _db.Pantry.Single(p => p.IngredientId == pasta.Id).QuantityInStock;
        Assert.Equal(3, tomatoStock);   // 5 - 2
        Assert.Equal(200, pastaStock);  // 300 - 100
    }

    [Fact]
    public async Task CookRecipeAsync_AddsDepleted_ToShoppingList()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        var pasta  = _db.Ingredients.Single(i => i.Name == "Pasta");
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 2, Unit = "whole" });  // exact match → depletes
        _db.Pantry.Add(new PantryItem { Ingredient = pasta,  QuantityInStock = 500, Unit = "grams" });
        await _db.SaveChangesAsync();

        var recipe = _db.Recipes.First();
        await _sut.CookRecipeAsync(recipe.Id);

        var shopping = _db.ShoppingList.ToList();
        Assert.Single(shopping); // only tomato depleted
        Assert.Equal(tomato.Id, shopping[0].IngredientId);
    }

    [Fact]
    public async Task CookRecipeAsync_NoDuplicates_OnShoppingList()
    {
        await SeedBasicDataAsync();
        var tomato = _db.Ingredients.Single(i => i.Name == "Tomato");
        var pasta  = _db.Ingredients.Single(i => i.Name == "Pasta");
        _db.Pantry.Add(new PantryItem { Ingredient = tomato, QuantityInStock = 2, Unit = "whole" });
        _db.Pantry.Add(new PantryItem { Ingredient = pasta,  QuantityInStock = 500, Unit = "grams" });
        _db.ShoppingList.Add(new ShoppingListItem { Ingredient = tomato }); // already on list
        await _db.SaveChangesAsync();

        var recipe = _db.Recipes.First();
        await _sut.CookRecipeAsync(recipe.Id);

        var tomatoEntries = _db.ShoppingList.Count(s => s.IngredientId == tomato.Id);
        Assert.Equal(1, tomatoEntries); // no duplicate
    }
}
```

- [ ] **Step 2: Run tests — expect compile failure (RecipeService not yet created)**

```bash
dotnet test PantryToPlate.Tests/PantryToPlate.Tests.csproj
```
Expected: Build error — `RecipeService` and `IRecipeService` not found.

- [ ] **Step 3: Create `Services/IRecipeService.cs`**

```csharp
using PantryToPlate.Models;

namespace PantryToPlate.Services;

public interface IRecipeService
{
    Task<List<Recipe>> GetAvailableRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task CookRecipeAsync(int recipeId);
}
```

- [ ] **Step 4: Create `Services/RecipeService.cs`**

```csharp
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Data;
using PantryToPlate.Models;

namespace PantryToPlate.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _db;

    public RecipeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Recipe>> GetAvailableRecipesAsync()
    {
        var recipes = await _db.Recipes
            .Include(r => r.RequiredIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();

        var pantry = await _db.Pantry.ToListAsync();

        return recipes.Where(recipe =>
            recipe.RequiredIngredients
                .Where(ri => !ri.Ingredient.IsStaple)
                .All(ri =>
                {
                    var stock = pantry.FirstOrDefault(p => p.IngredientId == ri.IngredientId);
                    return stock != null && stock.QuantityInStock >= ri.QuantityRequired;
                })
        ).ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _db.Recipes
            .Include(r => r.RequiredIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task CookRecipeAsync(int recipeId)
    {
        var recipe = await _db.Recipes
            .Include(r => r.RequiredIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null) return;

        foreach (var ri in recipe.RequiredIngredients)
        {
            var pantryItem = await _db.Pantry
                .FirstOrDefaultAsync(p => p.IngredientId == ri.IngredientId);

            if (pantryItem == null) continue;

            pantryItem.QuantityInStock -= ri.QuantityRequired;

            if (pantryItem.QuantityInStock <= 0)
            {
                pantryItem.QuantityInStock = 0;
                var exists = await _db.ShoppingList
                    .AnyAsync(s => s.IngredientId == ri.IngredientId);
                if (!exists)
                    _db.ShoppingList.Add(new ShoppingListItem { IngredientId = ri.IngredientId });
            }
        }

        await _db.SaveChangesAsync();
    }
}
```

- [ ] **Step 5: Run tests — expect all pass**

```bash
dotnet test PantryToPlate.Tests/PantryToPlate.Tests.csproj --logger "console;verbosity=normal"
```
Expected: `Passed! - 6 tests`

- [ ] **Step 6: Commit**

```bash
git add PantryToPlate/Services/ PantryToPlate.Tests/Services/
git commit -m "feat: add RecipeService with tests"
```

---

## Task 6: ViewModels

**Files:**
- Create: `PantryToPlate/ViewModels/HomeViewModel.cs`
- Create: `PantryToPlate/ViewModels/RecipeDetailViewModel.cs`
- Create: `PantryToPlate/ViewModels/PantryViewModel.cs`
- Create: `PantryToPlate/ViewModels/ShoppingListViewModel.cs`

- [ ] **Step 1: Create `ViewModels/HomeViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Models;
using PantryToPlate.Services;

namespace PantryToPlate.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    private ObservableCollection<Recipe> _recipes = [];

    [ObservableProperty]
    private bool _isEmpty;

    [ObservableProperty]
    private string _emptyMessage = string.Empty;

    public HomeViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [RelayCommand]
    public async Task LoadRecipesAsync()
    {
        var available = await _recipeService.GetAvailableRecipesAsync();
        Recipes = new ObservableCollection<Recipe>(available);
        IsEmpty = Recipes.Count == 0;
        EmptyMessage = "You don't have enough ingredients for any recipe right now.";
    }
}
```

- [ ] **Step 2: Create `ViewModels/RecipeDetailViewModel.cs`**

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Data;
using PantryToPlate.Models;
using PantryToPlate.Services;

namespace PantryToPlate.ViewModels;

public record IngredientCheckItem(string Name, decimal QuantityRequired, string Unit, bool HasStock);

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;
    private readonly AppDbContext _db;

    [ObservableProperty] private Recipe? _recipe;
    [ObservableProperty] private List<IngredientCheckItem> _ingredientChecks = [];
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _hasError;
    [ObservableProperty] private bool _showInstructions = false;

    public int RecipeId { get; set; }

    // AppDbContext injected via constructor — NOT passed as RelayCommand parameter
    public RecipeDetailViewModel(IRecipeService recipeService, AppDbContext db)
    {
        _recipeService = recipeService;
        _db = db;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId);
        if (Recipe == null) { HasError = true; IsLoading = false; return; }

        var pantry = _db.Pantry.ToList();
        IngredientChecks = Recipe.RequiredIngredients
            .Where(ri => !ri.Ingredient.IsStaple)
            .Select(ri =>
            {
                var stock = pantry.FirstOrDefault(p => p.IngredientId == ri.IngredientId);
                var hasStock = stock != null && stock.QuantityInStock >= ri.QuantityRequired;
                return new IngredientCheckItem(ri.Ingredient.Name, ri.QuantityRequired, ri.Unit, hasStock);
            })
            .ToList();

        IsLoading = false;
    }

    [RelayCommand]
    private void ToggleInstructions() => ShowInstructions = !ShowInstructions;

    [RelayCommand]
    public async Task CookAsync()
    {
        await _recipeService.CookRecipeAsync(RecipeId);
    }
}
```

- [ ] **Step 3: Create `ViewModels/PantryViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Data;
using PantryToPlate.Models;

namespace PantryToPlate.ViewModels;

public partial class PantryViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<PantryItem> _items = [];
    [ObservableProperty] private string _newIngredientName = string.Empty;
    [ObservableProperty] private decimal _newQuantity;
    [ObservableProperty] private string _newUnit = "pieces";

    public PantryViewModel(AppDbContext db)
    {
        _db = db;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var list = await _db.Pantry
            .Include(p => p.Ingredient)
            .Where(p => !p.Ingredient.IsStaple)
            .ToListAsync();
        Items = new ObservableCollection<PantryItem>(list);
    }

    [RelayCommand]
    public async Task AddItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName) || NewQuantity <= 0) return;

        var ingredient = await _db.Ingredients
            .FirstOrDefaultAsync(i => i.Name.ToLower() == NewIngredientName.ToLower());

        if (ingredient == null)
        {
            ingredient = new Ingredient { Name = NewIngredientName, IsStaple = false };
            _db.Ingredients.Add(ingredient);
            await _db.SaveChangesAsync();
        }

        var existing = await _db.Pantry.FirstOrDefaultAsync(p => p.IngredientId == ingredient.Id);
        if (existing != null)
        {
            existing.QuantityInStock = NewQuantity;
            existing.Unit = NewUnit;
        }
        else
        {
            _db.Pantry.Add(new PantryItem
            {
                Ingredient = ingredient,
                QuantityInStock = NewQuantity,
                Unit = NewUnit
            });
        }

        await _db.SaveChangesAsync();
        NewIngredientName = string.Empty;
        NewQuantity = 0;
        await LoadAsync();
    }

    [RelayCommand]
    public async Task DeleteItemAsync(PantryItem item)
    {
        _db.Pantry.Remove(item);
        await _db.SaveChangesAsync();
        Items.Remove(item);
    }

    [RelayCommand]
    public async Task UpdateItemAsync(PantryItem item)
    {
        await _db.SaveChangesAsync();
    }
}
```

- [ ] **Step 4: Create `ViewModels/ShoppingListViewModel.cs`**

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Data;
using PantryToPlate.Models;

namespace PantryToPlate.ViewModels;

public partial class ShoppingListViewModel : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<ShoppingListItem> _items = [];
    [ObservableProperty] private bool _isEmpty;

    public ShoppingListViewModel(AppDbContext db)
    {
        _db = db;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var list = await _db.ShoppingList
            .Include(s => s.Ingredient)
            .ToListAsync();
        Items = new ObservableCollection<ShoppingListItem>(list);
        IsEmpty = Items.Count == 0;
    }

    [RelayCommand]
    public async Task TogglePurchasedAsync(ShoppingListItem item)
    {
        item.IsPurchased = !item.IsPurchased;
        await _db.SaveChangesAsync();
    }

    [RelayCommand]
    public async Task ClearPurchasedAsync()
    {
        var purchased = _db.ShoppingList.Where(s => s.IsPurchased);
        _db.ShoppingList.RemoveRange(purchased);
        await _db.SaveChangesAsync();
        await LoadAsync();
    }
}
```

- [ ] **Step 5: Build to confirm no errors**

```bash
dotnet build PantryToPlate/PantryToPlate.csproj
```

- [ ] **Step 6: Commit**

```bash
git add PantryToPlate/ViewModels/
git commit -m "feat: add all ViewModels"
```

---

## Task 7: Converter

**Files:**
- Create: `PantryToPlate/Converters/BoolToCheckIconConverter.cs`

- [ ] **Step 1: Create `Converters/BoolToCheckIconConverter.cs`**

```csharp
using System.Globalization;

namespace PantryToPlate.Converters;

public class BoolToCheckIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "✅" : "❌";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

- [ ] **Step 2: Commit**

```bash
git add PantryToPlate/Converters/
git commit -m "feat: add BoolToCheckIconConverter"
```

---

## Task 8: MauiProgram + DI Registration

**Files:**
- Modify: `PantryToPlate/MauiProgram.cs`

- [ ] **Step 1: Replace `MauiProgram.cs`**

```csharp
using Microsoft.Extensions.Logging;
using PantryToPlate.Data;
using PantryToPlate.Services;
using PantryToPlate.ViewModels;
using PantryToPlate.Views;

namespace PantryToPlate;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // DB
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pantrytoplate.db");
        builder.Services.AddSingleton(_ => new AppDbContext(dbPath));

        // Services
        builder.Services.AddSingleton<IRecipeService, RecipeService>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<PantryViewModel>();
        builder.Services.AddTransient<ShoppingListViewModel>();

        // Views (Pages)
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<PantryPage>();
        builder.Services.AddTransient<ShoppingListPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Init DB + seed
        Task.Run(async () =>
        {
            var db = app.Services.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await DatabaseSeeder.SeedAsync(db);
        }).GetAwaiter().GetResult();

        return app;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add PantryToPlate/MauiProgram.cs
git commit -m "feat: register DI, init DB + seeder on startup"
```

---

## Task 9: AppShell (Tab Navigation)

**Files:**
- Modify: `PantryToPlate/AppShell.xaml`
- Modify: `PantryToPlate/AppShell.xaml.cs`

- [ ] **Step 1: Replace `AppShell.xaml`**

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell
    x:Class="PantryToPlate.AppShell"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:views="clr-namespace:PantryToPlate.Views">

    <TabBar>
        <ShellContent Title="Home"          Icon="home.png"     Route="home"
                      ContentTemplate="{DataTemplate views:HomePage}" />
        <ShellContent Title="Pantry"        Icon="pantry.png"   Route="pantry"
                      ContentTemplate="{DataTemplate views:PantryPage}" />
        <ShellContent Title="Shopping List" Icon="cart.png"     Route="shopping"
                      ContentTemplate="{DataTemplate views:ShoppingListPage}" />
    </TabBar>

    <!-- Route for pushing RecipeDetailPage onto the nav stack -->
</Shell>
```

- [ ] **Step 2: Replace `AppShell.xaml.cs`**

```csharp
namespace PantryToPlate;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(Views.RecipeDetailPage), typeof(Views.RecipeDetailPage));
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add PantryToPlate/AppShell.xaml PantryToPlate/AppShell.xaml.cs
git commit -m "feat: configure shell tabs + recipe detail route"
```

---

## Task 10: Home Page (XAML + Code-Behind)

**Files:**
- Create: `PantryToPlate/Views/HomePage.xaml`
- Create: `PantryToPlate/Views/HomePage.xaml.cs`

- [ ] **Step 1: Create `Views/HomePage.xaml`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="PantryToPlate.Views.HomePage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:vm="clr-namespace:PantryToPlate.ViewModels"
    Title="What Can I Make Tonight?">

    <Grid RowDefinitions="*">

        <!-- Empty state -->
        <VerticalStackLayout
            IsVisible="{Binding IsEmpty}"
            HorizontalOptions="Center"
            VerticalOptions="Center"
            Spacing="8">
            <Label Text="🍽️" FontSize="48" HorizontalOptions="Center" />
            <Label Text="{Binding EmptyMessage}"
                   FontSize="16"
                   TextColor="Gray"
                   HorizontalTextAlignment="Center"
                   Margin="24,0" />
        </VerticalStackLayout>

        <!-- Recipe list -->
        <CollectionView
            ItemsSource="{Binding Recipes}"
            IsVisible="{Binding IsEmpty, Converter={StaticResource InverseBool}}"
            Margin="16">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Frame Margin="0,6" Padding="16" CornerRadius="12" HasShadow="True">
                        <Frame.GestureRecognizers>
                            <TapGestureRecognizer
                                Command="{Binding Source={RelativeSource AncestorType={x:Type vm:HomeViewModel}}, Path=NavigateToRecipeCommand}"
                                CommandParameter="{Binding .}" />
                        </Frame.GestureRecognizers>
                        <VerticalStackLayout Spacing="4">
                            <Label Text="{Binding Name}" FontSize="18" FontAttributes="Bold" />
                            <Label FontSize="13" TextColor="Gray">
                                <Label.FormattedText>
                                    <FormattedString>
                                        <Span Text="Uses: " />
                                        <Span Text="{Binding RequiredIngredients, Converter={StaticResource IngredientsToSummary}}" />
                                    </FormattedString>
                                </Label.FormattedText>
                            </Label>
                        </VerticalStackLayout>
                    </Frame>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

- [ ] **Step 2: Create `Views/HomePage.xaml.cs`**

```csharp
using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _vm;

    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadRecipesAsync();
    }
}
```

- [ ] **Step 3: Add `NavigateToRecipeCommand` and `IngredientsToSummary` converter to HomeViewModel**

Add to `ViewModels/HomeViewModel.cs`:
```csharp
[RelayCommand]
private async Task NavigateToRecipe(Recipe recipe)
{
    await Shell.Current.GoToAsync($"{nameof(RecipeDetailPage)}?RecipeId={recipe.Id}");
}
```

- [ ] **Step 4: Create `Converters/IngredientsToSummaryConverter.cs`**

```csharp
using System.Globalization;
using PantryToPlate.Models;

namespace PantryToPlate.Converters;

public class IngredientsToSummaryConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<RecipeIngredient> items) return string.Empty;
        return string.Join(", ", items
            .Where(i => !i.Ingredient.IsStaple)
            .Take(4)
            .Select(i => i.Ingredient.Name));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

- [ ] **Step 5: Create `Converters/InverseBoolConverter.cs`**

```csharp
using System.Globalization;

namespace PantryToPlate.Converters;

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
}
```

- [ ] **Step 6: Register converters in `App.xaml` ResourceDictionary**

In `App.xaml`, inside `<Application.Resources>`:
```xml
<ResourceDictionary>
    <converters:BoolToCheckIconConverter x:Key="BoolToCheckIcon" />
    <converters:InverseBoolConverter x:Key="InverseBool" />
    <converters:IngredientsToSummaryConverter x:Key="IngredientsToSummary" />
</ResourceDictionary>
```

Also add xmlns: `xmlns:converters="clr-namespace:PantryToPlate.Converters"`

- [ ] **Step 7: Commit**

```bash
git add PantryToPlate/Views/HomePage.xaml PantryToPlate/Views/HomePage.xaml.cs
git add PantryToPlate/Converters/ PantryToPlate/App.xaml
git add PantryToPlate/ViewModels/HomeViewModel.cs
git commit -m "feat: Home page with recipe list + navigation"
```

---

## Task 11: Recipe Detail Page

**Files:**
- Create: `PantryToPlate/Views/RecipeDetailPage.xaml`
- Create: `PantryToPlate/Views/RecipeDetailPage.xaml.cs`

- [ ] **Step 1: Create `Views/RecipeDetailPage.xaml`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="PantryToPlate.Views.RecipeDetailPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="{Binding Recipe.Name}">

    <Grid RowDefinitions="*, Auto">

        <ScrollView Grid.Row="0">
            <VerticalStackLayout Padding="16" Spacing="16">

                <!-- Error state -->
                <Label Text="Recipe not found." IsVisible="{Binding HasError}"
                       FontSize="16" TextColor="Red" />

                <!-- Ingredient Checklist -->
                <Label Text="Ingredients" FontSize="20" FontAttributes="Bold" />
                <CollectionView ItemsSource="{Binding IngredientChecks}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Grid ColumnDefinitions="40,*,Auto" Padding="0,6">
                                <Label Grid.Column="0"
                                       Text="{Binding HasStock, Converter={StaticResource BoolToCheckIcon}}"
                                       FontSize="20" />
                                <Label Grid.Column="1"
                                       Text="{Binding Name}"
                                       FontSize="15" VerticalOptions="Center" />
                                <Label Grid.Column="2"
                                       FontSize="13" TextColor="Gray" VerticalOptions="Center">
                                    <Label.FormattedText>
                                        <FormattedString>
                                            <Span Text="{Binding QuantityRequired}" />
                                            <Span Text=" " />
                                            <Span Text="{Binding Unit}" />
                                        </FormattedString>
                                    </Label.FormattedText>
                                </Label>
                            </Grid>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>

                <!-- Cooking Instructions (collapsible via tap) -->
                <VerticalStackLayout Spacing="8">
                    <Label Text="Cooking Instructions"
                           FontSize="18" FontAttributes="Bold">
                        <Label.GestureRecognizers>
                            <TapGestureRecognizer Command="{Binding ToggleInstructionsCommand}" />
                        </Label.GestureRecognizers>
                    </Label>
                    <Label Text="{Binding Recipe.Instructions}"
                           FontSize="14"
                           IsVisible="{Binding ShowInstructions}" />
                </VerticalStackLayout>

            </VerticalStackLayout>
        </ScrollView>

        <!-- Pinned CTA Button -->
        <Button
            Grid.Row="1"
            Text="Got it! I'm cooking this"
            Command="{Binding ShowConfirmationCommand}"
            BackgroundColor="#FF6B35"
            TextColor="White"
            FontSize="18"
            FontAttributes="Bold"
            Margin="16"
            CornerRadius="12"
            HeightRequest="56" />
    </Grid>
</ContentPage>
```

- [ ] **Step 2: Create `Views/RecipeDetailPage.xaml.cs`**

```csharp
using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

[QueryProperty(nameof(RecipeId), "RecipeId")]
public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _vm;

    public int RecipeId
    {
        set { _vm.RecipeId = value; }
    }

    // AppDbContext is already injected into RecipeDetailViewModel via DI
    public RecipeDetailPage(RecipeDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
```

- [ ] **Step 3: Add `ShowConfirmationCommand` to `RecipeDetailViewModel.cs`**

Add this method:
```csharp
[RelayCommand]
private async Task ShowConfirmation()
{
    bool confirmed = await Shell.Current.DisplayAlert(
        "Ready to Cook?",
        "Do you have all the ingredients ready?",
        "Yes",
        "Cancel");

    if (!confirmed) return;

    await CookAsync();
    await Shell.Current.GoToAsync("//home");
}
```

- [ ] **Step 4: Build to confirm no errors**

```bash
dotnet build PantryToPlate/PantryToPlate.csproj
```

- [ ] **Step 5: Commit**

```bash
git add PantryToPlate/Views/RecipeDetailPage.xaml PantryToPlate/Views/RecipeDetailPage.xaml.cs
git add PantryToPlate/ViewModels/RecipeDetailViewModel.cs
git commit -m "feat: Recipe Detail page with checklist + confirmation modal"
```

---

## Task 12: Pantry Page

**Files:**
- Create: `PantryToPlate/Views/PantryPage.xaml`
- Create: `PantryToPlate/Views/PantryPage.xaml.cs`

- [ ] **Step 1: Create `Views/PantryPage.xaml`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="PantryToPlate.Views.PantryPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="My Pantry">

    <Grid RowDefinitions="Auto,*">

        <!-- Add Item Form -->
        <VerticalStackLayout Grid.Row="0" Padding="16" Spacing="8"
                             BackgroundColor="#F5F5F5">
            <Label Text="Add Ingredient" FontAttributes="Bold" FontSize="16" />
            <Entry Placeholder="Ingredient name"
                   Text="{Binding NewIngredientName}" />
            <Grid ColumnDefinitions="*,*" ColumnSpacing="8">
                <Entry Grid.Column="0" Placeholder="Qty" Keyboard="Numeric"
                       Text="{Binding NewQuantity}" />
                <Picker Grid.Column="1" Title="Unit"
                        SelectedItem="{Binding NewUnit}">
                    <Picker.Items>
                        <x:String>pieces</x:String>
                        <x:String>grams</x:String>
                        <x:String>whole</x:String>
                        <x:String>ml</x:String>
                        <x:String>kg</x:String>
                        <x:String>liter</x:String>
                    </Picker.Items>
                </Picker>
            </Grid>
            <Button Text="Add to Pantry"
                    Command="{Binding AddItemCommand}"
                    BackgroundColor="#4CAF50"
                    TextColor="White"
                    CornerRadius="8" />
        </VerticalStackLayout>

        <!-- Pantry List -->
        <CollectionView Grid.Row="1"
                        ItemsSource="{Binding Items}"
                        Margin="16,8">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <SwipeView>
                        <SwipeView.RightItems>
                            <SwipeItems>
                                <SwipeItem Text="Delete"
                                           BackgroundColor="Red"
                                           Command="{Binding Source={RelativeSource AncestorType={x:Type ContentPage}}, Path=BindingContext.DeleteItemCommand}"
                                           CommandParameter="{Binding .}" />
                            </SwipeItems>
                        </SwipeView.RightItems>
                        <Grid ColumnDefinitions="*,Auto,Auto" Padding="4,12">
                            <Label Grid.Column="0" Text="{Binding Ingredient.Name}"
                                   FontSize="15" VerticalOptions="Center" />
                            <Label Grid.Column="1" FontSize="15"
                                   VerticalOptions="Center" Margin="0,0,4,0">
                                <Label.FormattedText>
                                    <FormattedString>
                                        <Span Text="{Binding QuantityInStock}" />
                                        <Span Text=" " />
                                        <Span Text="{Binding Unit}" />
                                    </FormattedString>
                                </Label.FormattedText>
                            </Label>
                        </Grid>
                    </SwipeView>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

- [ ] **Step 2: Create `Views/PantryPage.xaml.cs`**

```csharp
using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

public partial class PantryPage : ContentPage
{
    private readonly PantryViewModel _vm;

    public PantryPage(PantryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add PantryToPlate/Views/PantryPage.xaml PantryToPlate/Views/PantryPage.xaml.cs
git commit -m "feat: Pantry page with add/delete items"
```

---

## Task 13: Shopping List Page

**Files:**
- Create: `PantryToPlate/Views/ShoppingListPage.xaml`
- Create: `PantryToPlate/Views/ShoppingListPage.xaml.cs`

- [ ] **Step 1: Create `Views/ShoppingListPage.xaml`**

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage
    x:Class="PantryToPlate.Views.ShoppingListPage"
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Title="Shopping List">

    <Grid RowDefinitions="*,Auto">

        <!-- Empty state -->
        <VerticalStackLayout
            IsVisible="{Binding IsEmpty}"
            HorizontalOptions="Center" VerticalOptions="Center" Spacing="8">
            <Label Text="🛒" FontSize="48" HorizontalOptions="Center" />
            <Label Text="No items yet. Cook a meal to auto-fill this list."
                   FontSize="16" TextColor="Gray"
                   HorizontalTextAlignment="Center" Margin="24,0" />
        </VerticalStackLayout>

        <!-- Shopping list -->
        <CollectionView Grid.Row="0"
                        ItemsSource="{Binding Items}"
                        IsVisible="{Binding IsEmpty, Converter={StaticResource InverseBool}}"
                        Margin="16,8">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid ColumnDefinitions="Auto,*" Padding="4,10" ColumnSpacing="12">
                        <CheckBox Grid.Column="0"
                                  IsChecked="{Binding IsPurchased}"
                                  CheckedChanged="OnCheckedChanged" />
                        <Label Grid.Column="1"
                               Text="{Binding Ingredient.Name}"
                               FontSize="15"
                               VerticalOptions="Center"
                               TextDecorations="{Binding IsPurchased, Converter={StaticResource BoolToStrikethrough}}" />
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Clear button -->
        <Button Grid.Row="1"
                Text="Clear Purchased"
                Command="{Binding ClearPurchasedCommand}"
                BackgroundColor="#E0E0E0"
                TextColor="#555"
                Margin="16"
                CornerRadius="8" />
    </Grid>
</ContentPage>
```

- [ ] **Step 2: Create `Views/ShoppingListPage.xaml.cs`**

```csharp
using PantryToPlate.Models;
using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

public partial class ShoppingListPage : ContentPage
{
    private readonly ShoppingListViewModel _vm;

    public ShoppingListPage(ShoppingListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }

    private async void OnCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox cb && cb.BindingContext is ShoppingListItem item)
            await _vm.TogglePurchasedAsync(item);
    }
}
```

- [ ] **Step 3: Create `Converters/BoolToStrikethroughConverter.cs`**

```csharp
using System.Globalization;

namespace PantryToPlate.Converters;

public class BoolToStrikethroughConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TextDecorations.Strikethrough : TextDecorations.None;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
```

Also register it in `App.xaml`:
```xml
<converters:BoolToStrikethroughConverter x:Key="BoolToStrikethrough" />
```

- [ ] **Step 4: Build full solution**

```bash
dotnet build PantryToPlate.sln
```
Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 5: Run all tests**

```bash
dotnet test PantryToPlate.Tests/PantryToPlate.Tests.csproj --logger "console;verbosity=normal"
```
Expected: All tests pass.

- [ ] **Step 6: Commit**

```bash
git add PantryToPlate/Views/ShoppingListPage.xaml PantryToPlate/Views/ShoppingListPage.xaml.cs
git add PantryToPlate/Converters/BoolToStrikethroughConverter.cs
git add PantryToPlate/App.xaml
git commit -m "feat: Shopping List page + complete MVP build"
```

---

## Task 14: Final Verification

- [ ] **Step 1: Full build**

```bash
dotnet build PantryToPlate.sln
```
Expected: `0 Error(s)`

- [ ] **Step 2: All tests pass**

```bash
dotnet test PantryToPlate.Tests/PantryToPlate.Tests.csproj
```
Expected: All 6 tests pass.

- [ ] **Step 3: Verify PRD requirements coverage**

| Requirement | Task |
|---|---|
| Recipe suggestion from pantry | Task 5 (RecipeService) |
| Staples never block/show | Task 5 tests explicitly verify |
| ✅/❌ checklist | Task 11 (RecipeDetailPage) |
| "Got it! I'm cooking this" button | Task 11 |
| Confirmation modal | Task 11 (DisplayAlert) |
| Auto-deduct ingredients | Task 5 (CookRecipeAsync) |
| Shopping list auto-populated | Task 5 (CookRecipeAsync) |
| No duplicates on shopping list | Task 5 test |
| Pantry CRUD | Task 12 |
| 25 seeded recipes | Task 4 (DatabaseSeeder) |
| Offline / no accounts | All — no network calls |
| Empty state messages | Tasks 10, 13 |

- [ ] **Step 4: Final commit**

```bash
git add .
git commit -m "chore: Pantry-to-Plate MVP complete"
```
