// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 提供对显示在主窗口底部的状态栏的访问
/// </summary>
[Obsolete("不再使用常驻通知")]
public sealed class ToastService : ReactiveObject
{
    static readonly Lazy<ToastService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ToastService Current => mCurrent.Value;

    static readonly Lazy<bool> mIsSupported = new(() => Ioc.Get<IToast>() is ToastImpl);

    public static bool IsSupported => mIsSupported.Value;

    readonly Subject<string> notifier;
    string persisitentMessage = "";
    string notificationMessage = "";

    #region Message 变更通知

    /// <summary>
    /// 获取指示当前状态的字符串。
    /// </summary>
    public string Message
    {
        get => notificationMessage ?? persisitentMessage;
        set
        {
            notificationMessage = value;
            persisitentMessage = value;
            this.RaisePropertyChanged();
        }
    }

    #endregion

    /// <summary>
    /// 显示状态
    /// </summary>
    bool _IsVisible;

    public bool IsVisible
    {
        get => _IsVisible;
        set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
    }

    ToastService()
    {
        notifier = new Subject<string>();
        notifier
            .Do(x =>
            {
                notificationMessage = x;
                RaiseMessagePropertyChanged();
            })
            .Throttle(TimeSpan.FromMilliseconds(5000))
            .Subscribe(_ =>
            {
                notificationMessage = string.Empty;
                RaiseMessagePropertyChanged();
            });

        this.WhenAnyValue(x => x.Message)
                 .Subscribe(x => IsVisible = !string.IsNullOrEmpty(x));
    }

    public void Set()
    {
        CloseBtn_Click();
    }

    [Obsolete("不再使用常驻通知")]
    public void Set(string message)
    {
        MainThread2.BeginInvokeOnMainThread(() => Message = message);
    }

    public void Notify(string message)
    {
        notifier.OnNext(message);
    }

    void RaiseMessagePropertyChanged()
    {
        this.RaisePropertyChanged(nameof(Message));
    }

    public void CloseBtn_Click()
    {
        Set("");
        IsVisible = false;
    }
}