using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using System.Collections.ObjectModel;

namespace PantryToPlate.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    private ObservableCollection<Recipe> recipes = new();

    [ObservableProperty]
    private bool isLoading;

    public HomeViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [RelayCommand]
    private async Task LoadRecipesAsync()
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

    [RelayCommand]
    private async Task NavigateToRecipeAsync(Recipe recipe)
    {
        await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
    }

    [RelayCommand]
    private async Task CreateRecipeAsync()
    {
        await Shell.Current.GoToAsync("RecipeEditorPage");
    }
}