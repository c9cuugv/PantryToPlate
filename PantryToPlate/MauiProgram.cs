using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryToPlate.Core.Data;
using PantryToPlate.Core.Services;
using PantryToPlate.ViewModels;
using PantryToPlate.Converters;

namespace PantryToPlate;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register DbContext
		string dbPath = Path.Combine(FileSystem.AppDataDirectory, "pantry.db");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Register Services
		builder.Services.AddSingleton<IRecipeService, RecipeService>();

		// Register ViewModels
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<RecipeDetailViewModel>();
		builder.Services.AddTransient<RecipeEditorViewModel>();
		builder.Services.AddTransient<PantryViewModel>();
		builder.Services.AddTransient<ShoppingListViewModel>();

		// Register Converters
		builder.Services.AddSingleton<BoolToCheckIconConverter>();
		builder.Services.AddSingleton<InverseBoolConverter>();
		builder.Services.AddSingleton<IngredientsToSummaryConverter>();
		builder.Services.AddSingleton<BoolToStrikethroughConverter>();

		var app = builder.Build();

		// Initialize and seed database
		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			db.Database.EnsureCreated();
			DatabaseSeeder.SeedAsync(db).Wait();
		}

		return app;
	}
}
