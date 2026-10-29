namespace BD.WTTS.UI.ViewModels;

public sealed class ProxySettingsWindowViewModel : WindowViewModel
{
    public static string DisplayName => Strings.CommunityFix_ProxySettings;

    public ProxySettingsWindowViewModel()
    {
        ProxySettings.MigrateDnsServerList();
        Title = DisplayName;
    }


    public IEnumerable<string> SystemProxyIps { get; }
        = new[] {
            "0.0.0.0",
            "127.0.0.1",
        };


    public void ResetSettings()
    {
        lock (SettingsPropertyBase.GetSynchronizationRoot<ProxySettings_>())
        {
            ProxySettings.SystemProxyIp.Reset(save: false);
            ProxySettings.ProxyMasterDns.Reset(save: false);
            ProxySettings.SystemProxyPortId.Reset(save: false);
            ProxySettings.ProgramStartupRunProxy.Reset(save: false);
            ProxySettings.EnableHttpProxyToHttps.Reset(save: false);
            ProxySettings.OnlyEnableProxyScript.Reset(save: false);
            ProxySettings.OnlyEnableProxyScript.Save();
        }

        Toast.Show(ToastIcon.Success, "重置成功");
    }
}