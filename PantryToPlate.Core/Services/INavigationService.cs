using System.Threading.Tasks;

namespace PantryToPlate.Core.Services;

public interface INavigationService
{
    Task GoToAsync(string route, bool animate = false);
}
