using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PantryToPlate.Core.Data;

namespace PantryToPlate.Core.Services;

public sealed class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly object gate = new();
    private Task? initialized;

    public DatabaseInitializer(IServiceScopeFactory scopeFactory)
    {
        this.scopeFactory = scopeFactory;
    }

    public Task Initialized => initialized ?? Task.CompletedTask;

    public void Start()
    {
        lock (gate)
        {
            initialized ??= Task.Run(InitializeAsync);
        }
    }

    private async Task InitializeAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureCreatedAsync();
        db.EnsureShoppingListSchema();
        await DatabaseSeeder.SeedAsync(db);
    }
}
