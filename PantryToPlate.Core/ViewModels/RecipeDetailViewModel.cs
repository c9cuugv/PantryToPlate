using System.Collections.ObjectModel;
using System.Windows.Input;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;

namespace PantryToPlate.Core.ViewModels;

public partial class RecipeDetailViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;

    private Recipe? recipe;
    public Recipe? Recipe
    {
        get => recipe;
        set
        {
            if (SetProperty(ref recipe, value))
                BuildInstructionSteps();
        }
    }

    public ObservableCollection<string> InstructionSteps { get; } = new();

    private void BuildInstructionSteps()
    {
        InstructionSteps.Clear();
        if (Recipe is null || string.IsNullOrWhiteSpace(Recipe.Instructions))
            return;

        var steps = Recipe.Instructions
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Select((text, index) => $"{index + 1}. {text}");

        foreach (var step in steps)
            InstructionSteps.Add(step);
    }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    private bool isCooking;
    public bool IsCooking { get => isCooking; set => SetProperty(ref isCooking, value); }

    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;

    public RecipeDetailViewModel()
    {
        _recipeService = null!;
        _navigationService = null!;
        _dialogService = null!;
    }

    public RecipeDetailViewModel(IRecipeService recipeService, INavigationService navigationService, IDialogService dialogService)
    {
        _recipeService = recipeService;
        _navigationService = navigationService;
        _dialogService = dialogService;
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
            bool confirm = await _dialogService.ShowConfirmAsync("Cook Recipe", "Great choice! Are you sure you want to make this?", "Yes", "No");
            if (confirm)
            {
                await _recipeService.CookRecipeAsync(Recipe.Id);
                await _navigationService.GoToAsync("..");
            }
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