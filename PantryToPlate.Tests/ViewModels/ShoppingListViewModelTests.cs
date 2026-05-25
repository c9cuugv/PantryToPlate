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
}
