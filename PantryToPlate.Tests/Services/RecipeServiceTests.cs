using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

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

    [Fact]
    public async Task AddRecipeAsync_AddsRecipeToDatabase()
    {
        await SeedBasicDataAsync();

        var mushroom = new Ingredient { Name = "Mushroom", IsStaple = false };
        _db.Ingredients.Add(mushroom);
        await _db.SaveChangesAsync();

        var newRecipe = new Recipe
        {
            Name = "Mushroom Toast",
            Instructions = "Toast bread with mushrooms.",
            RequiredIngredients =
            [
                new RecipeIngredient { Ingredient = mushroom, IngredientId = mushroom.Id, QuantityRequired = 3, Unit = "pieces" }
            ]
        };

        await _sut.AddRecipeAsync(newRecipe);

        var dbRecipe = await _db.Recipes.Include(r => r.RequiredIngredients).FirstOrDefaultAsync(r => r.Name == "Mushroom Toast");
        Assert.NotNull(dbRecipe);
        Assert.Equal("Mushroom Toast", dbRecipe!.Name);
        Assert.Single(dbRecipe.RequiredIngredients);
    }

    [Fact]
    public async Task DeleteRecipeAsync_RemovesRecipeFromDatabase()
    {
        await SeedBasicDataAsync();
        var recipe = _db.Recipes.First();

        await _sut.DeleteRecipeAsync(recipe.Id);

        var dbRecipe = await _db.Recipes.FindAsync(recipe.Id);
        Assert.Null(dbRecipe);
    }

    [Fact]
    public async Task UpdateRecipeAsync_UpdatesRecipeFieldsAndIngredients()
    {
        await SeedBasicDataAsync();
        var recipe = await _db.Recipes.Include(r => r.RequiredIngredients).FirstAsync();

        recipe.Name = "Updated Pasta";
        recipe.Instructions = "New instructions";
        recipe.RequiredIngredients =
            [
                new RecipeIngredient { IngredientId = recipe.RequiredIngredients.First().IngredientId, QuantityRequired = 1, Unit = "piece" }
            ];

        await _sut.UpdateRecipeAsync(recipe);

        var dbRecipe = await _db.Recipes.Include(r => r.RequiredIngredients).FirstOrDefaultAsync(r => r.Id == recipe.Id);
        Assert.NotNull(dbRecipe);
        Assert.Equal("Updated Pasta", dbRecipe!.Name);
        Assert.Equal("New instructions", dbRecipe.Instructions);
        Assert.Single(dbRecipe.RequiredIngredients);
        Assert.Equal(1, dbRecipe.RequiredIngredients[0].QuantityRequired);
    }
}
