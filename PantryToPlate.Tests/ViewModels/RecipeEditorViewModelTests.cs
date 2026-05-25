using Microsoft.EntityFrameworkCore;
using Moq;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using PantryToPlate.Core.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.ViewModels;

public class RecipeEditorViewModelTests : IDisposable
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<IRecipeImportService> _recipeImportServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly AppDbContext _db;
    private readonly RecipeEditorViewModel _viewModel;

    public RecipeEditorViewModelTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _recipeImportServiceMock = new Mock<IRecipeImportService>();
        _navigationServiceMock = new Mock<INavigationService>();

        var dbPath = Path.Combine(Path.GetTempPath(), $"test_viewmodel_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Filename={dbPath}")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _viewModel = new RecipeEditorViewModel(
            _recipeServiceMock.Object, 
            _recipeImportServiceMock.Object, 
            _db, 
            _navigationServiceMock.Object);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task SaveRecipeAsync_WithValidData_ShouldCallServiceAndNavigate()
    {
        // Arrange
        _viewModel.RecipeName = "New Recipe";
        _viewModel.Instructions = "Some instructions";

        // Act
        await _viewModel.SaveRecipeAsync();

        // Assert
        _recipeServiceMock.Verify(s => s.AddRecipeAsync(It.Is<Recipe>(r => r.Name == "New Recipe")), Times.Once);
        _navigationServiceMock.Verify(s => s.GoToAsync("..", true), Times.Once);
    }

    [Fact]
    public async Task SaveRecipeAsync_WithInvalidData_ShouldNotSave()
    {
        // Arrange
        _viewModel.RecipeName = "";
        _viewModel.Instructions = "";

        // Act
        await _viewModel.SaveRecipeAsync();

        // Assert
        _recipeServiceMock.Verify(s => s.AddRecipeAsync(It.IsAny<Recipe>()), Times.Never);
        _navigationServiceMock.Verify(s => s.GoToAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }
}
