using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;

    private ObservableCollection<Recipe> recipes = new();
    public ObservableCollection<Recipe> Recipes { get => recipes; set => SetProperty(ref recipes, value); }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    private readonly INavigationService _navigationService;

    public HomeViewModel()
    {
        _recipeService = null!;
        _navigationService = null!;
    }

    public HomeViewModel(IRecipeService recipeService, INavigationService navigationService)
    {
        _recipeService = recipeService;
        _navigationService = navigationService;
    }

    public ICommand LoadRecipesCommand => new RelayCommand(async () => await LoadRecipesAsync());
    public async Task LoadRecipesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var availableRecipes = await _recipeService.GetAvailableRecipesAsync();
            Recipes.Clear();
            foreach (var recipe in availableRecipes)
            {
                Recipes.Add(recipe);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand NavigateToRecipeCommand => new RelayCommand<Recipe>(async (recipe) => await NavigateToRecipeAsync(recipe));
    public async Task NavigateToRecipeAsync(Recipe recipe)
    {
        await _navigationService.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
    }

    public ICommand CreateRecipeCommand => new RelayCommand(async () => await CreateRecipeAsync());
    public async Task CreateRecipeAsync()
    {
        await _navigationService.GoToAsync("RecipeEditorPage");
    }
}