using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.Generic;

namespace System.Application.UI.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.CommunityFix;
            protected set { throw new NotImplementedException(); }
        }

        //private IList<MenuItemViewModel> _MenuItems = new[]
        //{
        //        new MenuItemViewModel
        //        {
        //            Header = AppResources.More,
        //            Items = new[]
        //    {
        //        new MenuItemViewModel { Header = "_登录新账号",IconKey="SteamDrawing" },
        //        new MenuItemViewModel { Header = "编辑" },
        //        new MenuItemViewModel { Header = "-" },
        //        new MenuItemViewModel
        //        {
        //            Header = "Recent",
        //            Items = new[]
        //            {
        //                new MenuItemViewModel
        //                {
        //                    Header = "File1.txt",
        //                },
        //                new MenuItemViewModel
        //                {
        //                    Header = "File2.txt",
        //                },
        //            }
        //        },
        //    }
        //        },
        //};


        private IReadOnlyCollection<ProxyDomain> _ProxyDomains = new List<ProxyDomain> {
      new ProxyDomain{
Name="SteamCommunity",
Domains = new List<string>{"steamcommunity.com" },
ToDomain = "steamcommunity.rmbgame.net",
Hosts = new List<string>{ "steamcommunity.com", "www.steamcommunity.com"},
DomainTag = DomainTag.SteamCommunity,
IsEnable= true,
},
new ProxyDomain{
Name="SteamStore",
Domains = new List<string>{"store.steampowered.com","api.steampowered.com" },
ToDomain = "steamstore.rmbgame.net",
Hosts = new List<string>{ "store.steampowered.com", "api.steampowered.com"},
DomainTag = DomainTag.SteamStore,
IsEnable= false,
},
new ProxyDomain{
Name="Steam Update",
Domains = new List<string>{"media.steampowered.com" },
ToDomain = "steammedia.rmbgame.net",
Hosts = new List<string>{ "media.steampowered.com",},
DomainTag = DomainTag.SteamMeidia,
IsEnable= false,
},
      new ProxyDomain{
Name="SteamCommunity",
Domains = new List<string>{"steamcommunity.com" },
ToDomain = "steamcommunity.rmbgame.net",
Hosts = new List<string>{ "steamcommunity.com", "www.steamcommunity.com"},
DomainTag = DomainTag.SteamCommunity,
IsEnable= true,
},
new ProxyDomain{
Name="SteamStore",
Domains = new List<string>{"store.steampowered.com","api.steampowered.com" },
ToDomain = "steamstore.rmbgame.net",
Hosts = new List<string>{ "store.steampowered.com", "api.steampowered.com"},
DomainTag = DomainTag.SteamStore,
IsEnable= false,
},
new ProxyDomain{
Name="Steam Update",
Domains = new List<string>{"media.steampowered.com" },
ToDomain = "steammedia.rmbgame.net",
Hosts = new List<string>{ "media.steampowered.com",},
DomainTag = DomainTag.SteamMeidia,
IsEnable= false,
},
      new ProxyDomain{
Name="SteamCommunity",
Domains = new List<string>{"steamcommunity.com" },
ToDomain = "steamcommunity.rmbgame.net",
Hosts = new List<string>{ "steamcommunity.com", "www.steamcommunity.com"},
DomainTag = DomainTag.SteamCommunity,
IsEnable= true,
},
new ProxyDomain{
Name="SteamStore",
Domains = new List<string>{"store.steampowered.com","api.steampowered.com" },
ToDomain = "steamstore.rmbgame.net",
Hosts = new List<string>{ "store.steampowered.com", "api.steampowered.com"},
DomainTag = DomainTag.SteamStore,
IsEnable= false,
},
new ProxyDomain{
Name="Steam Update",
Domains = new List<string>{"media.steampowered.com" },
ToDomain = "steammedia.rmbgame.net",
Hosts = new List<string>{ "media.steampowered.com",},
DomainTag = DomainTag.SteamMeidia,
IsEnable= false,
},
        };
        public IReadOnlyCollection<ProxyDomain> ProxyDomains
        {
            get => _ProxyDomains;
            set
            {
                if (_ProxyDomains != value)
                {
                    _ProxyDomains = value;
                    this.RaisePropertyChanged();
                }
            }
        }




    }
}
