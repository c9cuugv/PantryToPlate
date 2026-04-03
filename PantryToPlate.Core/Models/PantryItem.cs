namespace PantryToPlate.Core.Models;

public class PantryItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityInStock { get; set; }
    public string Unit { get; set; } = string.Empty;
}
