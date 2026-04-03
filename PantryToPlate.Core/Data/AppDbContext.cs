using Microsoft.EntityFrameworkCore;
using PantryToPlate.Core.Models;

namespace PantryToPlate.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<PantryItem> Pantry { get; set; }
    public DbSet<ShoppingListItem> ShoppingList { get; set; }

    private readonly string _dbPath;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RequiredIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId);

        modelBuilder.Entity<PantryItem>()
            .HasOne(p => p.Ingredient)
            .WithMany()
            .HasForeignKey(p => p.IngredientId);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(s => s.Ingredient)
            .WithMany()
            .HasForeignKey(s => s.IngredientId);
    }
}
