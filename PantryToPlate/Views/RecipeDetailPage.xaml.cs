using PantryToPlate.Core.ViewModels;
namespace PantryToPlate.Views;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailPage : ContentPage
{
    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public string RecipeId
    {
        set
        {
            if (int.TryParse(value, out var id) && BindingContext is RecipeDetailViewModel vm)
            {
                _ = vm.LoadRecipeAsync(id);
            }
        }
    }
}