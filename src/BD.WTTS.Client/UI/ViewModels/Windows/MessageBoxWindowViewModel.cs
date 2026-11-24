// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class MessageBoxWindowViewModel : DialogWindowViewModel
{
    [Reactive]
    public partial object? Content { get; set; }

    [Reactive]
    public partial bool IsCancelcBtn { get; set; }

    [Reactive]
    public partial bool IsShowRememberChoose { get; set; }

    [Reactive]
    public partial bool RememberChoose { get; set; }
}
