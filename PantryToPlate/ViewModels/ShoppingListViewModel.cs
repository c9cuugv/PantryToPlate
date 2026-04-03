using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using System.Collections.ObjectModel;

namespace PantryToPlate.ViewModels;

public partial class ShoppingListViewModel : ObservableObject
{
    private readonly AppDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<ShoppingListItem> shoppingListItems = new();

    [ObservableProperty]
    private bool isLoading;

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
            var items = await Task.Run(() => _dbContext.ShoppingListItems
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

    [RelayCommand]
    private async Task TogglePurchasedAsync(ShoppingListItem item)
    {
        try
        {
            item.IsPurchased = !item.IsPurchased;
            _dbContext.ShoppingListItems.Update(item);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error toggling purchased status: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ClearPurchasedAsync()
    {
        try
        {
            var purchasedItems = ShoppingListItems.Where(i => i.IsPurchased).ToList();
            _dbContext.ShoppingListItems.RemoveRange(purchasedItems);
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
}