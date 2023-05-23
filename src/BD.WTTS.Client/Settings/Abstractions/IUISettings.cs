// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface IUISettings
{
    static IUISettings? Instance => Ioc.Get_Nullable<IOptionsMonitor<IUISettings>>()?.CurrentValue;

    #region 主题

    /// <summary>
    /// 主题
    /// </summary>
    AppTheme? Theme { get; set; }

    const AppTheme DefaultTheme = AppTheme.FollowingSystem;

    /// <summary>
    /// 主题强调色（16 进制 RGB 字符串）
    /// </summary>
    string? ThemeAccent { get; set; }

    const string DefaultThemeAccent = "#FF0078D7";

    /// <summary>
    /// 从系统中获取主题强调色
    /// </summary>
    bool? UseSystemThemeAccent { get; set; }

    const bool DefaultUseSystemThemeAccent = true;

    #endregion

    /// <summary>
    /// 语言
    /// </summary>
    string? Language { get; set; }

    /// <summary>
    /// 不再提示的消息框
    /// </summary>
    HashSet<MessageBox.DontPromptType> MessageBoxDontPrompts { get; set; }

    /// <summary>
    /// 是否显示广告
    /// </summary>
    bool? IsShowAdvertisement { get; set; }

    const bool DefaultIsShowAdvertisement = true;

    /// <summary>
    /// 窗口位置大小
    /// </summary>
    ConcurrentDictionary<string, SizePosition> WindowSizePositions { get; set; }

    /// <summary>
    /// 字体
    /// </summary>
    string? FontName { get; set; }

    /// <summary>
    /// 库存游戏网格布局大小
    /// </summary>
    int? GameListGridSize { get; set; }

    const int DefaultGameListGridSize = 150;

    /// <summary>
    /// 圆角
    /// </summary>
    bool? Fillet { get; set; }

    const bool DefaultFillet = false;

    #region WindowBackground 窗口背景

    /// <summary>
    /// 窗口背景不透明度
    /// </summary>
    double? WindowBackgroundOpacity { get; set; }

    const double DefaultWindowBackgroundOpacity = .8;

    /// <summary>
    /// 窗口背景材质
    /// </summary>
    WindowBackgroundMaterial? WindowBackgroundMaterial { get; set; }

    static readonly WindowBackgroundMaterial DefaultWindowBackgroundMaterial
        = OperatingSystem2.IsWindows11AtLeast() ? Enums.WindowBackgroundMaterial.Mica
        : Enums.WindowBackgroundMaterial.AcrylicBlur;

    /// <summary>
    /// 动态桌面背景
    /// </summary>
    bool? WindowBackgroundDynamic { get; set; }

    const bool DefaultWindowBackgroundDynamic = false;

    /// <summary>
    /// 自定义背景图像
    /// </summary>
    bool? WindowBackgroundCustomImage { get; set; }

    const bool DefaultWindowBackgroundCustomImage = false;

    /// <summary>
    /// 自定义背景图像路径
    /// </summary>
    string? WindowBackgroundCustomImagePath { get; set; }

    const string DefaultWindowBackgroundCustomImagePath = "/UI/Assets/back.png";

    /// <summary>
    /// 自定义背景图像不透明度
    /// </summary>
    double? WindowBackgroundCustomImageOpacity { get; set; }

    const double DefaultWindowBackgroundCustomImageOpacity = .8;

    /// <summary>
    /// 自定义背景图像缩放方式
    /// </summary>
    XamlMediaStretch? WindowBackgroundCustomImageStretch { get; set; }

    const XamlMediaStretch DefaultWindowBackgroundCustomImageStretch = XamlMediaStretch.UniformToFill;

    #endregion
}
