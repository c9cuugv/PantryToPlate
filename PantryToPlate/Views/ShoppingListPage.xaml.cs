using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

public partial class ShoppingListPage : ContentPage
{
    private ShoppingListViewModel ViewModel => (ShoppingListViewModel)BindingContext;

    public ShoppingListPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadShoppingListItemsAsync();
    }

    private void OnPurchasedToggled(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Core.Models.ShoppingListItem item)
        {
            ViewModel.TogglePurchasedCommand.Execute(item);
        }
    }
}