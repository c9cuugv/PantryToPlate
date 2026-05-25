using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace PantryToPlate.Core.Services;

public class RecipeImportService : IRecipeImportService
{
    private readonly AppDbContext _db;
    private static readonly HttpClient _httpClient = new HttpClient();

    static RecipeImportService()
    {
        // Add a modern user agent to prevent websites from blocking requests
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public RecipeImportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ImportedRecipe> ImportRecipeAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be empty", nameof(url));

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
            var html = await client.GetStringAsync(url);
            return ParseRecipeHtml(html);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to fetch recipe from the URL. Ensure the link is valid and you have an internet connection. Error: {ex.Message}");
        }
    }

    public ImportedRecipe ParseRecipeHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return new ImportedRecipe();

        // Extract JSON-LD script blocks
        var regex = new Regex(@"<script[^>]*type=""application/ld\+json""[^>]*>(.*?)</script>|<script[^>]*type='application/ld\+json'[^>*]>(.*?)</script>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        var matches = regex.Matches(html);

        JsonElement? recipeElement = null;

        foreach (Match match in matches)
        {
            var jsonText = (match.Groups[1].Value + match.Groups[2].Value).Trim();
            // Decode HTML entities (e.g. &quot;)
            jsonText = System.Net.WebUtility.HtmlDecode(jsonText);

            try
            {
                using var doc = JsonDocument.Parse(jsonText);
                var found = FindRecipeObject(doc.RootElement);
                if (found != null)
                {
                    // Clone because the doc will be disposed
                    recipeElement = found.Value.Clone();
                    break;
                }
            }
            catch (JsonException)
            {
                // Ignore parsing errors of other unrelated scripts
            }
        }

        if (recipeElement == null)
            throw new Exception("Could not find structured Schema.org Recipe data on this page.");

        var recipe = new ImportedRecipe();

        // 1. Extract Name
        if (recipeElement.Value.TryGetProperty("name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
        {
            recipe.Name = nameProp.GetString() ?? string.Empty;
        }

        // 2. Extract Instructions
        recipe.Instructions = ExtractInstructions(recipeElement.Value);

        // 3. Extract Ingredients
        var dbIngredients = _db.Ingredients.ToList();
        if (recipeElement.Value.TryGetProperty("recipeIngredient", out var ingProp) && ingProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var ingVal in ingProp.EnumerateArray())
            {
                if (ingVal.ValueKind == JsonValueKind.String)
                {
                    var rawText = ingVal.GetString();
                    if (!string.IsNullOrWhiteSpace(rawText))
                    {
                        var parsed = ParseIngredientInternal(rawText, dbIngredients);
                        recipe.Ingredients.Add(parsed);
                    }
                }
            }
        }
        else if (recipeElement.Value.TryGetProperty("ingredients", out var ingPropAlt) && ingPropAlt.ValueKind == JsonValueKind.Array)
        {
            foreach (var ingVal in ingPropAlt.EnumerateArray())
            {
                if (ingVal.ValueKind == JsonValueKind.String)
                {
                    var rawText = ingVal.GetString();
                    if (!string.IsNullOrWhiteSpace(rawText))
                    {
                        var parsed = ParseIngredientInternal(rawText, dbIngredients);
                        recipe.Ingredients.Add(parsed);
                    }
                }
            }
        }

        return recipe;
    }

    public ParsedIngredient ParseIngredient(string rawText)
    {
        List<Ingredient> dbIngredients;
        try
        {
            dbIngredients = _db?.Ingredients.ToList() ?? new List<Ingredient>();
        }
        catch
        {
            dbIngredients = new List<Ingredient>();
        }
        return ParseIngredientInternal(rawText, dbIngredients);
    }

    private ParsedIngredient ParseIngredientInternal(string rawText, List<Ingredient> dbIngredients)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return new ParsedIngredient();

        var cleaned = rawText.Trim();

        // 1. Unicode Vulgar Fraction cleanups
        cleaned = cleaned.Replace("½", " 1/2 ")
                         .Replace("⅓", " 1/3 ")
                         .Replace("⅔", " 2/3 ")
                         .Replace("¼", " 1/4 ")
                         .Replace("¾", " 3/4 ")
                         .Replace("⅛", " 1/8 ")
                         .Replace("⅜", " 3/8 ")
                         .Replace("⅝", " 5/8 ")
                         .Replace("⅞", " 7/8 ");

        // Clean double-spaces
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

        decimal quantity = 1.0m;
        string remaining = cleaned;

        // 2. Extract number
        var wfRegex = new Regex(@"^(\d+)\s+(\d+)/(\d+)");
        var wfMatch = wfRegex.Match(cleaned);

        var fRegex = new Regex(@"^(\d+)/(\d+)");
        var fMatch = fRegex.Match(cleaned);

        var numRegex = new Regex(@"^(\d+(?:\.\d+)?)");
        var numMatch = numRegex.Match(cleaned);

        bool parsedNum = false;

        if (wfMatch.Success)
        {
            var whole = decimal.Parse(wfMatch.Groups[1].Value);
            var num = decimal.Parse(wfMatch.Groups[2].Value);
            var den = decimal.Parse(wfMatch.Groups[3].Value);
            quantity = whole + (num / den);
            remaining = cleaned.Substring(wfMatch.Length).Trim();
            parsedNum = true;
        }
        else if (fMatch.Success)
        {
            var num = decimal.Parse(fMatch.Groups[1].Value);
            var den = decimal.Parse(fMatch.Groups[2].Value);
            quantity = num / den;
            remaining = cleaned.Substring(fMatch.Length).Trim();
            parsedNum = true;
        }
        else if (numMatch.Success)
        {
            quantity = decimal.Parse(numMatch.Groups[1].Value);
            remaining = cleaned.Substring(numMatch.Length).Trim();
            parsedNum = true;
        }

        if (parsedNum)
        {
            // Regex to match a trailing range block: e.g. "- 2 " or " to 3 " or " or 4 "
            var rangeSuffixRegex = new Regex(@"^(?:\s*(?:-|to|or)\s*(?:\d+\s+\d+/\d+|\d+/\d+|\d+(?:\.\d+)?))", RegexOptions.IgnoreCase);
            var suffixMatch = rangeSuffixRegex.Match(remaining);
            if (suffixMatch.Success)
            {
                remaining = remaining.Substring(suffixMatch.Length).Trim();
            }
        }

        // Clean any leading punctuation or "of" left in the text
        remaining = Regex.Replace(remaining, @"^[,\-\s]+", "").Trim();

        // 3. Extract unit
        var unitRegex = new Regex(@"^(grams|gram|g|ml|milliliters|milliliter|lbs|lb|pounds|pound|ounces|ounce|oz|cups|cup|tbsp|tablespoons|tablespoon|tsp|teaspoons|teaspoon|can|cans|whole|unit|pieces|piece|cloves|clove|slices|slice|head|heads|sprig|sprigs|bunch|bunches)\b", RegexOptions.IgnoreCase);
        var unitMatch = unitRegex.Match(remaining);

        string unit = "whole";
        string ingredientNameText = remaining;

        if (unitMatch.Success)
        {
            var rawUnit = unitMatch.Groups[1].Value.ToLowerInvariant();
            unit = StandardizeUnit(rawUnit);
            ingredientNameText = remaining.Substring(unitMatch.Length).Trim();
        }

        if (ingredientNameText.StartsWith("of ", StringComparison.OrdinalIgnoreCase))
        {
            ingredientNameText = ingredientNameText.Substring(3).Trim();
        }

        var commaIdx = ingredientNameText.IndexOf(',');
        if (commaIdx >= 0)
        {
            ingredientNameText = ingredientNameText.Substring(0, commaIdx).Trim();
        }

        var cleanedName = CleanIngredientName(ingredientNameText);

        // 4. Intelligent DB Matching
        var matchedIngredient = MatchExistingIngredient(cleanedName, dbIngredients);
        string name = matchedIngredient != null ? matchedIngredient.Name : cleanedName;

        return new ParsedIngredient
        {
            Name = name,
            Quantity = Math.Round(quantity, 3),
            Unit = unit,
            IsSelected = true
        };
    }

    private string StandardizeUnit(string rawUnit)
    {
        switch (rawUnit)
        {
            case "g":
            case "gram":
            case "grams":
                return "grams";

            case "ml":
            case "milliliter":
            case "milliliters":
                return "ml";

            case "lb":
            case "lbs":
            case "pound":
            case "pounds":
                return "lb";

            case "whole":
            case "unit":
            case "pieces":
            case "piece":
            case "count":
            case "clove":
            case "cloves":
            case "slices":
            case "slice":
            case "head":
            case "heads":
            case "sprig":
            case "sprigs":
            case "bunch":
            case "bunches":
                return "whole";

            default:
                return rawUnit;
        }
    }

    private string Singularize(string name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        var lower = name.ToLowerInvariant();
        if (lower.EndsWith("ies"))
        {
            return name.Substring(0, name.Length - 3) + "y";
        }
        if (lower.EndsWith("es"))
        {
            if (lower.EndsWith("tomatoes") || lower.EndsWith("potatoes"))
            {
                return name.Substring(0, name.Length - 2);
            }
            if (lower.EndsWith("shes") || lower.EndsWith("ches") || lower.EndsWith("xes") || lower.EndsWith("ses"))
            {
                return name.Substring(0, name.Length - 2);
            }
        }
        if (lower.EndsWith("s") && !lower.EndsWith("ss") && !lower.EndsWith("us") && !lower.EndsWith("is") && !lower.EndsWith("as"))
        {
            return name.Substring(0, name.Length - 1);
        }
        return name;
    }

    private string CleanIngredientName(string rawName)
    {
        var cleaned = rawName.Trim();
        
        string[] stripPrefixes = { "fresh ", "organic ", "large ", "medium ", "small ", "shredded ", "diced ", "sliced ", "minced ", "cooked " };
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var prefix in stripPrefixes)
            {
                if (cleaned.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    cleaned = cleaned.Substring(prefix.Length).Trim();
                    changed = true;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(cleaned))
            return string.Empty;

        cleaned = Singularize(cleaned);

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleaned.ToLower());
    }

    private Ingredient? MatchExistingIngredient(string name, List<Ingredient> dbIngredients)
    {
        if (dbIngredients == null || dbIngredients.Count == 0)
            return null;

        name = name.Trim().ToLowerInvariant();

        var exact = dbIngredients.FirstOrDefault(i => i.Name.Trim().ToLowerInvariant() == name);
        if (exact != null) return exact;

        var singular = Singularize(name);
        if (singular != name)
        {
            var matchSingular = dbIngredients.FirstOrDefault(i => i.Name.Trim().ToLowerInvariant() == singular);
            if (matchSingular != null) return matchSingular;
        }

        var containsDb = dbIngredients.FirstOrDefault(i => name.Contains(i.Name.ToLowerInvariant()));
        if (containsDb != null) return containsDb;

        var containsParsed = dbIngredients.FirstOrDefault(i => i.Name.ToLowerInvariant().Contains(name));
        if (containsParsed != null) return containsParsed;

        return null;
    }

    private JsonElement? FindRecipeObject(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("@type", out var typeProp) || element.TryGetProperty("type", out typeProp))
            {
                if (typeProp.ValueKind == JsonValueKind.String)
                {
                    var typeStr = typeProp.GetString();
                    if (string.Equals(typeStr, "Recipe", StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }
                else if (typeProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in typeProp.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String &&
                            string.Equals(item.GetString(), "Recipe", StringComparison.OrdinalIgnoreCase))
                        {
                            return element;
                        }
                    }
                }
            }

            foreach (var prop in element.EnumerateObject())
            {
                var found = FindRecipeObject(prop.Value);
                if (found != null)
                {
                    return found;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var found = FindRecipeObject(item);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    private string ExtractInstructions(JsonElement recipeElement)
    {
        if (!recipeElement.TryGetProperty("recipeInstructions", out var instProp))
        {
            if (recipeElement.TryGetProperty("instructions", out instProp)) { }
            else return string.Empty;
        }

        var steps = new List<string>();

        if (instProp.ValueKind == JsonValueKind.String)
        {
            return instProp.GetString() ?? string.Empty;
        }
        else if (instProp.ValueKind == JsonValueKind.Array)
        {
            ExtractStepsFromArray(instProp, steps);
        }

        return string.Join("\n", steps);
    }

    private void ExtractStepsFromArray(JsonElement arrayElement, List<string> steps)
    {
        foreach (var item in arrayElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var text = item.GetString()?.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    steps.Add(text);
                }
            }
            else if (item.ValueKind == JsonValueKind.Object)
            {
                if (item.TryGetProperty("@type", out var typeProp) && 
                    string.Equals(typeProp.GetString(), "HowToStep", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                    {
                        var text = textProp.GetString()?.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            steps.Add(text);
                        }
                    }
                }
                else if (item.TryGetProperty("@type", out var typePropSec) && 
                         string.Equals(typePropSec.GetString(), "HowToSection", StringComparison.OrdinalIgnoreCase))
                {
                    if (item.TryGetProperty("itemListElement", out var listProp) && listProp.ValueKind == JsonValueKind.Array)
                    {
                        ExtractStepsFromArray(listProp, steps);
                    }
                }
                else if (item.TryGetProperty("text", out var textPropFallback) && textPropFallback.ValueKind == JsonValueKind.String)
                {
                    var text = textPropFallback.GetString()?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        steps.Add(text);
                    }
                }
            }
        }
    }
}
