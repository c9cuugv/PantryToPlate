namespace PantryToPlate.Core.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsStaple { get; set; }
}
