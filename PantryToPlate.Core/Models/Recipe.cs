namespace PantryToPlate.Core.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<RecipeIngredient> RequiredIngredients { get; set; } = [];
}
