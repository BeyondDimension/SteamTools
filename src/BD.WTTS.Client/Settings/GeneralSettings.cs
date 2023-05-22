// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class GeneralSettings_ : IGeneralSettings, ISettings
{
    public const string Name = nameof(GeneralSettings);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public bool AutoCheckAppUpdate { get; set; } = true;

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public UpdateChannelType UpdateChannel { get; set; } = UpdateChannelType.Auto;

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public bool AutoRunOnStartup { get; set; }

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public bool MinimizeOnStartup { get; set; }

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public bool TrayIcon { get; set; }

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool GameListUseLocalCache { get; set; }

    [MPKey(6), MP2Key(6), JsonPropertyOrder(6)]

    public Dictionary<Platform, string> TextReaderProvider { get; set; } = new()
    {
#if DEBUG
        { Platform.WinUI, "Test" },
#endif
    };

    [MPKey(7), MP2Key(7), JsonPropertyOrder(7)]
    public EncodingType HostsFileEncodingType { get; set; }

    [MPKey(8), MP2Key(8), JsonPropertyOrder(8)]
    public bool GPU { get; set; } = true;

    [MPKey(9), MP2Key(9), JsonPropertyOrder(9)]
    public bool NativeOpenGL { get; set; }

    [MPKey(10), MP2Key(10), JsonPropertyOrder(10)]
    public bool ScreenCapture { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(GeneralSettings_))]
internal partial class GeneralSettingsContext : JsonSerializerContext
{
    static GeneralSettingsContext? instance;

    public static GeneralSettingsContext Instance
        => instance ??= new GeneralSettingsContext(ISettings.GetDefaultOptions());
}

partial class GeneralSettings_ : ISettings<GeneralSettings_>
{
    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => GeneralSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => GeneralSettingsContext.Instance.GeneralSettings_;

    static JsonTypeInfo<GeneralSettings_> ISettings<GeneralSettings_>.JsonTypeInfo
        => GeneralSettingsContext.Instance.GeneralSettings_;
}

[SettingsGeneration]
public static class GeneralSettings
{
    /// <inheritdoc cref="IGeneralSettings.AutoCheckAppUpdate"/>
    public static SettingsProperty<bool, GeneralSettings_> AutoCheckAppUpdate { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.UpdateChannel"/>
    public static SettingsProperty<UpdateChannelType, GeneralSettings_> UpdateChannel { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.AutoRunOnStartup"/>
    public static SettingsProperty<bool, GeneralSettings_> AutoRunOnStartup { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.MinimizeOnStartup"/>
    public static SettingsProperty<bool, GeneralSettings_> MinimizeOnStartup { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.TrayIcon"/>
    public static SettingsProperty<bool, GeneralSettings_> TrayIcon { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.GameListUseLocalCache"/>
    public static SettingsProperty<bool, GeneralSettings_> GameListUseLocalCache { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.TextReaderProvider"/>
    public static SettingsProperty<Dictionary<Platform, string>, GeneralSettings_> TextReaderProvider { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.HostsFileEncodingType"/>
    public static SettingsProperty<EncodingType, GeneralSettings_> HostsFileEncodingType { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.GPU"/>
    public static SettingsProperty<bool, GeneralSettings_> GPU { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.NativeOpenGL"/>
    public static SettingsProperty<bool, GeneralSettings_> NativeOpenGL { get; } = new();

    /// <inheritdoc cref="IGeneralSettings.ScreenCapture"/>
    public static SettingsProperty<bool, GeneralSettings_> ScreenCapture { get; } = new();
}
