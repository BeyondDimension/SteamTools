// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class WindowViewModel : PageViewModel, IWindowViewModel
{
    public WindowViewModel()
    {
        Close = _ => windowManager.CloseWindow(this);
        Show = () => windowManager.ShowWindow(this);
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    protected readonly IWindowManager windowManager = IWindowManager.Instance;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsVisible => windowManager.IsVisibleWindow(this);

    /// <summary>
    /// 关闭当前 ViewModel 绑定的窗口 参数为是否正常关闭
    /// </summary>
    public virtual Action<bool>? Close { get; set; }

    /// <summary>
    /// 显示当前 ViewModel 绑定的窗口
    /// </summary>
    public virtual Action Show { get; set; }

    /// <summary>
    /// 隐藏当前 ViewModel 绑定的窗口
    /// </summary>
    public virtual void Hide() => windowManager.HideWindow(this);

    public virtual void OnClosing(object? sender, CancelEventArgs e) { }
}
