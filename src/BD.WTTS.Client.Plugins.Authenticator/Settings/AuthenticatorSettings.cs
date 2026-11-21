namespace BD.WTTS.Settings;

[JsonSerializable(typeof(AuthenticatorSettings_))]
internal partial class AuthenticatorSettingsContext : JsonSerializerContext
{
    internal static AuthenticatorSettingsContext Instance { get; } = new(ISettings.GetDefaultOptions());
}

public sealed class AuthenticatorSettings_ : ISettings<AuthenticatorSettings_>
{
    static string ISettings.Name => nameof(AuthenticatorSettings);

    static JsonSerializerContext ISettings.JsonSerializerContext => AuthenticatorSettingsContext.Instance;

    static JsonTypeInfo ISettings.JsonTypeInfo => AuthenticatorSettingsContext.Instance.AuthenticatorSettings_;

    static JsonTypeInfo<AuthenticatorSettings_> ISettings<AuthenticatorSettings_>.JsonTypeInfo => AuthenticatorSettingsContext.Instance.AuthenticatorSettings_;

    public bool RequireWindowsHello { get; set; }
}

internal static class AuthenticatorSettings
{
    internal static SettingsStructProperty<bool, AuthenticatorSettings_> RequireWindowsHello { get; } = new();
}
