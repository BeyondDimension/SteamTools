namespace BD.WTTS.Settings;

public static partial class ProxySettings
{
    public static SettingsStructProperty<ScriptInjectScopeMode, ProxySettings_> ScriptInjectScopeMode { get; }
        = new(Enums.ScriptInjectScopeMode.All);

    public static SettingsStructProperty<ScriptUserAgentPreset, ProxySettings_> ScriptUserAgentPresets { get; }
        = new(ScriptUserAgentPreset.None);

    public static SettingsProperty<string, ProxySettings_> ScriptCustomUserAgents { get; }
        = new(null);

    public static SettingsStructProperty<bool, ProxySettings_> ScriptScopeMigrated { get; }
        = new(false);

    public static void MigrateScriptScope()
    {
        lock (Abstractions.SettingsPropertyBase.GetSynchronizationRoot<ProxySettings_>())
        {
            if (ScriptScopeMigrated.Value)
                return;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            if (IsOnlyWorkSteamBrowser.Value)
            {
                ScriptUserAgentPresets.Value = ScriptUserAgentPreset.SteamBrowser;
                ScriptInjectScopeMode.Value = Enums.ScriptInjectScopeMode.SelectedUserAgents;
            }
#endif

            ScriptScopeMigrated.Value = true;
        }
    }
}
