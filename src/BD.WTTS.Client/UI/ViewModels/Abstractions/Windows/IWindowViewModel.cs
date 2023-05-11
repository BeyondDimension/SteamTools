// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IWindowViewModel : IPageViewModel
{
    abstract void OnClosing(object? sender, CancelEventArgs e);
}
