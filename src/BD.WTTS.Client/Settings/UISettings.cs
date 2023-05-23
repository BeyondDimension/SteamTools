using static BD.WTTS.Settings.Abstractions.IUISettings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class UISettings_ : IUISettings, ISettings
{
    public const string Name = nameof(UISettings);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public AppTheme? Theme { get; set; }

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string? ThemeAccent { get; set; }

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public bool? UseSystemThemeAccent { get; set; }

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string? Language { get; set; }

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public HashSet<MessageBox.DontPromptType> MessageBoxDontPrompts { get; set; } = new()
    {
#if DEBUG
        MessageBox.DontPromptType.Undefined,
#endif
    };

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool? IsShowAdvertisement { get; set; }

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public ConcurrentDictionary<string, SizePosition> WindowSizePositions { get; set; } = new();

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public string? FontName { get; set; }

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public int? GameListGridSize { get; set; }

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool? Fillet { get; set; }

    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public double? WindowBackgroundOpacity { get; set; }

    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public WindowBackgroundMaterial? WindowBackgroundMaterial { get; set; }

    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public bool? WindowBackgroundDynamic { get; set; }

    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public bool? WindowBackgroundCustomImage { get; set; }

    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public string? WindowBackgroundCustomImagePath { get; set; }

    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public double? WindowBackgroundCustomImageOpacity { get; set; }

    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public XamlMediaStretch? WindowBackgroundCustomImageStretch { get; set; }
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
    public static SettingsStructProperty<AppTheme, UISettings_> Theme { get; }
        = new(DefaultTheme);

    /// <inheritdoc cref="IUISettings.ThemeAccent"/>
    public static SettingsProperty<string, UISettings_> ThemeAccent { get; }
        = new(DefaultThemeAccent);

    /// <inheritdoc cref="IUISettings.UseSystemThemeAccent"/>
    public static SettingsStructProperty<bool, UISettings_> UseSystemThemeAccent { get; }
        = new(DefaultUseSystemThemeAccent);

    /// <inheritdoc cref="IUISettings.Language"/>
    public static SettingsProperty<string, UISettings_> Language { get; } = new();

    /// <inheritdoc cref="IUISettings.MessageBoxDontPrompts"/>
    public static SettingsProperty<MessageBox.DontPromptType, HashSet<MessageBox.DontPromptType>, UISettings_> MessageBoxDontPrompts { get; } = new();

    /// <inheritdoc cref="IUISettings.IsShowAdvertisement"/>
    public static SettingsStructProperty<bool, UISettings_> IsShowAdvertisement { get; }
        = new(DefaultIsShowAdvertisement);

    /// <inheritdoc cref="IUISettings.WindowSizePositions"/>
    public static SettingsProperty<KeyValuePair<string, SizePosition>, ConcurrentDictionary<string, SizePosition>, UISettings_> WindowSizePositions { get; } = new();

    /// <inheritdoc cref="IUISettings.FontName"/>
    public static SettingsProperty<string, UISettings_> FontName { get; } = new();

    /// <inheritdoc cref="IUISettings.GameListGridSize"/>
    public static SettingsStructProperty<int, UISettings_> GameListGridSize { get; }
        = new(DefaultGameListGridSize);

    /// <inheritdoc cref="IUISettings.Fillet"/>
    public static SettingsStructProperty<bool, UISettings_> Fillet { get; }
        = new(DefaultFillet);

    /// <inheritdoc cref="IUISettings.WindowBackgroundOpacity"/>
    public static SettingsStructProperty<double, UISettings_> WindowBackgroundOpacity { get; }
        = new(DefaultWindowBackgroundOpacity);

    /// <inheritdoc cref="IUISettings.WindowBackgroundMaterial"/>
    public static SettingsStructProperty<WindowBackgroundMaterial, UISettings_> WindowBackgroundMaterial { get; }
        = new(DefaultWindowBackgroundMaterial);

    /// <inheritdoc cref="IUISettings.WindowBackgroundDynamic"/>
    public static SettingsStructProperty<bool, UISettings_> WindowBackgroundDynamic { get; }
        = new(DefaultWindowBackgroundDynamic);

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImage"/>
    public static SettingsStructProperty<bool, UISettings_> WindowBackgroundCustomImage { get; }
        = new(DefaultWindowBackgroundCustomImage);

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImagePath"/>
    public static SettingsProperty<string, UISettings_> WindowBackgroundCustomImagePath { get; }
        = new(DefaultWindowBackgroundCustomImagePath);

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImageOpacity"/>
    public static SettingsStructProperty<double, UISettings_> WindowBackgroundCustomImageOpacity { get; }
        = new(DefaultWindowBackgroundCustomImageOpacity);

    /// <inheritdoc cref="IUISettings.WindowBackgroundCustomImageStretch"/>
    public static SettingsStructProperty<XamlMediaStretch, UISettings_> WindowBackgroundCustomImageStretch { get; }
        = new(DefaultWindowBackgroundCustomImageStretch);

}