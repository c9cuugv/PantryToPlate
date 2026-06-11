using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.ViewModels;

public class ShoppingListViewModelTests
{
    private readonly AppDbContext _dbContext;
    private readonly ShoppingListViewModel _viewModel;

    public ShoppingListViewModelTests()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _viewModel = new ShoppingListViewModel(_dbContext);
    }

    [Fact]
    public void ShoppingListItem_PurchaseAmountText_ShouldShowQuantityAndUnit()
    {
        var item = new ShoppingListItem { QuantityToBuy = 2.50m, Unit = "cups" };

        Assert.Equal("2.5 cups", item.PurchaseAmountText);
    }

    [Fact]
    public async Task TogglePurchasedAsync_ShouldFlipStatus()
    {
        // Arrange
        var item = new ShoppingListItem { Ingredient = new Ingredient { Name = "Milk" }, IsPurchased = false };
        _dbContext.ShoppingList.Add(item);
        await _dbContext.SaveChangesAsync();

        // Act
        await _viewModel.TogglePurchasedAsync(item);

        // Assert
        Assert.True(item.IsPurchased);
        var dbItem = _dbContext.ShoppingList.First();
        Assert.True(dbItem.IsPurchased);
    }

    [Fact]
    public async Task ClearPurchasedAsync_ShouldRemoveOnlyPurchasedItems()
    {
        // Arrange
        var item1 = new ShoppingListItem { IsPurchased = true, Ingredient = new Ingredient { Name = "I1" } };
        var item2 = new ShoppingListItem { IsPurchased = false, Ingredient = new Ingredient { Name = "I2" } };
        _dbContext.ShoppingList.AddRange(item1, item2);
        await _dbContext.SaveChangesAsync();

        _viewModel.ShoppingListItems.Add(item1);
        _viewModel.ShoppingListItems.Add(item2);

        // Act
        await _viewModel.ClearPurchasedAsync();

        // Assert
        Assert.Single(_dbContext.ShoppingList.ToList());
        Assert.Equal("I2", _dbContext.ShoppingList.First().Ingredient.Name);
        Assert.DoesNotContain(item1, _viewModel.ShoppingListItems);
        Assert.Contains(item2, _viewModel.ShoppingListItems);
    }

    [Fact]
    public async Task RestockToPantryAsync_ShouldRestockAndRemovePurchasedItems()
    {
        // Arrange
        var tomato = new Ingredient { Name = "Tomato" };
        var pasta = new Ingredient { Name = "Pasta" };
        var milk = new Ingredient { Name = "Milk" };
        _dbContext.Ingredients.AddRange(tomato, pasta, milk);
        await _dbContext.SaveChangesAsync();

        var recipe = new Recipe { Name = "Dummy Recipe", Instructions = "Mix." };
        _dbContext.Recipes.Add(recipe);
        await _dbContext.SaveChangesAsync();

        _dbContext.RecipeIngredients.Add(new RecipeIngredient { RecipeId = recipe.Id, IngredientId = tomato.Id, QuantityRequired = 1, Unit = "unit" });
        _dbContext.RecipeIngredients.Add(new RecipeIngredient { RecipeId = recipe.Id, IngredientId = pasta.Id, QuantityRequired = 100, Unit = "grams" });

        var item1 = new ShoppingListItem { IsPurchased = true, Ingredient = tomato, IngredientId = tomato.Id };
        var item2 = new ShoppingListItem { IsPurchased = true, Ingredient = pasta, IngredientId = pasta.Id };
        var item3 = new ShoppingListItem { IsPurchased = false, Ingredient = milk, IngredientId = milk.Id };
        _dbContext.ShoppingList.AddRange(item1, item2, item3);
        await _dbContext.SaveChangesAsync();

        _viewModel.ShoppingListItems.Add(item1);
        _viewModel.ShoppingListItems.Add(item2);
        _viewModel.ShoppingListItems.Add(item3);

        _dbContext.Pantry.Add(new PantryItem { IngredientId = tomato.Id, QuantityInStock = 2, Unit = "unit" });
        await _dbContext.SaveChangesAsync();

        // Act
        await _viewModel.RestockToPantryAsync();

        // Assert
        var shoppingItems = _dbContext.ShoppingList.ToList();
        Assert.Single(shoppingItems);
        Assert.Equal("Milk", shoppingItems[0].Ingredient.Name);
        Assert.DoesNotContain(item1, _viewModel.ShoppingListItems);
        Assert.DoesNotContain(item2, _viewModel.ShoppingListItems);
        Assert.Contains(item3, _viewModel.ShoppingListItems);

        var pantryItems = _dbContext.Pantry.ToList();
        Assert.Equal(2, pantryItems.Count);

        var tomatoPantry = pantryItems.Single(p => p.IngredientId == tomato.Id);
        Assert.Equal("unit", tomatoPantry.Unit);
        Assert.Equal(8, tomatoPantry.QuantityInStock); // 2 existing + 6 default

        var pastaPantry = pantryItems.Single(p => p.IngredientId == pasta.Id);
        Assert.Equal("grams", pastaPantry.Unit);
        Assert.Equal(500, pastaPantry.QuantityInStock); // 0 + 500 default
    }

    [Fact]
    public async Task RestockToPantryAsync_ShouldUseShoppingListQuantityAndUnit()
    {
        var tomato = new Ingredient { Name = "Tomato" };
        _dbContext.Ingredients.Add(tomato);
        await _dbContext.SaveChangesAsync();

        var item = new ShoppingListItem
        {
            IsPurchased = true,
            Ingredient = tomato,
            IngredientId = tomato.Id,
            QuantityToBuy = 2,
            Unit = "whole"
        };
        _dbContext.ShoppingList.Add(item);
        await _dbContext.SaveChangesAsync();

        _viewModel.ShoppingListItems.Add(item);

        await _viewModel.RestockToPantryAsync();

        var pantryItem = _dbContext.Pantry.Single(p => p.IngredientId == tomato.Id);
        Assert.Equal(2, pantryItem.QuantityInStock);
        Assert.Equal("whole", pantryItem.Unit);
    }
}
