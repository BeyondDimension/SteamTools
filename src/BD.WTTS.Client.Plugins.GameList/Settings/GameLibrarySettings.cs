namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class GameLibrarySettings_ : IGameLibrarySettings, ISettings
{
    public const string Name = nameof(GameLibrarySettings);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public bool GameInstalledFilter { get; set; } = false;

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public bool GameCloudArchiveFilter { get; set; } = false;

    [MPKey(2), MP2Key(2), JsonPropertyOrder(2)]
    public List<SteamAppType> GameTypeFiltres { get; set; } = new List<SteamAppType>
    { SteamAppType.Game, SteamAppType.Application, SteamAppType.Demo, SteamAppType.Beta };

    [MPKey(3), MP2Key(3), JsonPropertyOrder(3)]
    public Dictionary<uint, string?> HideGameList { get; set; } = new Dictionary<uint, string?>();

    [MPKey(4), MP2Key(4), JsonPropertyOrder(4)]
    public Dictionary<uint, string?>? AFKAppList { get; set; } = null;

    [MPKey(5), MP2Key(5), JsonPropertyOrder(5)]
    public bool IsAutoAFKApps { get; set; } = true;
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(GameLibrarySettings_))]
internal partial class GameLibrarySettingsContext : JsonSerializerContext
{
    static GameLibrarySettingsContext? instance;

    public static GameLibrarySettingsContext Instance
        => instance ??= new GameLibrarySettingsContext(ISettings.GetDefaultOptions());
}

partial class GameLibrarySettings_ : ISettings<GameLibrarySettings_>
{
    public static JsonTypeInfo<GameLibrarySettings_> JsonTypeInfo => GameLibrarySettingsContext.Instance.GameLibrarySettings_;

    public static JsonSerializerContext JsonSerializerContext => GameLibrarySettingsContext.Instance;

    static string ISettings.Name => Name;

    static JsonTypeInfo ISettings.JsonTypeInfo => GameLibrarySettingsContext.Instance.GameLibrarySettings_;
}

[SettingsGeneration]
public static class GameLibrarySettings
{
    /// <inheritdoc cref="IGameLibrarySettings.GameInstalledFilter"/>
    public static SettingsProperty<bool, GameLibrarySettings_> GameInstalledFilter { get; } = new();

    /// <inheritdoc cref="IGameLibrarySettings.GameCloudArchiveFilter"/>
    public static SettingsProperty<bool, GameLibrarySettings_> GameCloudArchiveFilter { get; } = new();

    /// <inheritdoc cref="IGameLibrarySettings.GameTypeFiltres"/>
    public static SettingsProperty<List<SteamAppType>, GameLibrarySettings_> GameTypeFiltres { get; } = new();

    /// <inheritdoc cref="IGameLibrarySettings.HideGameList"/>
    public static SettingsProperty<Dictionary<uint, string?>, GameLibrarySettings_> HideGameList { get; } = new();

    /// <inheritdoc cref="IGameLibrarySettings.AFKAppList"/>
    public static SettingsProperty<Dictionary<uint, string?>?, GameLibrarySettings_> AFKAppList { get; } = new();

    /// <inheritdoc cref="IGameLibrarySettings.IsAutoAFKApps"/>
    public static SettingsProperty<bool, GameLibrarySettings_> IsAutoAFKApps { get; } = new();
}
