using PantryToPlate.Core.Services;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public partial class ShoppingListViewModel : BaseViewModel
{
    private readonly AppDbContext _dbContext;

    private ObservableCollection<ShoppingListItem> shoppingListItems = new();
    public ObservableCollection<ShoppingListItem> ShoppingListItems { get => shoppingListItems; set => SetProperty(ref shoppingListItems, value); }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    public ShoppingListViewModel()
    {
        _dbContext = null!;
    }

    public ShoppingListViewModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LoadShoppingListItemsAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var items = await Task.Run(() => _dbContext.ShoppingList
                .Include(s => s.Ingredient)
                .ToList());

            ShoppingListItems.Clear();
            foreach (var item in items)
            {
                ShoppingListItems.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public ICommand TogglePurchasedCommand => new RelayCommand<ShoppingListItem>(async (item) => await TogglePurchasedAsync(item));
    public async Task TogglePurchasedAsync(ShoppingListItem item)
    {
        try
        {
            item.IsPurchased = !item.IsPurchased;
            _dbContext.ShoppingList.Update(item);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling purchased status: {ex.Message}");
        }
    }

    public ICommand ClearPurchasedCommand => new RelayCommand(async () => await ClearPurchasedAsync());
    public async Task ClearPurchasedAsync()
    {
        try
        {
            var purchasedItems = ShoppingListItems.Where(i => i.IsPurchased).ToList();
            _dbContext.ShoppingList.RemoveRange(purchasedItems);
            await _dbContext.SaveChangesAsync();

            foreach (var item in purchasedItems)
            {
                ShoppingListItems.Remove(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing purchased items: {ex.Message}");
        }
    }

    public ICommand RestockToPantryCommand => new RelayCommand(async () => await RestockToPantryAsync());
    public async Task RestockToPantryAsync()
    {
        try
        {
            var purchasedItems = ShoppingListItems.Where(i => i.IsPurchased).ToList();
            if (!purchasedItems.Any()) return;

            foreach (var item in purchasedItems)
            {
                string unit;
                if (!string.IsNullOrWhiteSpace(item.Unit))
                {
                    unit = item.Unit.ToLowerInvariant().Trim();
                }
                else
                {
                    unit = "unit";

                    var recipeIngredient = await _dbContext.RecipeIngredients
                        .FirstOrDefaultAsync(ri => ri.IngredientId == item.IngredientId);

                    if (recipeIngredient != null && !string.IsNullOrWhiteSpace(recipeIngredient.Unit))
                    {
                        unit = recipeIngredient.Unit.ToLowerInvariant().Trim();
                    }
                    else
                    {
                        string nameLower = item.Ingredient.Name.ToLowerInvariant().Trim();
                        if (nameLower.Contains("pasta")) unit = "lb";
                        else if (nameLower.Contains("tomato") || nameLower.Contains("onion") || nameLower.Contains("garlic") ||
                                 nameLower.Contains("egg") || nameLower.Contains("potato") || nameLower.Contains("carrot") ||
                                 nameLower.Contains("cabbage") || nameLower.Contains("bread") || nameLower.Contains("lemon") ||
                                 nameLower.Contains("pepper") || nameLower.Contains("zucchini") || nameLower.Contains("tuna") ||
                                 nameLower.Contains("beans") || nameLower.Contains("cucumber"))
                        {
                            unit = "unit";
                        }
                        else if (nameLower.Contains("beef") || nameLower.Contains("chicken") || nameLower.Contains("cheese") ||
                                 nameLower.Contains("flour") || nameLower.Contains("spinach") || nameLower.Contains("mushroom") ||
                                 nameLower.Contains("broccoli") || nameLower.Contains("salmon") || nameLower.Contains("shrimp") ||
                                 nameLower.Contains("ginger") || nameLower.Contains("tofu") || nameLower.Contains("rice"))
                        {
                            unit = "grams";
                        }
                        else if (nameLower.Contains("milk") || nameLower.Contains("oil") || nameLower.Contains("sauce"))
                        {
                            unit = "ml";
                        }
                    }
                }

                string unitLower = unit.ToLowerInvariant().Trim();
                decimal quantity = item.QuantityToBuy > 0 ? item.QuantityToBuy : 1.0m;
                if (item.QuantityToBuy <= 0)
                {
                    if (unitLower == "grams" || unitLower == "g") quantity = 500m;
                    else if (unitLower == "ml") quantity = 1000m;
                    else if (unitLower == "whole" || unitLower == "unit" || unitLower == "count" || unitLower == "pieces") quantity = 6m;
                    else if (unitLower == "lb" || unitLower == "lbs") quantity = 1m;
                }

                var existingPantry = await _dbContext.Pantry
                    .FirstOrDefaultAsync(p => p.IngredientId == item.IngredientId);

                if (existingPantry != null)
                {
                    if (existingPantry.Unit.ToLowerInvariant().Trim() == unitLower)
                    {
                        existingPantry.QuantityInStock += quantity;
                    }
                    else
                    {
                        decimal factor = RecipeService.GetConversionFactor(unit, existingPantry.Unit);
                        existingPantry.QuantityInStock += quantity * factor;
                    }
                }
                else
                {
                    _dbContext.Pantry.Add(new PantryItem
                    {
                        IngredientId = item.IngredientId,
                        QuantityInStock = quantity,
                        Unit = unit
                    });
                }
            }

            _dbContext.ShoppingList.RemoveRange(purchasedItems);
            await _dbContext.SaveChangesAsync();

            foreach (var item in purchasedItems)
            {
                ShoppingListItems.Remove(item);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restocking to pantry: {ex.Message}");
        }
    }
}
