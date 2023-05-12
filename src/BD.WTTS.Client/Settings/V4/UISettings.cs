using System.Text.Json.Serialization.Metadata;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class UISettings_ : BaseNotifyPropertyChanged, IUISettings
{
    public const string Name = "UI";

    [Reactive]
    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public AppTheme Theme { get; set; }

    [Reactive]
    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public string ThemeAccent { get; set; } = "";

    [Reactive]
    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public bool UseSystemThemeAccent { get; set; }

    [Reactive]
    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public string Language { get; set; } = "";

    [Reactive]
    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public HashSet<MessageBox.DontPromptType> MessageBoxDontPrompts { get; set; } = new()
    {
#if DEBUG
        MessageBox.DontPromptType.Undefined,
#endif
    };

    [Reactive]
    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool IsShowAdvertisement { get; set; }

    [Reactive]
    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]
    public ConcurrentDictionary<string, SizePosition> WindowSizePositions { get; set; } = new();

    [Reactive]
    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public string FontName { get; set; } = "";

    [Reactive]
    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public int GameListGridSize { get; set; }

    [Reactive]
    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool Fillet { get; set; }

    [Reactive]
    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public double WindowBackgroundOpacity { get; set; }

    [Reactive]
    [MPKey(11), MP2Key(11), JsonPropertyOrder(11)]
    public WindowBackgroundMaterial WindowBackgroundMaterial { get; set; }

    [Reactive]
    [MPKey(12), MP2Key(12), JsonPropertyOrder(12)]
    public bool WindowBackgroundDynamic { get; set; }

    [Reactive]
    [MPKey(13), MP2Key(13), JsonPropertyOrder(13)]
    public bool WindowBackgroundCustomImage { get; set; }

    [Reactive]
    [MPKey(14), MP2Key(14), JsonPropertyOrder(14)]
    public string WindowBackgroundCustomImagePath { get; set; } = "";

    [Reactive]
    [MPKey(15), MP2Key(15), JsonPropertyOrder(15)]
    public double WindowBackgroundCustomImageOpacity { get; set; }

    [Reactive]
    [MPKey(16), MP2Key(16), JsonPropertyOrder(16)]
    public XamlMediaStretch WindowBackgroundCustomImageStretch { get; set; }
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

    static JsonTypeInfo<UISettings_> ISettings<UISettings_>.JsonTypeInfo
        => UISettingsContext.Instance.UISettings_;
}