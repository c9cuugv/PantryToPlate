using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IDatabaseInitializer? _databaseInitializer;

    private ObservableCollection<Recipe> recipes = new();
    public ObservableCollection<Recipe> Recipes
    {
        get => recipes;
        set
        {
            if (SetProperty(ref recipes, value))
                OnPropertyChanged(nameof(HasRecipes));
        }
    }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    public bool HasRecipes => Recipes.Count > 0;

    private readonly INavigationService _navigationService;

    public HomeViewModel()
    {
        _recipeService = null!;
        _navigationService = null!;
    }

    public HomeViewModel(IRecipeService recipeService, INavigationService navigationService, IDatabaseInitializer? databaseInitializer = null)
    {
        _recipeService = recipeService;
        _navigationService = navigationService;
        _databaseInitializer = databaseInitializer;
    }

    public ICommand LoadRecipesCommand => new RelayCommand(async () => await LoadRecipesAsync());
    public async Task LoadRecipesAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            if (_databaseInitializer is not null)
                await _databaseInitializer.Initialized;

            var allRecipes = await _recipeService.GetAllRecipesAsync();
            var availableRecipes = await _recipeService.GetAvailableRecipesAsync();
            var availableIds = availableRecipes.Select(r => r.Id).ToHashSet();

            foreach (var recipe in allRecipes)
            {
                recipe.CanMake = availableIds.Contains(recipe.Id);
            }

            var sortedRecipes = allRecipes
                .OrderByDescending(r => r.CanMake)
                .ThenBy(r => r.Name)
                .ToList();

            Recipes.Clear();
            foreach (var recipe in sortedRecipes)
            {
                Recipes.Add(recipe);
            }
            OnPropertyChanged(nameof(HasRecipes));
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
