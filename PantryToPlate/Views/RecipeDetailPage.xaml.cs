namespace PantryToPlate;

[QueryProperty(nameof(RecipeId), "recipeId")]
public partial class RecipeDetailPage : ContentPage
{
    public RecipeDetailPage()
    {
        InitializeComponent();
    }

    public string RecipeId
    {
        set
        {
            if (int.TryParse(value, out var id) && BindingContext is ViewModels.RecipeDetailViewModel vm)
            {
                _ = vm.LoadRecipeAsync(id);
            }
        }
    }
}