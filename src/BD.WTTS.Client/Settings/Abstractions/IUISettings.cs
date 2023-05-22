// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public interface IUISettings : ISettings
{
    #region 主题

    /// <summary>
    /// 主题
    /// </summary>
    AppTheme Theme { get; set; }

    /// <summary>
    /// 主题强调色（16 进制 RGB 字符串）
    /// </summary>
    string ThemeAccent { get; set; }

    /// <summary>
    /// 从系统中获取主题强调色
    /// </summary>
    bool UseSystemThemeAccent { get; set; }

    #endregion

    /// <summary>
    /// 语言
    /// </summary>
    string Language { get; set; }

    /// <summary>
    /// 不再提示的消息框
    /// </summary>
    HashSet<MessageBox.DontPromptType> MessageBoxDontPrompts { get; set; }

    /// <summary>
    /// 是否显示广告
    /// </summary>
    bool IsShowAdvertisement { get; set; }

    /// <summary>
    /// 窗口位置大小
    /// </summary>
    ConcurrentDictionary<string, SizePosition> WindowSizePositions { get; set; }

    /// <summary>
    /// 字体
    /// </summary>
    string FontName { get; set; }

    /// <summary>
    /// 库存游戏网格布局大小
    /// </summary>
    int GameListGridSize { get; set; }

    /// <summary>
    /// 圆角
    /// </summary>
    bool Fillet { get; set; }

    #region WindowBackground 窗口背景

    /// <summary>
    /// 窗口背景不透明度
    /// </summary>
    double WindowBackgroundOpacity { get; set; }

    /// <summary>
    /// 窗口背景材质
    /// </summary>
    WindowBackgroundMaterial WindowBackgroundMaterial { get; set; }

    /// <summary>
    /// 动态桌面背景
    /// </summary>
    bool WindowBackgroundDynamic { get; set; }

    /// <summary>
    /// 自定义背景图像
    /// </summary>
    bool WindowBackgroundCustomImage { get; set; }

    /// <summary>
    /// 自定义背景图像路径
    /// </summary>
    string WindowBackgroundCustomImagePath { get; set; }

    /// <summary>
    /// 自定义背景图像不透明度
    /// </summary>
    double WindowBackgroundCustomImageOpacity { get; set; }

    /// <summary>
    /// 自定义背景图像缩放方式
    /// </summary>
    XamlMediaStretch WindowBackgroundCustomImageStretch { get; set; }

    #endregion
}
