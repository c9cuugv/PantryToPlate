using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using PantryToPlate.Core.Services;
using PantryToPlate.Core.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace PantryToPlate.Tests
{
    public class LiveUrlTest
    {
        private readonly ITestOutputHelper _output;

        public LiveUrlTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestCholeRecipeLink()
        {
            var dbPath = Path.Combine(Path.GetTempPath(), $"test_live_{Guid.NewGuid()}.db");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Filename={dbPath}")
                .Options;
            using var db = new AppDbContext(options);
            db.Database.EnsureCreated();

            var service = new RecipeImportService(db);
            var result = await service.ImportRecipeAsync("https://www.indianhealthyrecipes.com/chole/");
            
            _output.WriteLine("=== Recipe Title ===");
            _output.WriteLine(result.Name);
            _output.WriteLine("\n=== Instructions ===");
            _output.WriteLine(result.Instructions);
            _output.WriteLine("\n=== Ingredients ===");
            foreach (var ing in result.Ingredients)
            {
                _output.WriteLine($"- {ing.Quantity} {ing.Unit} of {ing.Name}");
            }
        }
    }
}
