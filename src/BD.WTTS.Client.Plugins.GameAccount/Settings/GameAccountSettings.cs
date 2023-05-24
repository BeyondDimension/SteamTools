#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1634 // File header should show copyright
// <console-tools-generated/>
#pragma warning restore SA1634 // File header should show copyright
#pragma warning restore IDE0079 // 请删除不必要的忽略
using static BD.WTTS.Settings.Abstractions.IGameAccountSettings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(GameAccountSettings_))]
internal partial class GameAccountSettingsContext : JsonSerializerContext
{
    static GameAccountSettingsContext? instance;

    public static GameAccountSettingsContext Instance
        => instance ??= new GameAccountSettingsContext(ISettings.GetDefaultOptions());
}

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class GameAccountSettings_ : IGameAccountSettings, ISettings, ISettings<GameAccountSettings_>
{
    public const string Name = nameof(GameAccountSettings);

    static string ISettings.Name => Name;

    static JsonSerializerContext ISettings.JsonSerializerContext
        => GameAccountSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo
        => GameAccountSettingsContext.Instance.GameAccountSettings_;

    static JsonTypeInfo<GameAccountSettings_> ISettings<GameAccountSettings_>.JsonTypeInfo
        => GameAccountSettingsContext.Instance.GameAccountSettings_;

    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    [MPKey(0), MP2Key(0), JsonPropertyOrder(0)]
    public ConcurrentDictionary<long, string?>? AccountRemarks { get; set; }

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    [MPKey(1), MP2Key(1), JsonPropertyOrder(1)]
    public IReadOnlyCollection<DisableAuthorizedDevice>? DisableAuthorizedDevice { get; set; }

}
public static class GameAccountSettings
{
    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    public static SettingsProperty<KeyValuePair<long, string?>, ConcurrentDictionary<long, string?>, GameAccountSettings_> AccountRemarks { get; }
        = new(DefaultAccountRemarks);

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    public static SettingsProperty<IReadOnlyCollection<DisableAuthorizedDevice>, GameAccountSettings_> DisableAuthorizedDevice { get; }
        = new(DefaultDisableAuthorizedDevice);

}
