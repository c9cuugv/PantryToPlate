using System.Globalization;

namespace PantryToPlate.Core.Models;

public class ShoppingListItem
{
    public int Id { get; set; }
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
    public decimal QuantityToBuy { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string PurchaseAmountText => QuantityToBuy > 0
        ? $"{QuantityToBuy.ToString("G29", CultureInfo.InvariantCulture)} {Unit}".Trim()
        : Unit.Trim();
    public bool IsPurchased { get; set; }
}
