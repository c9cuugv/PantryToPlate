using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryToPlate.Core.Models;

namespace PantryToPlate.Core.Data;

public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<PantryItem> Pantry { get; set; }
    public DbSet<ShoppingListItem> ShoppingList { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        System.Diagnostics.Debug.WriteLine("=== AppDbContext CLASS LOADED (v2-fix) ===");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default path if not configured by DI (e.g. CLI tools)
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pantry.db");
            optionsBuilder.UseSqlite($"Filename={dbPath}");
        }
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
