using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

namespace PantryToPlate.Tests.Integration;

public class AppWorkflowTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly RecipeService _recipeService;

    public AppWorkflowTests()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"integration_test_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Filename={dbPath}")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _recipeService = new RecipeService(_db);
        SeedTestData();
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    private void SeedTestData()
    {
        var tomato = new Ingredient { Name = "Tomato", IsStaple = false };
        var pasta = new Ingredient { Name = "Pasta", IsStaple = true };
        var oil = new Ingredient { Name = "Olive Oil", IsStaple = true };
        var garlic = new Ingredient { Name = "Garlic", IsStaple = false };

        _db.Ingredients.AddRange(tomato, pasta, oil, garlic);
        _db.SaveChanges();

        var pastaRecipe = new Recipe
        {
            Name = "Spaghetti Aglio e Olio",
            Instructions = "Cook pasta, fry garlic in oil, mix together"
        };

        _db.Recipes.Add(pastaRecipe);
        _db.SaveChanges();

        _db.RecipeIngredients.AddRange(
            new RecipeIngredient { RecipeId = pastaRecipe.Id, IngredientId = pasta.Id, QuantityRequired = 400, Unit = "g" },
            new RecipeIngredient { RecipeId = pastaRecipe.Id, IngredientId = oil.Id, QuantityRequired = 3, Unit = "tbsp" },
            new RecipeIngredient { RecipeId = pastaRecipe.Id, IngredientId = garlic.Id, QuantityRequired = 4, Unit = "cloves" }
        );
        _db.SaveChanges();

        _db.Pantry.AddRange(
            new PantryItem { IngredientId = pasta.Id, QuantityInStock = 500, Unit = "g" },
            new PantryItem { IngredientId = oil.Id, QuantityInStock = 1000, Unit = "ml" },
            new PantryItem { IngredientId = garlic.Id, QuantityInStock = 10, Unit = "cloves" }
        );
        _db.SaveChanges();
    }

    [Fact]
    public async Task UserFlow_BrowseRecipes_ShouldShowAvailableRecipes()
    {
        var recipes = await _recipeService.GetAvailableRecipesAsync();
        Assert.Single(recipes);
        Assert.Equal("Spaghetti Aglio e Olio", recipes.First().Name);
    }

    [Fact]
    public async Task UserFlow_CookRecipe_ShouldDeductFromPantry()
    {
        var recipe = _db.Recipes.First();
        var initialOil = _db.Pantry.First(p => p.Ingredient!.Name == "Olive Oil").QuantityInStock;

        await _recipeService.CookRecipeAsync(recipe.Id);

        var updatedOil = _db.Pantry.First(p => p.Ingredient!.Name == "Olive Oil").QuantityInStock;
        Assert.Equal(initialOil - 3, updatedOil);
    }

    [Fact]
    public async Task UserFlow_RemoveIngredient_ShouldMakeRecipeUnavailable()
    {
        var garlic = _db.Pantry.First(p => p.Ingredient!.Name == "Garlic");
        _db.Pantry.Remove(garlic);
        _db.SaveChanges();

        var recipes = await _recipeService.GetAvailableRecipesAsync();
        Assert.Empty(recipes);
    }

    [Fact]
    public async Task UserFlow_AddRecipe_ShouldWorkCorrectly()
    {
        var newRecipe = new Recipe
        {
            Name = "Tomato Sauce",
            Instructions = "Cook tomatoes with oil and garlic"
        };

        await _recipeService.AddRecipeAsync(newRecipe);

        var allRecipes = _db.Recipes.ToList();
        Assert.Equal(2, allRecipes.Count);
        Assert.Contains(allRecipes, r => r.Name == "Tomato Sauce");
    }
}
