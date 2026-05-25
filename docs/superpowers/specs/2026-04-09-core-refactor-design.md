# Core Refactor Design — BaseViewModel + RelayCommand

**Date:** 2026-04-09  
**Scope:** `PantryToPlate.Core` only — ViewModels + RelayCommand  
**Goal:** Reduce repeated boilerplate, improve readability, decrease line count

---

## Problem

5 ViewModels each duplicate:
- `isLoading` / `IsLoading` property (~4 lines each)
- Busy-guard pattern: `if (IsLoading) return; IsLoading = true; try/finally` (~4 lines each)

`RelayCommand` and `RelayCommand<T>` each hold two nullable delegate fields where only one is ever set, with null-branch logic in `Execute`.

---

## Change 1 — BaseViewModel

Add `IsLoading` property and `ExecuteBusyAsync` helper to `BaseViewModel.cs`:

```csharp
private bool isLoading;
public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

protected async Task ExecuteBusyAsync(Func<Task> action)
{
    if (IsLoading) return;
    IsLoading = true;
    try   { await action(); }
    finally { IsLoading = false; }
}
```

**Remove from:** `HomeViewModel`, `PantryViewModel`, `RecipeDetailViewModel`, `ShoppingListViewModel`  
- Delete `isLoading` field + `IsLoading` property from each (~16 lines total)  
- Replace each load method's guard + try/finally with `await ExecuteBusyAsync(async () => { ... })`  

**Keep:** `RecipeEditorViewModel.IsSaving` — semantically distinct from loading state.

---

## Change 2 — RelayCommand

Merge `RelayCommand.cs` + `RelayCommandT.cs` into single `RelayCommand.cs`.  
Replace two nullable delegate fields with single `Func<Task> _execute`. Sync `Action` constructor wraps into `Task.CompletedTask`:

```csharp
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
    public async void Execute(object? parameter) => await _execute();
}
```

`RelayCommand<T>` follows the same pattern (in the same file). Note: merged file must include `using System.Threading.Tasks;`:

```csharp
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
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;
    public async void Execute(object? parameter) => await _execute((T)parameter!);
}
```

**Delete:** `RelayCommandT.cs`  
**Drops:** ~25 lines of null-branching logic across both files.

---

## Files Changed

| File | Action |
|------|--------|
| `PantryToPlate.Core/ViewModels/BaseViewModel.cs` | Add `IsLoading` + `ExecuteBusyAsync` |
| `PantryToPlate.Core/ViewModels/HomeViewModel.cs` | Remove `IsLoading`, use `ExecuteBusyAsync` |
| `PantryToPlate.Core/ViewModels/PantryViewModel.cs` | Remove `IsLoading`, use `ExecuteBusyAsync` |
| `PantryToPlate.Core/ViewModels/RecipeDetailViewModel.cs` | Remove `IsLoading`, use `ExecuteBusyAsync` |
| `PantryToPlate.Core/ViewModels/ShoppingListViewModel.cs` | Remove `IsLoading`, use `ExecuteBusyAsync` |
| `PantryToPlate.Core/ViewModels/RelayCommand.cs` | Simplify + absorb `RelayCommandT.cs` |
| `PantryToPlate.Core/ViewModels/RelayCommandT.cs` | **Delete** |

---

## Out of Scope

- View code-behind (`.xaml.cs`) — excluded per user decision
- `RecipeEditorViewModel.IsSaving` — kept, semantically distinct
- `null!` parameterless constructors — XAML requires them, no net gain removing
