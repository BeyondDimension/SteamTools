// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IWindowViewModel : IPageViewModel
{
    /// <summary>
    /// 关闭当前 ViewModel 绑定的窗口
    /// </summary>
    Action<bool>? Close { get; set; }

    /// <summary>
    /// 显示当前 ViewModel 绑定的窗口
    /// </summary>
    Action Show { get; set; }

    abstract void OnClosing(object? sender, CancelEventArgs e);
}
