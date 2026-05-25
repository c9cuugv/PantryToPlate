using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;
using PantryToPlate.Core.Services;
using Xunit;

namespace PantryToPlate.Tests.Services;

public class RecipeImportServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly RecipeImportService _sut;

    public RecipeImportServiceTests()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_import_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Filename={dbPath}")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _sut = new RecipeImportService(_db);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Theory]
    [InlineData("1 1/2 lbs ground beef", 1.5, "lb", "Ground Beef")]
    [InlineData("½ cup fresh organic pasta", 0.5, "cup", "Pasta")]
    [InlineData("3/4 gram ginger, minced", 0.75, "grams", "Ginger")]
    [InlineData("2 whole onions", 2, "whole", "Onion")]
    [InlineData("1-2 tomatoes", 1, "whole", "Tomato")]
    [InlineData("3 cloves of garlic, chopped", 3, "whole", "Garlic")]
    [InlineData("Salt to taste", 1, "whole", "Salt To Taste")]
    public void ParseIngredient_ParsesQuantitiesUnitsAndNamesCorrectly(string rawText, decimal expectedQty, string expectedUnit, string expectedName)
    {
        // Act
        var result = _sut.ParseIngredient(rawText);

        // Assert
        Assert.Equal(expectedQty, result.Quantity);
        Assert.Equal(expectedUnit, result.Unit);
        Assert.Equal(expectedName, result.Name);
    }

    [Fact]
    public void ParseIngredient_IntelligentlyMatchesExistingDbIngredients()
    {
        // Arrange
        var seededGarlic = new Ingredient { Name = "Garlic", IsStaple = false };
        var seededPasta = new Ingredient { Name = "Pasta", IsStaple = false };
        _db.Ingredients.AddRange(seededGarlic, seededPasta);
        _db.SaveChanges();

        var dbList = _db.Ingredients.ToList();
        var sutPrivate = new RecipeImportService(_db); // will query the DB

        // Act
        var resultGarlic = sutPrivate.ParseIngredient("3 cloves of fresh organic garlic cloves, chopped");
        var resultPasta = sutPrivate.ParseIngredient("1 1/2 lbs of cooked pasta");

        // Assert
        Assert.Equal("Garlic", resultGarlic.Name);
        Assert.Equal(3, resultGarlic.Quantity);
        Assert.Equal("whole", resultGarlic.Unit);

        Assert.Equal("Pasta", resultPasta.Name);
        Assert.Equal(1.5m, resultPasta.Quantity);
        Assert.Equal("lb", resultPasta.Unit);
    }

    [Fact]
    public void ParseRecipeHtml_ExtractsJsonLdSuccessfully()
    {
        // Arrange
        var mockHtml = @"
        <html>
        <head>
            <title>Mock Recipe</title>
            <script type=""application/ld+json"">
            {
                ""@context"": ""https://schema.org"",
                ""@type"": ""Recipe"",
                ""name"": ""Delicious Garlic Pasta"",
                ""recipeIngredient"": [
                    ""1 1/2 lbs pasta"",
                    ""3 cloves garlic""
                ],
                ""recipeInstructions"": [
                    {
                        ""@type"": ""HowToStep"",
                        ""text"": ""Boil water and cook pasta.""
                    },
                    {
                        ""@type"": ""HowToStep"",
                        ""text"": ""Sauté garlic in oil and toss with pasta.""
                    }
                ]
            }
            </script>
        </head>
        <body>
            <h1>Delicious Garlic Pasta</h1>
        </body>
        </html>";

        // Act
        var result = _sut.ParseRecipeHtml(mockHtml);

        // Assert
        Assert.Equal("Delicious Garlic Pasta", result.Name);
        Assert.Equal(2, result.Ingredients.Count);
        Assert.Equal("Pasta", result.Ingredients[0].Name);
        Assert.Equal(1.5m, result.Ingredients[0].Quantity);
        Assert.Equal("lb", result.Ingredients[0].Unit);
        Assert.Equal("Garlic", result.Ingredients[1].Name);
        Assert.Equal(3, result.Ingredients[1].Quantity);
        Assert.Equal("whole", result.Ingredients[1].Unit);
        Assert.Contains("Boil water and cook pasta.", result.Instructions);
        Assert.Contains("Sauté garlic in oil and toss with pasta.", result.Instructions);
    }
}
