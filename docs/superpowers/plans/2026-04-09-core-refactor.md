# Core Refactor Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate repeated `IsLoading` boilerplate and dual-delegate `RelayCommand` pattern in `PantryToPlate.Core`.

**Architecture:** Move `IsLoading` property and `ExecuteBusyAsync` guard helper into `BaseViewModel` so all 5 ViewModels inherit it. Merge `RelayCommand` and `RelayCommand<T>` into one file using a single `Func<Task>` delegate field, removing all null-branch logic.

**Tech Stack:** .NET 10, C#, xUnit, EF Core 8.0.10

---

## Chunk 1: BaseViewModel — add IsLoading + ExecuteBusyAsync

**Files:**
- Modify: `PantryToPlate.Core/ViewModels/BaseViewModel.cs`

- [ ] **Step 1: Confirm baseline tests pass**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet test PantryToPlate.Tests/ --no-build 2>&1 | tail -5
```

Expected: `Passed! - Failed: 0`

- [ ] **Step 2: Add `IsLoading` + `ExecuteBusyAsync` to `BaseViewModel.cs`**

Open `PantryToPlate.Core/ViewModels/BaseViewModel.cs`. Add after the `SetProperty` method:

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

Also add `using System.Threading.Tasks;` to the using block if not already present.

Final file should look like:

```csharp
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
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
```

- [ ] **Step 3: Build Core to verify no compile errors**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet build PantryToPlate.Core/ 2>&1 | tail -5
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Commit**

```bash
git add PantryToPlate.Core/ViewModels/BaseViewModel.cs
git commit -m "refactor: add IsLoading + ExecuteBusyAsync to BaseViewModel"
```

---

## Chunk 2: Merge RelayCommand files

**Files:**
- Modify: `PantryToPlate.Core/ViewModels/RelayCommand.cs`
- Delete: `PantryToPlate.Core/ViewModels/RelayCommandT.cs`

- [ ] **Step 1: Replace `RelayCommand.cs` with merged content**

Overwrite `PantryToPlate.Core/ViewModels/RelayCommand.cs` with:

```csharp
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
    public async void Execute(object? parameter) => await _execute();
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
    public bool CanExecute(object? parameter) => _canExecute?.Invoke((T)parameter!) ?? true;
    public async void Execute(object? parameter) => await _execute((T)parameter!);
}
```

- [ ] **Step 2: Delete `RelayCommandT.cs`**

```bash
rm "PantryToPlate.Core/ViewModels/RelayCommandT.cs"
```

- [ ] **Step 3: Build Core**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet build PantryToPlate.Core/ 2>&1 | tail -5
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 4: Run tests**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet test PantryToPlate.Tests/ 2>&1 | tail -5
```

Expected: `Passed! - Failed: 0`

- [ ] **Step 5: Commit**

```bash
git add PantryToPlate.Core/ViewModels/RelayCommand.cs
git rm PantryToPlate.Core/ViewModels/RelayCommandT.cs
git commit -m "refactor: merge RelayCommand<T> into RelayCommand.cs, single Func<Task> delegate"
```

---

## Chunk 3: Remove IsLoading from ViewModels — use ExecuteBusyAsync

**Files:**
- Modify: `PantryToPlate.Core/ViewModels/HomeViewModel.cs`
- Modify: `PantryToPlate.Core/ViewModels/PantryViewModel.cs`
- Modify: `PantryToPlate.Core/ViewModels/RecipeDetailViewModel.cs`
- Modify: `PantryToPlate.Core/ViewModels/ShoppingListViewModel.cs`

### HomeViewModel

- [ ] **Step 1: Remove `isLoading` field + `IsLoading` property**

In `HomeViewModel.cs`, delete these two lines:
```csharp
private bool isLoading;
public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
```

- [ ] **Step 2: Replace busy-guard pattern in `LoadRecipesAsync`**

Find the existing pattern (approximately):
```csharp
public async Task LoadRecipesAsync()
{
    if (IsLoading) return;
    IsLoading = true;
    try
    {
        // ... load logic ...
    }
    finally
    {
        IsLoading = false;
    }
}
```

Replace with:
```csharp
public async Task LoadRecipesAsync()
    => await ExecuteBusyAsync(async () =>
    {
        // ... same load logic (no guard, no IsLoading = true/false) ...
    });
```

### PantryViewModel

- [ ] **Step 3: Remove `isLoading` / `IsLoading` from `PantryViewModel.cs`**

Delete:
```csharp
private bool isLoading;
public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
```

- [ ] **Step 4: Replace busy-guard in `LoadPantryItemsAsync`**

Same pattern as HomeViewModel — wrap body in `ExecuteBusyAsync`.

### RecipeDetailViewModel

- [ ] **Step 5: Remove `isLoading` / `IsLoading` from `RecipeDetailViewModel.cs`**

Delete:
```csharp
private bool isLoading;
public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
```

Note: `isCooking` / `IsCooking` is a separate property — leave it untouched.

- [ ] **Step 6: Replace busy-guard in both guarded methods of `RecipeDetailViewModel`**

`RecipeDetailViewModel` has **two** methods that use the `IsLoading` guard — wrap both:

1. `LoadRecipeAsync` — the initial recipe load
2. The second method (around line 69) that guards with `if (Recipe is null || IsLoading) return`

For the second method the guard condition combines a null-check with `IsLoading`. After removing the field, rewrite as:

```csharp
if (Recipe is null) return;
await ExecuteBusyAsync(async () =>
{
    // ... original body without IsLoading guard ...
});
```

`ExecuteBusyAsync` already guards re-entrancy via `IsLoading` inherited from `BaseViewModel`.

### ShoppingListViewModel

- [ ] **Step 7: Remove `isLoading` / `IsLoading` from `ShoppingListViewModel.cs`**

Delete:
```csharp
private bool isLoading;
public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
```

- [ ] **Step 8: Replace busy-guard in `LoadShoppingListItemsAsync`**

Wrap load body in `ExecuteBusyAsync`.

### Verify

- [ ] **Step 9: Build full solution**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet build PantryToPlate.Core/ && dotnet build PantryToPlate.Tests/ 2>&1 | tail -5
```

Expected: `Build succeeded. 0 Error(s)`

- [ ] **Step 10: Run all tests**

```bash
export DOTNET_ROOT="/Volumes/APPLE HDD ST2000DM001 Media/dotnet-sdk" && export PATH="$DOTNET_ROOT:$PATH" && export NUGET_PACKAGES="/Volumes/APPLE HDD ST2000DM001 Media/nuget-cache" && dotnet test PantryToPlate.Tests/ 2>&1 | tail -10
```

Expected: `Passed! - Failed: 0` (27 tests)

- [ ] **Step 11: Commit**

```bash
git add PantryToPlate.Core/ViewModels/HomeViewModel.cs \
        PantryToPlate.Core/ViewModels/PantryViewModel.cs \
        PantryToPlate.Core/ViewModels/RecipeDetailViewModel.cs \
        PantryToPlate.Core/ViewModels/ShoppingListViewModel.cs
git commit -m "refactor: remove duplicate IsLoading from ViewModels, use BaseViewModel.ExecuteBusyAsync"
```
