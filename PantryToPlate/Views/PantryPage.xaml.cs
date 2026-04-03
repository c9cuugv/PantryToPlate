using PantryToPlate.ViewModels;

namespace PantryToPlate.Views;

public partial class PantryPage : ContentPage
{
    private PantryViewModel ViewModel => (PantryViewModel)BindingContext;

    public PantryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadPantryItemsAsync();
    }
}