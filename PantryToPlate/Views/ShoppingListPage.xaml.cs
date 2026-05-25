using PantryToPlate.Core.ViewModels;

namespace PantryToPlate.Views;

public partial class ShoppingListPage : ContentPage
{
    private ShoppingListViewModel ViewModel => (ShoppingListViewModel)BindingContext;

    public ShoppingListPage(ShoppingListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
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