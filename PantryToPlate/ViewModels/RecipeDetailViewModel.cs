using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

namespace PantryToPlate.ViewModels;

public partial class RecipeDetailViewModel : ObservableObject
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty]
    private Recipe? recipe;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isCooking;

    public RecipeDetailViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    public async Task LoadRecipeAsync(int recipeId)
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            Recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CookRecipeAsync()
    {
        if (Recipe is null || IsCooking) return;

        IsCooking = true;
        try
        {
            await _recipeService.CookRecipeAsync(Recipe.Id);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsCooking = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRecipeAsync()
    {
        if (Recipe is null || IsLoading) return;

        IsLoading = true;
        try
        {
            await _recipeService.DeleteRecipeAsync(Recipe.Id);
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsLoading = false;
        }
    }
}