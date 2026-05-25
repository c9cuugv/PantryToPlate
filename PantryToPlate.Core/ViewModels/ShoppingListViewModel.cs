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
}