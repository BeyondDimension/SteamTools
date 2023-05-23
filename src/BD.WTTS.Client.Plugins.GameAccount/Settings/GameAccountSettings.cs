using static BD.WTTS.Settings.Abstractions.IGameAccountSettings;

namespace BD.WTTS.Settings;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class GameAccountSettings_ : IGameAccountSettings, ISettings
{
    public const string Name = nameof(GameAccountSettings);

    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public ConcurrentDictionary<long, string?>? AccountRemarks { get; set; } = new();

    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public IReadOnlyCollection<DisableAuthorizedDevice>? DisableAuthorizedDevice { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(GameAccountSettings_))]
internal partial class GameAccountSettingsContext : JsonSerializerContext
{
    static GameAccountSettingsContext? instance;

    public static GameAccountSettingsContext Instance
        => instance ??= new GameAccountSettingsContext(ISettings.GetDefaultOptions());
}

partial class GameAccountSettings_ : ISettings<GameAccountSettings_>
{
    public static JsonTypeInfo<GameAccountSettings_> JsonTypeInfo => GameAccountSettingsContext.Instance.GameAccountSettings_;

    public static JsonSerializerContext JsonSerializerContext => GameAccountSettingsContext.Instance;

    static string ISettings.Name => Name;

    static JsonTypeInfo ISettings.JsonTypeInfo => GameAccountSettingsContext.Instance.GameAccountSettings_;
}

[SettingsGeneration]
public static class GameAccountSettings
{
    /// <inheritdoc cref="IGameAccountSettings.AccountRemarks"/>
    public static SettingsProperty<KeyValuePair<long, string?>, ConcurrentDictionary<long, string?>, GameAccountSettings_> AccountRemarks { get; } = new(null, false);

    /// <inheritdoc cref="IGameAccountSettings.DisableAuthorizedDevice"/>
    public static SettingsProperty<IReadOnlyCollection<DisableAuthorizedDevice>, GameAccountSettings_> DisableAuthorizedDevice { get; } = new(DefaultDisableAuthorizedDevice);
}