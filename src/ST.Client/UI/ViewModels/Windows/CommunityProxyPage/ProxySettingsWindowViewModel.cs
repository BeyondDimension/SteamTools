using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
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
            IDnsAnalysisService.PrimaryDNS_114,
            IDnsAnalysisService.PrimaryDNS_Ali,
            IDnsAnalysisService.PrimaryDNS_Dnspod,
            IDnsAnalysisService.PrimaryDNS_Baidu,
            IDnsAnalysisService.PrimaryDNS_Google,
            IDnsAnalysisService.PrimaryDNS_Cloudflare,
        };
    }
}
