using BD.WTTS.Client.Resources;
using BD.WTTS.Enums;

namespace BD.WTTS.UI.ViewModels;

public sealed class ProxySettingsWindowViewModel : WindowViewModel
{
    public static string DisplayName => Strings.CommunityFix_ProxySettings;

    public ProxySettingsWindowViewModel()
    {
        Title = DisplayName;
    }

    public IEnumerable<ExternalProxyType> ProxyTypes { get; }
        = Enum2.GetAll<ExternalProxyType>();

    public IEnumerable<string> ProxyDNSs { get; } = new[]
    {
            "System Default",
            IDnsAnalysisService.PrimaryDNS_114,
            IDnsAnalysisService.PrimaryDNS_Ali,
            IDnsAnalysisService.PrimaryDNS_Dnspod,
            IDnsAnalysisService.PrimaryDNS_Baidu,
            IDnsAnalysisService.PrimaryDNS_Google,
            IDnsAnalysisService.PrimaryDNS_Cloudflare,
    };

    public IEnumerable<string> SystemProxyIps { get; }
        = new[] { "0.0.0.0", "127.0.0.1" };
}
