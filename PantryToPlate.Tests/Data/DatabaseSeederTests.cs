using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PantryToPlate.Tests.Data;

public class DatabaseSeederTests : IDisposable
{
    private readonly AppDbContext _db;

    public DatabaseSeederTests()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_seeder_{Guid.NewGuid()}.db");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Filename={dbPath}")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public async Task SeedAsync_CreatesIngredients()
    {
        await DatabaseSeeder.SeedAsync(_db);
        Assert.True(await _db.Ingredients.CountAsync() > 0);
    }

    [Fact]
    public async Task SeedAsync_CreatesRecipes()
    {
        await DatabaseSeeder.SeedAsync(_db);
        Assert.True(await _db.Recipes.CountAsync() > 0);
    }

    [Fact]
    public async Task SeedAsync_CreatesPantryItems()
    {
        await DatabaseSeeder.SeedAsync(_db);
        var count = await _db.Pantry.CountAsync();
        Assert.True(count > 0, $"Expected seeded pantry items, got {count}");
    }

    [Fact]
    public async Task SeedAsync_PantryItems_HaveValidIngredientRefs()
    {
        await DatabaseSeeder.SeedAsync(_db);
        var items = await _db.Pantry.Include(p => p.Ingredient).ToListAsync();
        Assert.All(items, item =>
        {
            Assert.True(item.IngredientId > 0);
            Assert.NotNull(item.Ingredient);
            Assert.False(string.IsNullOrEmpty(item.Ingredient.Name));
        });
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_SecondCallSkips()
    {
        await DatabaseSeeder.SeedAsync(_db);
        var countAfterFirst = await _db.Ingredients.CountAsync();

        await DatabaseSeeder.SeedAsync(_db);
        var countAfterSecond = await _db.Ingredients.CountAsync();

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public async Task SeedAsync_PantryItems_HavePositiveStock()
    {
        await DatabaseSeeder.SeedAsync(_db);
        var items = await _db.Pantry.ToListAsync();
        Assert.All(items, item => Assert.True(item.QuantityInStock > 0));
    }

    [Fact]
    public void EnsureShoppingListSchema_AddsQuantityColumnsToExistingDatabase()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_schema_{Guid.NewGuid()}.db");

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE "ShoppingList" (
                    "Id" INTEGER NOT NULL CONSTRAINT "PK_ShoppingList" PRIMARY KEY AUTOINCREMENT,
                    "IngredientId" INTEGER NOT NULL,
                    "IsPurchased" INTEGER NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Filename={dbPath}")
            .Options;

        using var db = new AppDbContext(options);
        db.EnsureShoppingListSchema();

        using var verifyConnection = new SqliteConnection($"Data Source={dbPath}");
        verifyConnection.Open();
        using var verifyCommand = verifyConnection.CreateCommand();
        verifyCommand.CommandText = "PRAGMA table_info(\"ShoppingList\");";
        using var reader = verifyCommand.ExecuteReader();

        var columns = new List<string>();
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        Assert.Contains("QuantityToBuy", columns);
        Assert.Contains("Unit", columns);
    }
}
