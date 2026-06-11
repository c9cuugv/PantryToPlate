using System.Threading.Tasks;

namespace PantryToPlate.Core.Services;

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);
}
