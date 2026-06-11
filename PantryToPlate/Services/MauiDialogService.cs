using PantryToPlate.Core.Services;

namespace PantryToPlate.Services;

public class MauiDialogService : IDialogService
{
    public Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
    {
        if (Shell.Current?.CurrentPage != null)
        {
            return Shell.Current.CurrentPage.DisplayAlertAsync(title, message, accept, cancel);
        }
        return Task.FromResult(false);
    }
}
