// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class UISettings_ : IUISettings
{
    public const string Name = nameof(UISettings_);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public AppTheme Theme { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string ThemeAccent { get; set; } = "#FF0078D7";

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public bool UseSystemThemeAccent { get; set; } = true;

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string Language { get; set; } = "";

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public HashSet<MessageBox.DontPromptType> MessageBoxDontPrompts { get; set; } = new()
    {
#if DEBUG
        MessageBox.DontPromptType.Undefined,
#endif
    };

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool IsShowAdvertisement { get; set; } = true;

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public ConcurrentDictionary<string, SizePosition> WindowSizePositions { get; set; } = new();

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public string FontName { get; set; } = "";

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public int GameListGridSize { get; set; } = 150;

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool Fillet { get; set; }

    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public double WindowBackgroundOpacity { get; set; } = .8;

    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public WindowBackgroundMaterial WindowBackgroundMaterial { get; set; }
        = OperatingSystem2.IsWindows11AtLeast() ? WindowBackgroundMaterial.Mica
        : WindowBackgroundMaterial.AcrylicBlur;

    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public bool WindowBackgroundDynamic { get; set; }

    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public bool WindowBackgroundCustomImage { get; set; }

    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public string WindowBackgroundCustomImagePath { get; set; } = "/UI/Assets/back.png";

    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public double WindowBackgroundCustomImageOpacity { get; set; } = .8;

    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public XamlMediaStretch WindowBackgroundCustomImageStretch { get; set; }
        = XamlMediaStretch.UniformToFill;
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(UISettings_))]
internal partial class UISettingsContext : JsonSerializerContext
{
    static UISettingsContext? instance;

    public static UISettingsContext Instance
        => instance ??= new UISettingsContext(ISettings.GetDefaultOptions());
}

partial class UISettings_ : ISettings<UISettings_>
{
    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => UISettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => UISettingsContext.Instance.UISettings_;

    static JsonTypeInfo<UISettings_> ISettings<UISettings_>.JsonTypeInfo
        => UISettingsContext.Instance.UISettings_;
}

[SettingsGeneration]
public static class UISettings
{
    /// <inheritdoc cref="IUISettings.Theme"/>
    public static SettingsProperty<AppTheme, UISettings_> Theme { get; } = new();

    /// <inheritdoc cref="IUISettings.ThemeAccent"/>
    public static SettingsProperty<string, UISettings_> ThemeAccent { get; } = new();

    /// <inheritdoc cref="IUISettings.UseSystemThemeAccent"/>
    public static SettingsProperty<bool, UISettings_> UseSystemThemeAccent { get; } = new();

    /// <inheritdoc cref="IUISettings.Language"/>
    public static SettingsProperty<string, UISettings_> Language { get; } = new();

    /// <inheritdoc cref="IUISettings.MessageBoxDontPrompts"/>
    public static SettingsProperty<HashSet<MessageBox.DontPromptType>, UISettings_> MessageBoxDontPrompts { get; } = new();

    /// <inheritdoc cref="IUISettings.IsShowAdvertisement"/>
    public static SettingsProperty<bool, UISettings_> IsShowAdvertisement { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowSizePositions"/>
    public static SettingsProperty<ConcurrentDictionary<string, SizePosition>, UISettings_> WindowSizePositions { get; } = new();

    /// <inheritdoc cref="IUISettings.FontName"/>
    public static SettingsProperty<string, UISettings_> FontName { get; } = new();

    /// <inheritdoc cref="IUISettings.GameListGridSize"/>
    public static SettingsProperty<int, UISettings_> GameListGridSize { get; } = new();

    /// <inheritdoc cref="IUISettings.Fillet"/>
    public static SettingsProperty<bool, UISettings_> Fillet { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundOpacity"/>
    public static SettingsProperty<double, UISettings_> WindowBackgroundOpacity { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundMaterial"/>
    public static SettingsProperty<WindowBackgroundMaterial, UISettings_> WindowBackgroundMaterial { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundDynamic"/>
    public static SettingsProperty<bool, UISettings_> WindowBackgroundDynamic { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImage"/>
    public static SettingsProperty<bool, UISettings_> WindowBackgroundCustomImage { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImagePath"/>
    public static SettingsProperty<string, UISettings_> WindowBackgroundCustomImagePath { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImageOpacity"/>
    public static SettingsProperty<double, UISettings_> WindowBackgroundCustomImageOpacity { get; } = new();

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImageStretch"/>
    public static SettingsProperty<XamlMediaStretch, UISettings_> WindowBackgroundCustomImageStretch { get; } = new();

}