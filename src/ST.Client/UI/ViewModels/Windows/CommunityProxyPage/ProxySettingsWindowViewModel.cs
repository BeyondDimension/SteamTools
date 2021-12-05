using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using Titanium.Web.Proxy.Models;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class ProxySettingsWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.CommunityFix_ProxySettings;

        public ProxySettingsWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
        }

        public IEnumerable<ExternalProxyType> ProxyTypes { get; } = Enum2.GetAll<ExternalProxyType>();

        public IEnumerable<string?> ProxyDNSs { get; } = new List<string?>()
        {
            null,
            DnsAnalysis.PrimaryDNS_114,
            DnsAnalysis.PrimaryDNS_Ali,
            DnsAnalysis.PrimaryDNS_Dnspod,
            DnsAnalysis.PrimaryDNS_Baidu,
            DnsAnalysis.PrimaryDNS_Google,
            DnsAnalysis.PrimaryDNS_Cloudflare,
        };
    }
}
