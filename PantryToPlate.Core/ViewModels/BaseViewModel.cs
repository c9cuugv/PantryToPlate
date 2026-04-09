using PantryToPlate.Core.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private bool isLoading;
    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

    protected async Task ExecuteBusyAsync(Func<Task> action)
    {
        if (IsLoading) return;
        IsLoading = true;
        try   { await action(); }
        finally { IsLoading = false; }
    }
}
