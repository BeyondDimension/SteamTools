// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

public sealed partial class GeneralSettings : SettingsHost2<GeneralSettings>
{
    /// <summary>
    /// 自动检查更新
    /// </summary>
    public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 下载更新渠道
    /// </summary>
    public static SerializableProperty<UpdateChannelType> UpdateChannel { get; }
        = GetProperty(defaultValue: default(UpdateChannelType));

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    static readonly SerializableProperty<bool> _WindowsStartupAutoRun = GetProperty(defaultValue: false);

    /// <summary>
    /// 程序是否开机自启动
    /// </summary>
    public static SerializableProperty<bool> WindowsStartupAutoRun => _WindowsStartupAutoRun;

    static readonly SerializableProperty<bool> _IsStartupAppMinimized = GetProperty(defaultValue: false);

    /// <summary>
    /// 程序启动时最小化
    /// </summary>
    public static SerializableProperty<bool> IsStartupAppMinimized => _IsStartupAppMinimized;

    static readonly SerializableProperty<bool> _IsEnableTrayIcon = GetProperty(defaultValue: true);

    /// <summary>
    /// 启用托盘
    /// </summary>
    public static SerializableProperty<bool> IsEnableTrayIcon => _IsEnableTrayIcon;

    static readonly SerializableProperty<bool> _IsSteamAppListLocalCache
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 启用游戏列表本地缓存
    /// </summary>
    public static SerializableProperty<bool> IsSteamAppListLocalCache => _IsSteamAppListLocalCache;

    static readonly SerializableProperty<IReadOnlyDictionary<Platform, string>> _TextReaderProvider = GetProperty(defaultValue: (IReadOnlyDictionary<Platform, string>?)null);

    /// <summary>
    /// 用户设置的文本阅读器提供商，根据平台值不同，值格式为 枚举字符串 或 程序路径
    /// </summary>
    public static SerializableProperty<IReadOnlyDictionary<Platform, string>> TextReaderProvider => _TextReaderProvider ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<EncodingType> _HostsEncodingType = GetProperty(defaultValue: default(EncodingType));

    /// <summary>
    /// Hosts 文件编码类型
    /// </summary>
    public static SerializableProperty<EncodingType> HostsEncodingType => _HostsEncodingType;

    static readonly SerializableProperty<bool> _UseGPURendering = GetProperty(defaultValue: true);

    /// <summary>
    /// 使用硬件加速
    /// </summary>
    public static SerializableProperty<bool> UseGPURendering => _UseGPURendering;

    static readonly SerializableProperty<bool> _UseWgl = GetProperty(defaultValue: false);

    /// <summary>
    /// (仅 Windows)Avalonia would try to use native Widows OpenGL when set to true. The default value is false.
    /// </summary>
    [SupportedOSPlatform("Windows")]
    public static SerializableProperty<bool> UseWgl => _UseWgl;

    ///// <summary>
    ///// 使用 Direct2D1 渲染(仅 Windows)
    ///// </summary>
    //public static SerializableProperty<bool> UseDirect2D1 { get; }
    //    = GetProperty(defaultValue: false, autoSave: true);

    ///// <summary>
    ///// 创建桌面快捷方式
    ///// </summary>
    //public static SerializableProperty<bool> CreateDesktopShortcut { get; }
    //    = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

    ///// <summary>
    ///// 是否显示起始页
    ///// </summary>
    //public static SerializableProperty<bool> IsShowStartPage { get; }
    //    = GetProperty(defaultValue: true, autoSave: true);

    ///// <summary>
    ///// 启用错误日志记录
    ///// </summary>
    //public static SerializableProperty<bool> IsEnableLogRecord { get; }
    //    = GetProperty(defaultValue: false, autoSave: true);

    //static readonly SerializableProperty<bool> _UseWinHttpHandler = GetProperty(defaultValue: false);

    ///// <summary>
    ///// (仅 Windows)使用基于 Windows 的 WinHTTP 接口处理消息
    ///// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.net.http.winhttphandler?view=dotnet-plat-ext-6.0</para>
    ///// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.net.http.socketshttphandler?view=net-6.0</para>
    ///// </summary>
    //[SupportedOSPlatform("Windows")]
    //public static SerializableProperty<bool> UseWinHttpHandler => _UseWinHttpHandler;

#endif

#if ANDROID

    static readonly SerializableProperty<bool> _CaptureScreen = GetProperty(defaultValue: false);

    /// <summary>
    /// 屏幕捕获(允许截图)
    /// </summary>
    public static SerializableProperty<bool> CaptureScreen => _CaptureScreen;

#endif
}
