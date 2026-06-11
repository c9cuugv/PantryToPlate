using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public partial class RecipeEditorViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IRecipeImportService _recipeImportService;
    private readonly AppDbContext _db;
    private readonly INavigationService _navigationService;

    public RecipeEditorViewModel()
    {
        // Parameterless constructor for XAML designer – services will be injected later.
        _recipeService = null!;
        _recipeImportService = null!;
        _db = null!;
        _navigationService = null!;
    }

    public RecipeEditorViewModel(IRecipeService recipeService, IRecipeImportService recipeImportService, AppDbContext db, INavigationService navigationService)
    {
        _recipeService = recipeService;
        _recipeImportService = recipeImportService;
        _db = db;
        _navigationService = navigationService;
    }

    private string recipeName = string.Empty;
    public string RecipeName { get => recipeName; set => SetProperty(ref recipeName, value); }

    private string instructions = string.Empty;
    public string Instructions { get => instructions; set => SetProperty(ref instructions, value); }

    private bool isSaving;
    public bool IsSaving { get => isSaving; set => SetProperty(ref isSaving, value); }

    // New properties for recipe import UI
    private string importUrl = string.Empty;
    public string ImportUrl { get => importUrl; set => SetProperty(ref importUrl, value); }

    private bool isLoadingRecipe;
    public bool IsLoadingRecipe { get => isLoadingRecipe; set => SetProperty(ref isLoadingRecipe, value); }

    private string importError = string.Empty;
    public string ImportError 
    { 
        get => importError; 
        set 
        {
            if (SetProperty(ref importError, value))
            {
                OnPropertyChanged(nameof(HasImportError));
            }
        } 
    }
    public bool HasImportError => !string.IsNullOrEmpty(ImportError);

    private string saveStatus = string.Empty;
    public string SaveStatus
    {
        get => saveStatus;
        set
        {
            if (SetProperty(ref saveStatus, value))
            {
                OnPropertyChanged(nameof(HasSaveStatus));
                OnPropertyChanged(nameof(IsSaveError));
            }
        }
    }
    public bool HasSaveStatus => !string.IsNullOrEmpty(SaveStatus);
    public bool IsSaveError => HasSaveStatus && SaveStatus.StartsWith("Error");

    // Collection of editable ingredient rows
    public ObservableCollection<RecipeIngredientEditItem> ParsedIngredients { get; } = new();

    // Commands
    public ICommand SaveRecipeCommand => new RelayCommand(async () => await SaveRecipeAsync());
    public ICommand LoadRecipeFromUrlCommand => new RelayCommand(async () => await LoadRecipeFromUrlAsync());
    public ICommand AddIngredientRowCommand => new RelayCommand(() => ParsedIngredients.Add(new RecipeIngredientEditItem()));
    public ICommand RemoveIngredientRowCommand => new RelayCommand<RecipeIngredientEditItem>(item => ParsedIngredients.Remove(item));

    private async Task LoadRecipeFromUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(ImportUrl))
        {
            ImportError = "Please enter a URL.";
            return;
        }
        try
        {
            IsLoadingRecipe = true;
            ImportError = string.Empty;
            var imported = await _recipeImportService.ImportRecipeAsync(ImportUrl);
            // Reset existing collection
            ParsedIngredients.Clear();
            foreach (var ing in imported.Ingredients)
            {
                ParsedIngredients.Add(new RecipeIngredientEditItem
                {
                    Name = ing.Name,
                    Quantity = ing.Quantity,
                    Unit = ing.Unit,
                    IsSelected = ing.IsSelected
                });
            }
            // Populate other fields
            RecipeName = imported.Name;
            Instructions = imported.Instructions;
        }
        catch (Exception ex)
        {
            ImportError = ex.Message;
        }
        finally
        {
            IsLoadingRecipe = false;
        }
    }

    public async Task SaveRecipeAsync()
    {
        if (IsSaving)
            return;

        if (string.IsNullOrWhiteSpace(RecipeName))
        {
            SaveStatus = "Error: Please enter a recipe name.";
            return;
        }
        if (string.IsNullOrWhiteSpace(Instructions))
        {
            SaveStatus = "Error: Please enter cooking instructions.";
            return;
        }

        IsSaving = true;
        SaveStatus = string.Empty;
        try
        {
            var recipe = new Recipe
            {
                Name = RecipeName,
                Instructions = Instructions,
                RequiredIngredients = new List<RecipeIngredient>()
            };

            foreach (var editItem in ParsedIngredients.Where(i => i.IsSelected))
            {
                var existing = await _db.Ingredients.FirstOrDefaultAsync(i => i.Name == editItem.Name);
                if (existing == null)
                {
                    existing = new Ingredient { Name = editItem.Name, IsStaple = false };
                    _db.Ingredients.Add(existing);
                    await _db.SaveChangesAsync();
                }

                recipe.RequiredIngredients.Add(new RecipeIngredient
                {
                    IngredientId = existing.Id,
                    QuantityRequired = editItem.Quantity,
                    Unit = editItem.Unit
                });
            }

            await _recipeService.AddRecipeAsync(recipe);
            SaveStatus = $"Saved \"{RecipeName}\"!";
            await _navigationService.GoToAsync("..", true);
        }
        catch (Exception ex)
        {
            SaveStatus = $"Error: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }
}

public class RecipeIngredientEditItem : BaseViewModel
{
    private string name = string.Empty;
    public string Name { get => name; set => SetProperty(ref name, value); }

    private decimal quantity;
    public decimal Quantity { get => quantity; set => SetProperty(ref quantity, value); }

    private string unit = "whole";
    public string Unit { get => unit; set => SetProperty(ref unit, value); }

    private bool isSelected = true;
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
}