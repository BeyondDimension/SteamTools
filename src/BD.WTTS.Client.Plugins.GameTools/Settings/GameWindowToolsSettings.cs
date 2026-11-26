#if WINDOWS
namespace BD.WTTS.Settings;

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(GameWindowToolsSettings_))]
internal partial class GameWindowToolsSettingsContext : JsonSerializerContext
{
    internal static GameWindowToolsSettingsContext Instance { get; } = new(ISettings.GetDefaultOptions());
}

public sealed class GameWindowToolsSettings_ : ISettings, ISettings<GameWindowToolsSettings_>
{
    static string ISettings.Name => nameof(GameWindowToolsSettings);
    static JsonSerializerContext ISettings.JsonSerializerContext => GameWindowToolsSettingsContext.Instance;
    static JsonTypeInfo ISettings.JsonTypeInfo => GameWindowToolsSettingsContext.Instance.GameWindowToolsSettings_;
    static JsonTypeInfo<GameWindowToolsSettings_> ISettings<GameWindowToolsSettings_>.JsonTypeInfo =>
        GameWindowToolsSettingsContext.Instance.GameWindowToolsSettings_;

    public List<NativeWindowRule>? Rules { get; set; }
}

internal static class GameWindowToolsSettings
{
    internal static SettingsProperty<NativeWindowRule, List<NativeWindowRule>, GameWindowToolsSettings_> Rules { get; } = new(null);
}
#endif
