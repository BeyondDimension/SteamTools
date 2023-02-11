namespace BD.WTTS.Settings;

public sealed partial class UISettings : SettingsHost2<UISettings>
{
    /// <summary>
    /// 主题
    /// </summary>
    public static SerializableProperty<short> Theme { get; }
        = GetProperty(defaultValue: (short)0);

    /// <summary>
    /// 语言
    /// </summary>
    public static SerializableProperty<string> Language { get; }
        = GetProperty(defaultValue: string.Empty);

    /// <summary>
    /// 不再提示的消息框数组
    /// </summary>
    public static SerializableProperty<HashSet<MessageBox.DontPromptType>?> DoNotShowMessageBoxs { get; }
        = GetProperty<HashSet<MessageBox.DontPromptType>?>(defaultValue: null, autoSave: false);

    /// <summary>
    /// 是否显示广告
    /// </summary>
    public static SerializableProperty<bool> IsShowAdvertise { get; }
        = GetProperty(defaultValue: true);

    static readonly SerializableProperty<ConcurrentDictionary<string, SizePosition>?>? _WindowSizePositions = WindowViewModel.IsSupportedSizePosition ?
      GetProperty<ConcurrentDictionary<string, SizePosition>?>(defaultValue: null, autoSave: false) : null;

    /// <summary>
    /// 所有窗口位置记忆字典集合
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<ConcurrentDictionary<string, SizePosition>?> WindowSizePositions => _WindowSizePositions ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<string>? _FontName = IApplication.IsDesktop() ? GetProperty(defaultValue: IFontManager.KEY_Default) : null;

    /// <summary>
    /// 字体
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<string> FontName => _FontName ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<double>? _AcrylicOpacity = IApplication.IsDesktop() ? GetProperty(defaultValue: .8) : null;

    /// <summary>
    /// 窗口背景透明度
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<double> AcrylicOpacity => _AcrylicOpacity ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<int>? _WindowBackgroundMateria = IApplication.IsDesktop() ? GetProperty(defaultValue: /*OperatingSystem2.IsWindows7() ? 0 : (*/OperatingSystem2.IsWindows11AtLeast() ? 5 : 3)/*)*/ : null;

    /// <summary>
    /// 窗口背景材质
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<int> WindowBackgroundMateria => _WindowBackgroundMateria ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<int>? _AppGridSize = IApplication.IsDesktop() ? GetProperty(defaultValue: 150) : null;

    /// <summary>
    /// 库存游戏封面大小
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<int> AppGridSize => _AppGridSize ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _SteamAccountRemarkReplaceName = IApplication.IsDesktop() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// Steam 账号备注替换名称显示
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> SteamAccountRemarkReplaceName => _SteamAccountRemarkReplaceName ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _EnableFilletUI = IApplication.IsDesktop() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 启用圆角界面
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> EnableFilletUI => _EnableFilletUI ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _EnableDesktopBackground = IApplication.IsDesktop() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 启用动态桌面背景
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> EnableDesktopBackground => _EnableDesktopBackground ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _EnableCustomBackgroundImage
        = IApplication.IsDesktop() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 启用自定义背景图片
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> EnableCustomBackgroundImage => _EnableCustomBackgroundImage ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<string>? _BackgroundImagePath = IApplication.IsDesktop() ? GetProperty(defaultValue: "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/AppResources/Placeholders/0.png") : null;

    /// <summary>
    /// 背景图片路径
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<string> BackgroundImagePath => _BackgroundImagePath ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<string>? _ThemeAccent = IApplication.IsDesktop() ? GetProperty(defaultValue: "#FF0078D7") : null;

    /// <summary>
    /// 主题颜色(十六进制字符串)
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<string> ThemeAccent => _ThemeAccent ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _GetUserThemeAccent = IApplication.IsDesktop() ? GetProperty(defaultValue: true) : null;

    /// <summary>
    /// 主题颜色从系统获取
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> GetUserThemeAccent => _GetUserThemeAccent ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<bool>? _MainMenuExpandedState = IApplication.IsDesktop() ? GetProperty(defaultValue: false) : null;

    /// <summary>
    /// 主菜单展开状态
    /// </summary>
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Linux")]
    public static SerializableProperty<bool> MainMenuExpandedState => _MainMenuExpandedState ?? throw new PlatformNotSupportedException();
}

//static void EnableDesktopBackground_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
//{
//    if (e.NewValue)
//    {
//        IApplication.Instance.SetDesktopBackgroundWindow();
//    }
//    else
//    {
//        INativeWindowApiService.Instance.ResetWallerpaper();
//    }
//}

//static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
//{
//    // 当前 Avalonia App 主题切换存在问题
//    //if (OperatingSystem2.Application.UseAvalonia()) return;
//    if (e.NewValue != e.OldValue)
//    {
//        var value = (AppTheme)e.NewValue;
//        if (value.IsDefined())
//        {
//            IApplication.Instance.Theme = value;
//        }
//    }
//}