// ReSharper disable once CheckNamespace
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

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    static readonly SerializableProperty<ConcurrentDictionary<string, SizePosition>?>? _WindowSizePositions = IApplication.IsDesktop() ?
      GetProperty<ConcurrentDictionary<string, SizePosition>?>(defaultValue: null, autoSave: false) : null;

    /// <summary>
    /// 所有窗口位置记忆字典集合
    /// </summary>
    public static SerializableProperty<ConcurrentDictionary<string, SizePosition>?> WindowSizePositions => _WindowSizePositions ?? throw new PlatformNotSupportedException();

    static readonly SerializableProperty<string> _FontName = GetProperty(defaultValue: IFontManager.KEY_Default);

    /// <summary>
    /// 字体
    /// </summary>
    public static SerializableProperty<string> FontName => _FontName;

    static readonly SerializableProperty<double> _AcrylicOpacity = GetProperty(defaultValue: .8);

    /// <summary>
    /// 窗口背景透明度
    /// </summary>
    public static SerializableProperty<double> AcrylicOpacity => _AcrylicOpacity;

    static readonly SerializableProperty<int> _WindowBackgroundMateria = GetProperty(defaultValue: /*OperatingSystem2.IsWindows7() ? 0 : (*/OperatingSystem2.IsWindows11AtLeast() ? 5 : 3)/*)*/;

    /// <summary>
    /// 窗口背景材质
    /// </summary>
    public static SerializableProperty<int> WindowBackgroundMateria => _WindowBackgroundMateria;

    static readonly SerializableProperty<int> _AppGridSize = GetProperty(defaultValue: 150);

    /// <summary>
    /// 库存游戏封面大小
    /// </summary>
    public static SerializableProperty<int> AppGridSize => _AppGridSize;

    static readonly SerializableProperty<bool> _SteamAccountRemarkReplaceName = GetProperty(defaultValue: false);

    /// <summary>
    /// Steam 账号备注替换名称显示
    /// </summary>
    public static SerializableProperty<bool> SteamAccountRemarkReplaceName => _SteamAccountRemarkReplaceName;

    static readonly SerializableProperty<bool> _EnableFilletUI = GetProperty(defaultValue: false);

    /// <summary>
    /// 启用圆角界面
    /// </summary>
    public static SerializableProperty<bool> EnableFilletUI => _EnableFilletUI;

    static readonly SerializableProperty<bool> _EnableDesktopBackground = GetProperty(defaultValue: false);

    /// <summary>
    /// 启用动态桌面背景
    /// </summary>
    public static SerializableProperty<bool> EnableDesktopBackground => _EnableDesktopBackground;

    static readonly SerializableProperty<bool> _EnableCustomBackgroundImage
        = GetProperty(defaultValue: false);

    /// <summary>
    /// 启用自定义背景图片
    /// </summary>
    public static SerializableProperty<bool> EnableCustomBackgroundImage => _EnableCustomBackgroundImage;

    static readonly SerializableProperty<string> _BackgroundImagePath = GetProperty(defaultValue: "/UI/Assets/back.png");

    /// <summary>
    /// 背景图片路径
    /// </summary>
    public static SerializableProperty<string> BackgroundImagePath => _BackgroundImagePath;

    static readonly SerializableProperty<string> _ThemeAccent = GetProperty(defaultValue: "#FF0078D7");

    /// <summary>
    /// 主题颜色(十六进制字符串)
    /// </summary>
    public static SerializableProperty<string> ThemeAccent => _ThemeAccent;

    static readonly SerializableProperty<bool> _GetUserThemeAccent = GetProperty(defaultValue: true);

    /// <summary>
    /// 主题颜色从系统获取
    /// </summary>
    public static SerializableProperty<bool> GetUserThemeAccent => _GetUserThemeAccent;

    static readonly SerializableProperty<bool> _MainMenuExpandedState = GetProperty(defaultValue: false);

    /// <summary>
    /// 主菜单展开状态
    /// </summary>
    public static SerializableProperty<bool> MainMenuExpandedState => _MainMenuExpandedState;

#endif
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