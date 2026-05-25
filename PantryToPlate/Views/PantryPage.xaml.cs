using PantryToPlate.Core.ViewModels;

namespace PantryToPlate.Views;

public partial class PantryPage : ContentPage
{
    private PantryViewModel ViewModel => (PantryViewModel)BindingContext;

    public PantryPage(PantryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadPantryItemsAsync();
    }
}