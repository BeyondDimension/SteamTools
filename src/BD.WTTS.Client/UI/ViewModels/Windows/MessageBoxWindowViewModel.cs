// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public class MessageBoxWindowViewModel : DialogWindowViewModel
{
    [Reactive]
    public object? Content { get; set; }

    [Reactive]
    public bool IsCancelcBtn { get; set; }

    [Reactive]
    public bool IsShowRememberChoose { get; set; }

    [Reactive]
    public bool RememberChoose { get; set; }
}
