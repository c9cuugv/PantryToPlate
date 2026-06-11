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
        _recipeServiceMock.Setup(s => s.GetAvailableRecipesAsync()).ReturnsAsync(new List<Recipe>());
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
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(recipes);

        // Act
        await _viewModel.LoadRecipesAsync();

        // Assert
        Assert.Equal(2, _viewModel.Recipes.Count);
        Assert.Equal("Recipe 1", _viewModel.Recipes[0].Name);
        Assert.Equal("Recipe 2", _viewModel.Recipes[1].Name);
        _recipeServiceMock.Verify(s => s.GetAllRecipesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoadRecipesAsync_ShouldSuggestAvailableRecipesFirst()
    {
        var available = new Recipe { Id = 2, Name = "Available Recipe" };
        var unavailable = new Recipe { Id = 1, Name = "Unavailable Recipe" };
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(new List<Recipe> { unavailable, available });
        _recipeServiceMock.Setup(s => s.GetAvailableRecipesAsync()).ReturnsAsync(new List<Recipe> { available });

        await _viewModel.LoadRecipesAsync();

        Assert.Equal(available.Id, _viewModel.Recipes[0].Id);
        Assert.True(_viewModel.Recipes[0].CanMake);
        Assert.False(_viewModel.Recipes[1].CanMake);
    }

    [Fact]
    public async Task LoadRecipesAsync_ShouldWaitForDatabaseInitialization()
    {
        var initializer = new TestDatabaseInitializer();
        var viewModel = new HomeViewModel(_recipeServiceMock.Object, _navigationServiceMock.Object, initializer);
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(new List<Recipe>());

        var loadTask = viewModel.LoadRecipesAsync();
        await Task.Delay(20);

        _recipeServiceMock.Verify(s => s.GetAllRecipesAsync(), Times.Never);

        initializer.Complete();
        await loadTask;

        _recipeServiceMock.Verify(s => s.GetAllRecipesAsync(), Times.Once);
    }

    [Fact]
    public async Task LoadRecipesAsync_ShouldSetIsLoading()
    {
        // Arrange
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).Returns(async () =>
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

    [Fact]
    public void HasRecipes_IsFalse_Initially()
    {
        Assert.False(_viewModel.HasRecipes);
    }

    [Fact]
    public async Task HasRecipes_IsTrue_AfterLoadRecipesAsync_WithResults()
    {
        var recipes = new List<Recipe> { new Recipe { Id = 1, Name = "Pasta" } };
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(recipes);

        await _viewModel.LoadRecipesAsync();

        Assert.True(_viewModel.HasRecipes);
    }

    [Fact]
    public async Task HasRecipes_IsFalse_AfterLoadRecipesAsync_WithNoResults()
    {
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(new List<Recipe>());

        await _viewModel.LoadRecipesAsync();

        Assert.False(_viewModel.HasRecipes);
    }

    [Fact]
    public async Task HasRecipes_RaisesPropertyChanged_AfterLoad()
    {
        var recipes = new List<Recipe> { new Recipe { Id = 1, Name = "Pasta" } };
        _recipeServiceMock.Setup(s => s.GetAllRecipesAsync()).ReturnsAsync(recipes);

        var changedProps = new List<string>();
        _viewModel.PropertyChanged += (_, e) => changedProps.Add(e.PropertyName ?? "");

        await _viewModel.LoadRecipesAsync();

        Assert.Contains("HasRecipes", changedProps);
    }

    private sealed class TestDatabaseInitializer : IDatabaseInitializer
    {
        private readonly TaskCompletionSource completion = new();

        public Task Initialized => completion.Task;

        public void Start()
        {
        }

        public void Complete()
        {
            completion.SetResult();
        }
    }
}
