using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using System.Collections.ObjectModel;

namespace PantryToPlate.ViewModels;

public partial class PantryViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<PantryItem> pantryItems = new();

    [ObservableProperty]
    private string newIngredientName = string.Empty;

    [ObservableProperty]
    private decimal newQuantity;

    [ObservableProperty]
    private string newUnit = string.Empty;

    [ObservableProperty]
    private bool isLoading;

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
            var items = await Task.Run(() => _dbContext.PantryItems
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

    [RelayCommand]
    private async Task AddPantryItemAsync()
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

            _dbContext.PantryItems.Add(pantryItem);
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

    [RelayCommand]
    private async Task DeletePantryItemAsync(PantryItem item)
    {
        try
        {
            _dbContext.PantryItems.Remove(item);
            await _dbContext.SaveChangesAsync();
            PantryItems.Remove(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting pantry item: {ex.Message}");
        }
    }
}