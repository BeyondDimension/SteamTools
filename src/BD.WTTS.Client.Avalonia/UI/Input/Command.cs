using System;
using System.Windows.Input;

namespace BD.WTTS.UI;

internal class Command : ICommand
{
    private readonly Action<object?> _onExecute;
    private readonly Func<object?, bool>? _onCanExecute;

    public Command(Action<object?> onExecute, Func<object?, bool>? onCanExecute = default)
    {
        _onExecute = onExecute;
        _onCanExecute = onCanExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) =>
       _onCanExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) =>
        _onExecute.Invoke(parameter);

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
