#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

using Compat = BD.WTTS.Settings.GameLibrarySettings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

/// <summary>
/// Steam 账号设置
/// </summary>
public sealed class SteamAccountSettings : SettingsHost2<SteamAccountSettings>
{
    static readonly SerializableProperty<ConcurrentDictionary<long, string?>?> _AccountRemarks =
        Compat.GetProperty<ConcurrentDictionary<long, string?>?>(defaultValue: null, autoSave: false);

    /// <summary>
    /// Steam 账号备注字典
    /// </summary>
    public static SerializableProperty<ConcurrentDictionary<long, string?>?> AccountRemarks => _AccountRemarks;

    // ----- AccountRemarks ClassName 之前用的 GameLibrarySettings 需要兼容已发行的版本，将错就错，之后新增的还是使用 GetProperty 而不是 Compat.GetProperty -----

    /// <summary>
    /// Steam 家庭共享临时禁用
    /// </summary>
    public static SerializableProperty<IReadOnlyCollection<DisableAuthorizedDevice>> DisableAuthorizedDevice => GetProperty(defaultValue: (IReadOnlyCollection<DisableAuthorizedDevice>)Array.Empty<DisableAuthorizedDevice>());
}

sealed class GameLibrarySettings : SettingsHost2<Compat>
{

}

#endif