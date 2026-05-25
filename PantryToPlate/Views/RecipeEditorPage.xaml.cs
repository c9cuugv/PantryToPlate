using PantryToPlate.Core.ViewModels;

namespace PantryToPlate.Views;

public partial class RecipeEditorPage : ContentPage
{
    public RecipeEditorPage(RecipeEditorViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}