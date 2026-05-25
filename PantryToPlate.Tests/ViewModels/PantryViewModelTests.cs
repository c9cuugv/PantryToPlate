using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.ViewModels;

public class PantryViewModelTests
{
    private readonly AppDbContext _dbContext;
    private readonly PantryViewModel _viewModel;

    public PantryViewModelTests()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _viewModel = new PantryViewModel(_dbContext);
    }

    [Fact]
    public async Task AddPantryItemAsync_ShouldAddIngredientAndPantryItem()
    {
        // Arrange
        _viewModel.NewIngredientName = "Tomato";
        _viewModel.NewQuantity = 5;
        _viewModel.NewUnit = "pcs";

        // Act
        await _viewModel.AddPantryItemAsync();

        // Assert
        var ingredient = _dbContext.Ingredients.FirstOrDefault(i => i.Name == "Tomato");
        Assert.NotNull(ingredient);

        var pantryItem = _dbContext.Pantry.FirstOrDefault(p => p.IngredientId == ingredient.Id);
        Assert.NotNull(pantryItem);
        Assert.Equal(5, pantryItem.QuantityInStock);
        Assert.Equal("pcs", pantryItem.Unit);
        
        Assert.Contains(pantryItem, _viewModel.PantryItems);
    }

    [Fact]
    public async Task DeletePantryItemAsync_ShouldRemoveFromDbAndCollection()
    {
        // Arrange
        var ingredient = new Ingredient { Name = "Onion" };
        var pantryItem = new PantryItem { Ingredient = ingredient, QuantityInStock = 2, Unit = "kg" };
        _dbContext.Ingredients.Add(ingredient);
        _dbContext.Pantry.Add(pantryItem);
        await _dbContext.SaveChangesAsync();

        _viewModel.PantryItems.Add(pantryItem);

        // Act
        await _viewModel.DeletePantryItemAsync(pantryItem);

        // Assert
        Assert.Empty(_dbContext.Pantry.ToList());
        Assert.DoesNotContain(pantryItem, _viewModel.PantryItems);
    }
}
