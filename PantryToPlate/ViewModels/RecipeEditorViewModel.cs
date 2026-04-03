using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

namespace PantryToPlate.ViewModels;

public partial class RecipeEditorViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    private string recipeName = string.Empty;

    [ObservableProperty]
    private string instructions = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    public RecipeEditorViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [RelayCommand]
    private async Task SaveRecipeAsync()
    {
        if (string.IsNullOrWhiteSpace(RecipeName) || string.IsNullOrWhiteSpace(Instructions) || IsSaving)
            return;

        IsSaving = true;
        try
        {
            var recipe = new Recipe
            {
                Name = RecipeName,
                Instructions = Instructions,
                RequiredIngredients = new List<RecipeIngredient>() // empty to start
            };

            await _recipeService.AddRecipeAsync(recipe);
            await Shell.Current.GoToAsync("..", true);
        }
        finally
        {
            IsSaving = false;
        }
    }
}