using PantryToPlate.Core.Services;

namespace PantryToPlate.Services;

public class MauiNavigationService : INavigationService
{
    public Task GoToAsync(string route, bool animate = false)
    {
        return Shell.Current.GoToAsync(route, animate);
    }
}
