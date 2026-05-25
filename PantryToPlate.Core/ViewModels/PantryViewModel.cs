using PantryToPlate.Core.Services;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public partial class PantryViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;

    private ObservableCollection<PantryItem> pantryItems = new();
    public ObservableCollection<PantryItem> PantryItems { get => pantryItems; set => SetProperty(ref pantryItems, value); }

    private string newIngredientName = string.Empty;
    public string NewIngredientName
    {
        get => newIngredientName;
        set
        {
            if (SetProperty(ref newIngredientName, value))
            {
                UpdateDefaultUnit();
            }
        }
    }

    private decimal newQuantity;
    public decimal NewQuantity { get => newQuantity; set => SetProperty(ref newQuantity, value); }

    private string newUnit = string.Empty;
    public string NewUnit { get => newUnit; set => SetProperty(ref newUnit, value); }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    public PantryViewModel()
    {
        _dbContext = null!;
    }

    public PantryViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LoadPantryItemsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var items = await Task.Run(() => _dbContext.Pantry
                .Include(p => p.Ingredient)
                .ToList());

            PantryItems.Clear();
            foreach (var item in items)
            {
                PantryItems.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand AddPantryItemCommand => new RelayCommand(async () => await AddPantryItemAsync());
    public async Task AddPantryItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName) ||
            NewQuantity <= 0 ||
            string.IsNullOrWhiteSpace(NewUnit))
            return;

        try
        {
            // Find or create ingredient
            var ingredient = await Task.Run(() => _dbContext.Ingredients
                .FirstOrDefault(i => i.Name.ToLower() == NewIngredientName.ToLower()));

            if (ingredient == null)
            {
                ingredient = new Ingredient { Name = NewIngredientName };
                _dbContext.Ingredients.Add(ingredient);
                await _dbContext.SaveChangesAsync();
            }

            var pantryItem = new PantryItem
            {
                IngredientId = ingredient.Id,
                Ingredient = ingredient,
                QuantityInStock = NewQuantity,
                Unit = NewUnit
            };

            _dbContext.Pantry.Add(pantryItem);
            await _dbContext.SaveChangesAsync();

            PantryItems.Add(pantryItem);

            // Clear form
            NewIngredientName = string.Empty;
            NewQuantity = 0;
            NewUnit = string.Empty;
        }
        catch (Exception ex)
        {
            // Handle error - in a real app, show user-friendly message
            Console.WriteLine($"Error adding pantry item: {ex.Message}");
        }
    }

    public ICommand DeletePantryItemCommand => new RelayCommand<PantryItem>(async (item) => await DeletePantryItemAsync(item));
    public async Task DeletePantryItemAsync(PantryItem item)
    {
        try
        {
            _dbContext.Pantry.Remove(item);
            await _dbContext.SaveChangesAsync();
            PantryItems.Remove(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting pantry item: {ex.Message}");
        }
    }

    private void UpdateDefaultUnit()
    {
        if (string.IsNullOrWhiteSpace(NewIngredientName)) return;

        var nameLower = NewIngredientName.ToLowerInvariant().Trim();
        if (nameLower.Contains("pasta"))
        {
            NewUnit = "lb";
        }
        else if (nameLower.Contains("tomato"))
        {
            NewUnit = "unit";
        }
        else if (nameLower.Contains("onion"))
        {
            NewUnit = "unit";
        }
        else if (nameLower.Contains("garlic") || nameLower.Contains("egg") || nameLower.Contains("potato") || 
                 nameLower.Contains("carrot") || nameLower.Contains("cabbage") || nameLower.Contains("bread") || 
                 nameLower.Contains("lemon") || nameLower.Contains("pepper") || nameLower.Contains("zucchini") ||
                 nameLower.Contains("tuna") || nameLower.Contains("beans") || nameLower.Contains("cucumber"))
        {
            NewUnit = "unit";
        }
        else if (nameLower.Contains("beef") || nameLower.Contains("chicken") || nameLower.Contains("cheese") || 
                 nameLower.Contains("flour") || nameLower.Contains("spinach") || nameLower.Contains("mushroom") ||
                 nameLower.Contains("broccoli") || nameLower.Contains("salmon") || nameLower.Contains("shrimp") ||
                 nameLower.Contains("ginger") || nameLower.Contains("tofu") || nameLower.Contains("rice"))
        {
            NewUnit = "grams";
        }
        else if (nameLower.Contains("milk") || nameLower.Contains("oil") || nameLower.Contains("sauce"))
        {
            NewUnit = "ml";
        }
    }
}