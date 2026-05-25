namespace PantryToPlate.Core.Services;

public class ParsedIngredient
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public bool IsSelected { get; set; } = true;
}

public class ImportedRecipe
{
    public string Name { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public List<ParsedIngredient> Ingredients { get; set; } = [];
}

public interface IRecipeImportService
{
    Task<ImportedRecipe> ImportRecipeAsync(string url);
    ImportedRecipe ParseRecipeHtml(string html);
    ParsedIngredient ParseIngredient(string rawText);
}
