using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PantryToPlate.Core.ViewModels;

public class RelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(() => { execute(); return Task.CompletedTask; }, canExecute) { }

    public RelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public async void Execute(object? parameter)
    {
        try { await _execute(); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[RelayCommand] Execute failed: {ex}"); }
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Func<T, Task> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        : this(t => { execute(t); return Task.CompletedTask; }, canExecute) { }

    public RelayCommand(Func<T, Task> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public bool CanExecute(object? parameter)
    {
        if (_canExecute is null) return true;
        return parameter is T typed && _canExecute(typed);
    }

    public async void Execute(object? parameter)
    {
        if (parameter is T typed)
        {
            try { await _execute(typed); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[RelayCommand<T>] Execute failed: {ex}"); }
        }
    }
}
