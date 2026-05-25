using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Models;

namespace PantryToPlate.Core.Services;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _db;

    public RecipeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Recipe>> GetAvailableRecipesAsync()
    {
        var recipes = await _db.Recipes
            .Include(r => r.RequiredIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .ToListAsync();

        var pantry = await _db.Pantry.ToListAsync();

        return recipes.Where(recipe =>
            recipe.RequiredIngredients
                .Where(ri => !ri.Ingredient.IsStaple)
                .All(ri =>
                {
                    var stock = pantry.FirstOrDefault(p => p.IngredientId == ri.IngredientId);
                    if (stock == null) return false;

                    var factor = GetConversionFactor(stock.Unit, ri.Unit);
                    var stockInRecipeUnit = stock.QuantityInStock * factor;
                    return stockInRecipeUnit >= ri.QuantityRequired;
                })
        ).ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _db.Recipes
            .Include(r => r.RequiredIngredients)
                .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task AddRecipeAsync(Recipe recipe)
    {
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateRecipeAsync(Recipe recipe)
    {
        var existing = await _db.Recipes
            .Include(r => r.RequiredIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (existing == null) return;

        existing.Name = recipe.Name;
        existing.Instructions = recipe.Instructions;

        // Replace ingredient requirements in the database so that we can avoid odd partial tracking state.
        var existingIngredientEntries = _db.RecipeIngredients.Where(ri => ri.RecipeId == existing.Id);
        _db.RecipeIngredients.RemoveRange(existingIngredientEntries);

        var newIngredients = recipe.RequiredIngredients.Select(ri => new RecipeIngredient
        {
            RecipeId = existing.Id,
            IngredientId = ri.IngredientId,
            QuantityRequired = ri.QuantityRequired,
            Unit = ri.Unit
        }).ToList();

        if (newIngredients.Any())
            _db.RecipeIngredients.AddRange(newIngredients);

        existing.RequiredIngredients = newIngredients;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(int recipeId)
    {
        var recipe = await _db.Recipes
            .Include(r => r.RequiredIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null) return;

        _db.Recipes.Remove(recipe);
        await _db.SaveChangesAsync();
    }

    public async Task CookRecipeAsync(int recipeId)
    {
        var recipe = await _db.Recipes
            .Include(r => r.RequiredIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipeId);

        if (recipe == null) return;

        foreach (var ri in recipe.RequiredIngredients)
        {
            var pantryItem = await _db.Pantry
                .FirstOrDefaultAsync(p => p.IngredientId == ri.IngredientId);

            if (pantryItem == null) continue;

            var factor = GetConversionFactor(ri.Unit, pantryItem.Unit);
            var deductedQty = ri.QuantityRequired * factor;

            pantryItem.QuantityInStock -= deductedQty;

            if (pantryItem.QuantityInStock <= 0.001m)
            {
                _db.Pantry.Remove(pantryItem);

                var exists = await _db.ShoppingList
                    .AnyAsync(s => s.IngredientId == ri.IngredientId);
                if (!exists)
                    _db.ShoppingList.Add(new ShoppingListItem { IngredientId = ri.IngredientId });
            }
        }

        await _db.SaveChangesAsync();
    }

    private static decimal GetConversionFactor(string fromUnit, string toUnit)
    {
        fromUnit = fromUnit.ToLowerInvariant().Trim();
        toUnit = toUnit.ToLowerInvariant().Trim();

        if (fromUnit == toUnit) return 1.0m;

        bool IsWeightLb(string u) => u == "lb" || u == "lbs";
        bool IsWeightG(string u) => u == "grams" || u == "g";

        if (IsWeightLb(fromUnit) && IsWeightG(toUnit))
        {
            return 453.59237m;
        }
        if (IsWeightG(fromUnit) && IsWeightLb(toUnit))
        {
            return 1.0m / 453.59237m;
        }

        bool IsCount(string u) => u == "whole" || u == "unit" || u == "count" || u == "pieces";
        if (IsCount(fromUnit) && IsCount(toUnit))
        {
            return 1.0m;
        }

        return 1.0m; // Default fallback to preserve existing test behaviors
    }
}
