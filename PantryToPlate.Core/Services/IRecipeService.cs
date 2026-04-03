using PantryToPlate.Core.Models;

namespace PantryToPlate.Core.Services;

public interface IRecipeService
{
    Task<List<Recipe>> GetAvailableRecipesAsync();
    Task<Recipe?> GetRecipeByIdAsync(int id);
    Task CookRecipeAsync(int recipeId);
    Task AddRecipeAsync(Recipe recipe);
    Task UpdateRecipeAsync(Recipe recipe);
    Task DeleteRecipeAsync(int recipeId);
}
