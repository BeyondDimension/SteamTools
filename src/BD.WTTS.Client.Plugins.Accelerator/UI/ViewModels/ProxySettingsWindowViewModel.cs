using static BD.WTTS.Services.IDnsAnalysisService;

namespace BD.WTTS.UI.ViewModels;

public sealed class ProxySettingsWindowViewModel : WindowViewModel
{
    public static string DisplayName => Strings.CommunityFix_ProxySettings;

    public ProxySettingsWindowViewModel()
    {
        Title = DisplayName;
    }

    //public IEnumerable<ExternalProxyType> ProxyTypes { get; }
    //    = Enum2.GetAll<ExternalProxyType>();

    public static IEnumerable<string> ProxyDNSs { get; } = new[]
    {
        "System Default",
        PrimaryDNS_114,
        PrimaryDNS_Ali,
        PrimaryDNS_Dnspod,
        PrimaryDNS_Baidu,
        PrimaryDNS_Google,
        PrimaryDNS_Cloudflare,
    };

    public IEnumerable<string> SystemProxyIps { get; }
        = new[] {
            "0.0.0.0",
            "127.0.0.1",
        };

    public static IEnumerable<string> DohAddress { get; }
        = new[] {
            Dnspod_DohAddres,
            Dnspod_DohAddres2,
            Dnspod_DohAddres3,
            DNS_Ali_DohAddres,
            DNS_Ali_DohAddres2,
            DNS_Ali_DohAddres3,
            Google_DohAddres,
            Cloudflare_DohAddres,
            DohAddres_360,
            TUNA_DohAddres,
        };

    public void ResetSettings()
    {
        // 更改多个设置项不立即保存
        ProxySettings.SystemProxyIp.Reset(save: false);
        ProxySettings.ProxyMasterDns.Reset(save: false);
        ProxySettings.SystemProxyPortId.Reset(save: false);
        ProxySettings.ProgramStartupRunProxy.Reset(save: false);
        ProxySettings.EnableHttpProxyToHttps.Reset(save: false);
        ProxySettings.UseDoh.Reset(save: false);
        ProxySettings.CustomDohAddres2.Reset(save: false);
        ProxySettings.OnlyEnableProxyScript.Reset(save: false);

        // 更改完成后保存一次
        ProxySettings.OnlyEnableProxyScript.Save();

        Toast.Show(ToastIcon.Success, "重置成功");
    }
}