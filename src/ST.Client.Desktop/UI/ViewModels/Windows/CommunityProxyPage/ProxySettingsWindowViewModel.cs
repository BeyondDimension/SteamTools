using ArchiSteamFarm.Steam;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using Titanium.Web.Proxy.Models;

namespace System.Application.UI.ViewModels
{
    public class ProxySettingsWindowViewModel : WindowViewModel
    {
        public ProxySettingsWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.CommunityFix_ProxySettings;
        }

        public IEnumerable<ExternalProxyType> ProxyTypes { get; set; } = Enum.GetValues<ExternalProxyType>();
    }
}