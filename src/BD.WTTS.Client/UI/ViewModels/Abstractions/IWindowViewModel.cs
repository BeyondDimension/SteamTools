namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IWindowViewModel : IPageViewModel
{
    /// <summary>
    /// 是否支持窗口大小与位置
    /// </summary>
    static bool IsSupportedSizePosition { protected get; set; } = IApplication.IsDesktop();

    abstract void OnClosing(object? sender, CancelEventArgs e);
}
