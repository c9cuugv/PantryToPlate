using Moq;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using PantryToPlate.Core.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.ViewModels;

public class HomeViewModelTests
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly HomeViewModel _viewModel;

    public HomeViewModelTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _viewModel = new HomeViewModel(_recipeServiceMock.Object, _navigationServiceMock.Object);
    }

    [Fact]
    public async Task LoadRecipesAsync_ShouldPopulateRecipes()
    {
        // Arrange
        var recipes = new List<Recipe>
        {
            new Recipe { Id = 1, Name = "Recipe 1" },
            new Recipe { Id = 2, Name = "Recipe 2" }
        };
        _recipeServiceMock.Setup(s => s.GetAvailableRecipesAsync()).ReturnsAsync(recipes);

        // Act
        await _viewModel.LoadRecipesAsync();

        // Assert
        Assert.Equal(2, _viewModel.Recipes.Count);
        Assert.Equal("Recipe 1", _viewModel.Recipes[0].Name);
        Assert.Equal("Recipe 2", _viewModel.Recipes[1].Name);
        _recipeServiceMock.Verify(s => s.GetAvailableRecipesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoadRecipesAsync_ShouldSetIsLoading()
    {
        // Arrange
        _recipeServiceMock.Setup(s => s.GetAvailableRecipesAsync()).Returns(async () =>
        {
            await Task.Delay(10);
            return new List<Recipe>();
        });

        // Act
        var loadTask = _viewModel.LoadRecipesAsync();
        
        // Assert
        Assert.True(_viewModel.IsLoading);
        
        await loadTask;
        Assert.False(_viewModel.IsLoading);
    }

    [Fact]
    public async Task NavigateToRecipeAsync_ShouldCallNavigationService()
    {
        // Arrange
        var recipe = new Recipe { Id = 5, Name = "Test Recipe" };

        // Act
        await _viewModel.NavigateToRecipeAsync(recipe);

        // Assert
        _navigationServiceMock.Verify(s => s.GoToAsync("RecipeDetailPage?recipeId=5", false), Times.Once);
    }

    [Fact]
    public async Task CreateRecipeAsync_ShouldCallNavigationService()
    {
        // Act
        await _viewModel.CreateRecipeAsync();

        // Assert
        _navigationServiceMock.Verify(s => s.GoToAsync("RecipeEditorPage", false), Times.Once);
    }
}
