using System.Windows.Input;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

namespace PantryToPlate.Core.ViewModels;

public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;

    private Recipe? recipe;
    public Recipe? Recipe { get => recipe; set => SetProperty(ref recipe, value); }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    private bool isCooking;
    public bool IsCooking { get => isCooking; set => SetProperty(ref isCooking, value); }

    private readonly INavigationService _navigationService;

    public RecipeDetailViewModel()
    {
        _recipeService = null!;
        _navigationService = null!;
    }

    public RecipeDetailViewModel(IRecipeService recipeService, INavigationService navigationService)
    {
        _recipeService = recipeService;
        _navigationService = navigationService;
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

    public ICommand CookRecipeCommand => new RelayCommand(async () => await CookRecipeAsync());
    public async Task CookRecipeAsync()
    {
        if (Recipe is null || IsCooking) return;

        IsCooking = true;
        try
        {
            await _recipeService.CookRecipeAsync(Recipe.Id);
            await _navigationService.GoToAsync("..");
        }
        finally
        {
            IsCooking = false;
        }
    }

    public ICommand DeleteRecipeCommand => new RelayCommand(async () => await DeleteRecipeAsync());
    public async Task DeleteRecipeAsync()
    {
        if (Recipe is null || IsLoading) return;

        IsLoading = true;
        try
        {
            await _recipeService.DeleteRecipeAsync(Recipe.Id);
            await _navigationService.GoToAsync("..");
        }
        finally
        {
            IsLoading = false;
        }
    }
}