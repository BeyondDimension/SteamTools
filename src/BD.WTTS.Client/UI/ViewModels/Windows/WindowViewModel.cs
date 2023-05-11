// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class WindowViewModel : PageViewModel, IWindowViewModel
{
    protected SizePosition? _SizePosition;

    public SizePosition SizePosition
    {
        get
        {
            _SizePosition ??= new();
            return _SizePosition;
        }
        set => this.RaiseAndSetIfChanged(ref _SizePosition, value);
    }

    public WindowViewModel()
    {
        var name = GetType().Name;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
        bool isSupportedSizePosition = IApplication.IsDesktop();
        if (isSupportedSizePosition)
        {
            if (UISettings.WindowSizePositions.Value?.ContainsKey(name) == true)
            {
                _SizePosition = UISettings.WindowSizePositions.Value[name];
            }

            this.WhenAnyValue(x => x.SizePosition.X, c => c.SizePosition.Y, v => v.SizePosition.Width, b => b.SizePosition.Height)
                .Subscribe(x =>
                {
                    if (x.Item1 == 0 && x.Item2 == 0 && x.Item3 == 0 && x.Item4 == 0)
                        return;
                    if (UISettings.WindowSizePositions.Value == null)
                        UISettings.WindowSizePositions.Value = new ConcurrentDictionary<string, SizePosition>();
                    else if (UISettings.WindowSizePositions.Value.ContainsKey(name))
                        UISettings.WindowSizePositions.Value[name] = SizePosition;
                    else
                        UISettings.WindowSizePositions.Value.TryAdd(name, SizePosition);
                    UISettings.WindowSizePositions.RaiseValueChanged();
                }).AddTo(this);
        }
#endif
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
