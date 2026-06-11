using Moq;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using PantryToPlate.Core.ViewModels;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.ViewModels;

public class RecipeDetailViewModelTests
{
    private readonly Mock<IRecipeService> _recipeServiceMock;
    private readonly Mock<INavigationService> _navigationServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailViewModelTests()
    {
        _recipeServiceMock = new Mock<IRecipeService>();
        _navigationServiceMock = new Mock<INavigationService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _viewModel = new RecipeDetailViewModel(_recipeServiceMock.Object, _navigationServiceMock.Object, _dialogServiceMock.Object);
    }

    [Fact]
    public async Task LoadRecipeAsync_ShouldSetRecipe()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        _recipeServiceMock.Setup(s => s.GetRecipeByIdAsync(1)).ReturnsAsync(recipe);

        // Act
        await _viewModel.LoadRecipeAsync(1);

        // Assert
        Assert.NotNull(_viewModel.Recipe);
        Assert.Equal("Test Recipe", _viewModel.Recipe.Name);
        _recipeServiceMock.Verify(s => s.GetRecipeByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task CookRecipeAsync_ShouldCallServiceAndNavigate_WhenConfirmed()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        _viewModel.Recipe = recipe;
        _dialogServiceMock.Setup(d => d.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _viewModel.CookRecipeAsync();

        // Assert
        _recipeServiceMock.Verify(s => s.CookRecipeAsync(1), Times.Once);
        _navigationServiceMock.Verify(s => s.GoToAsync("..", false), Times.Once);
    }

    [Fact]
    public async Task CookRecipeAsync_ShouldAskUserToConfirmCooking()
    {
        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        _viewModel.Recipe = recipe;
        _dialogServiceMock.Setup(d => d.ShowConfirmAsync("Cook Recipe", "Great choice! Are you sure you want to make this?", "Yes", "No"))
            .ReturnsAsync(false);

        await _viewModel.CookRecipeAsync();

        _dialogServiceMock.Verify(d => d.ShowConfirmAsync("Cook Recipe", "Great choice! Are you sure you want to make this?", "Yes", "No"), Times.Once);
        _recipeServiceMock.Verify(s => s.CookRecipeAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CookRecipeAsync_ShouldNotCallServiceOrNavigate_WhenCancelled()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        _viewModel.Recipe = recipe;
        _dialogServiceMock.Setup(d => d.ShowConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _viewModel.CookRecipeAsync();

        // Assert
        _recipeServiceMock.Verify(s => s.CookRecipeAsync(1), Times.Never);
        _navigationServiceMock.Verify(s => s.GoToAsync("..", false), Times.Never);
    }

    [Fact]
    public async Task DeleteRecipeAsync_ShouldCallServiceAndNavigate()
    {
        // Arrange
        var recipe = new Recipe { Id = 1, Name = "Test Recipe" };
        _viewModel.Recipe = recipe;

        // Act
        await _viewModel.DeleteRecipeAsync();

        // Assert
        _recipeServiceMock.Verify(s => s.DeleteRecipeAsync(1), Times.Once);
        _navigationServiceMock.Verify(s => s.GoToAsync("..", false), Times.Once);
    }
}
