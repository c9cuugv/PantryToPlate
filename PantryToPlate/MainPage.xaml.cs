using PantryToPlate.ViewModels;

namespace PantryToPlate;

public partial class MainPage : ContentPage
{
    private HomeViewModel ViewModel => (HomeViewModel)BindingContext;

    public MainPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadRecipesAsync();
    }

    private void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Core.Models.Recipe selectedRecipe)
        {
            ViewModel.NavigateToRecipeCommand.Execute(selectedRecipe);
        }
    }
}
