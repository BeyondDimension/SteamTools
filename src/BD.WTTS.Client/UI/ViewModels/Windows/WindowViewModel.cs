// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class WindowViewModel : PageViewModel, IWindowViewModel
{
    public WindowViewModel()
    {
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    protected readonly IWindowManager windowManager = IWindowManager.Instance;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsVisible => windowManager.IsVisibleWindow(this);

    /// <summary>
    /// 关闭当前 ViewModel 绑定的窗口
    /// </summary>
    public virtual void Close() => windowManager.CloseWindow(this);

    /// <summary>
    /// 显示当前 ViewModel 绑定的窗口
    /// </summary>
    public virtual void Show() => windowManager.ShowWindow(this);

    /// <summary>
    /// 隐藏当前 ViewModel 绑定的窗口
    /// </summary>
    public virtual void Hide() => windowManager.HideWindow(this);

    public virtual void OnClosing(object? sender, CancelEventArgs e) { }
}
