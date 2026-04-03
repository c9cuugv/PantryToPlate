namespace PantryToPlate.Core.Models;

public class ShoppingListItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public bool IsPurchased { get; set; }
}
