using PantryToPlate.Views;

namespace PantryToPlate;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("RecipeDetailPage", typeof(RecipeDetailPage));
		Routing.RegisterRoute("RecipeEditorPage", typeof(RecipeEditorPage));
	}
}
