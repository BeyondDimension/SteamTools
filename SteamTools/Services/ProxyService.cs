using Livet;
using SteamTool.Proxy;
using SteamTool.Proxy.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SteamTool.Model;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTool.Core;

namespace SteamTools.Services
{
    public class ProxyService : NotificationObject
    {
        public static ProxyService Current { get; } = new ProxyService();
        private readonly HostsService hostsService = new HostsService();
        public HttpProxy Proxy { get; set; }

        public void Initialize()
        {
            //启动时恢复host
            hostsService.RemoveHostsByTag();
            if (ProxySettings.SupportProxyServicesStatus.Value.Count > 0)
            {
                foreach (var item in ProxyDomains.Value)
                {
                    if (ProxySettings.SupportProxyServicesStatus.Value.TryGetValue(item.Index, out bool value))
                    {
                        item.IsEnable = value;
                    }
                }
            }
            Proxy = new HttpProxy(ProxyDomains.Value, ProductInfo.Product);
            InitJsScript();
            Proxy.IsEnableScript = IsEnableScript;
            if (ProxySettings.ProgramStartupRunProxy.Value)
            {
                ProxyStatus = true;
            }
        }

        private Lazy<IReadOnlyCollection<ProxyDomainModel>> _ProxyDomains = new Lazy<IReadOnlyCollection<ProxyDomainModel>>(() => new List<ProxyDomainModel>
{
new ProxyDomainModel{
Index=0,
Name=Resources.SteamCommunity,
Domains = new List<string>{"steamcommunity.com" },
ToDomain = "steamcommunity-a.akamaihd.net",
//ToDomain = "steampowered.com",
Hosts = new List<string>{ "steamcommunity.com", "www.steamcommunity.com"},
DomainTag = DomainTag.SteamCommunity,
IsEnable= true,
},
new ProxyDomainModel{
Index=1,
Name=Resources.SteamStore,
Domains = new List<string>{"steampowered.com" },
//ToDomain = "steamstore-a.akamaihd.net",
ToDomain = "media.steampowered.com",
Hosts = new List<string>{ "store.steampowered.com", "api.steampowered.com"},
DomainTag = DomainTag.SteamStore,
IsEnable= false,
},
new ProxyDomainModel{
Index=2,
Name=Resources.SteamImage,
Domains = new List<string>{"steamcdn-a.akamaihd.net" },
ToDomain = "cdn.akamai.steamstatic.com",
//ToDomain = "media.steampowered.com",
Hosts = new List<string>{ "steamcdn-a.akamaihd.net"},
DomainTag = DomainTag.SteamImage,
IsEnable= false,
},
new ProxyDomainModel{
Index=3,
Name=Resources.SteamChat,
Domains = new List<string>{"steam-chat.com" },
//ToDomain = "steamstore-a.akamaihd.net",
ToDomain = "media.steampowered.com",
Hosts = new List<string>{ "steam-chat.com"},
DomainTag = DomainTag.SteamChat,
IsEnable= false,
},
new ProxyDomainModel{
Index=4,
Name=Resources.Discord,
Domains = new List<string>{ "discordapp.com"},
//ToDomain = "discord.com",
ProxyIPAddres="162.159.129.233",
Hosts = new List<string>{
"discordapp.com",
"support.discordapp.com",
"url9177.discordapp.com",
"canary-api.discordapp.com",
"cdn-ptb.discordapp.com",
"ptb.discordapp.com",
"status.discordapp.com",
"cdn-canary.discordapp.com",
"cdn.discordapp.com",
"streamkit.discordapp.com",
"i18n.discordapp.com" ,
"url9624.discordapp.com",
"url7195.discordapp.com",
"merch.discordapp.com",
"printer.discordapp.com",
"canary.discordapp.com",
"apps.discordapp.com",
"pax.discordapp.com",
},
//ServerName= "discord.com",
DomainTag = DomainTag.Discord,
IsEnable= false,
},
new ProxyDomainModel{
Index=11,
Name=Resources.DiscordNet,
Domains = new List<string>{ "discordapp.net" },
ProxyIPAddres="162.159.130.232",
Hosts = new List<string>{
"dl.discordapp.net",
"media.discordapp.net",
"images-ext-2.discordapp.net",
"images-ext-1.discordapp.net",
},
DomainTag = DomainTag.DiscordNet,
IsEnable= false,
},
new ProxyDomainModel{
Index=5,
Name=Resources.Twitch,
Domains = new List<string>{ "twitch.tv" ,"usher.ttvnw.net"},
ToDomain="twitch.map.fastly.net",
Hosts = new List<string>{
"twitch.tv",
"www.twitch.tv",
"m.twitch.tv",
"app.twitch.tv",
"music.twitch.tv",
"badges.twitch.tv",
"blog.twitch.tv",
"inspector.twitch.tv",
"stream.twitch.tv",
"dev.twitch.tv",
"clips.twitch.tv",
"spade.twitch.tv",
"gql.twitch.tv",
"vod-secure.twitch.tv",
"vod-storyboards.twitch.tv",
"trowel.twitch.tv",
"countess.twitch.tv",
"extension-files.twitch.tv",
"vod-metro.twitch.tv",
"pubster.twitch.tv",
"help.twitch.tv",
"passport.twitch.tv",
"id.twitch.tv",
"link.twitch.tv",
"id-cdn.twitch.tv",
"player.twitch.tv",
"api.twitch.tv",
"cvp.twitch.tv",
"clips-media-assets2.twitch.tv",
"client-event-reporter.twitch.tv",
"gds-vhs-drops-campaign-images.twitch.tv",
"us-west-2.uploads-regional.twitch.tv",
"assets.help.twitch.tv",
"discuss.dev.twitch.tv",
"pubsub-edge.twitch.tv",
"irc-ws.chat.twitch.tv",
"irc-ws-r.chat.twitch.tv",
//"platform.twitter.com",
"usher.ttvnw.net",
},
DomainTag = DomainTag.Twitch,
IsEnable= false,
},
new ProxyDomainModel{
Index=6,
Name=Resources.OriginDownload,
Domains = new List<string>{"origin-a.akamaihd.net" },
ToDomain = "cctv4-lh.akamaihd.net",
Hosts = new List<string>{ "origin-a.akamaihd.net"},
DomainTag = DomainTag.OriginGameDownload,
IsEnable= false,
},
//new ProxyDomainModel{
//Name=Resources.UplayUpdate,
//Domains = new List<string>{"static3.cdn.ubi.com" },
//ToDomain = "static3.cdn.ubi.com",
//Hosts = new List<string>{ "static3.cdn.ubi.com"},
//DomainTag = DomainTag.UplayUpdate,
//IsEnable= false,
//},
//new ProxyDomainModel{
//Index=7,
//Name=Resources.GOG,
//Domains = new List<string>{ "gog.com" },
//ToDomain = "api.gog.com",
//Hosts = new List<string>{
//"www.gog.com",
//"images.gog.com",
//"images-1.gog.com",
//"images-2.gog.com",
//"images-3.gog.com",
//"images-4.gog.com",
//"webinstallers.gog.com",
//"menu.gog.com",
//"auth.gog.com",
//"login.gog.com",
////"api.gog.com",
//"reviews.gog.com",
//"insights-collector.gog.com",
//"remote-config.gog.com",
//"external-accounts.gog.com",
//"chat.gog.com",
//"presence.gog.com",
//"external-users.gog.com",
//"gamesdb.gog.com",
//"gameplay.gog.com",
//"cfg.gog.com",
//"notifications.gog.com",
//"users.gog.com",
//},
//DomainTag = DomainTag.GOG,
//IsEnable= false,
//},
new ProxyDomainModel{
Index=8,
Name=Resources.GoogleRecaptchaCode,
Domains = new List<string>{"www.google.com" },
ToDomain = "kh.google.com",
Hosts = new List<string>{ "www.google.com"},
DomainTag = DomainTag.GoogleCode,
IsEnable= false,
},
new ProxyDomainModel{
Index=9,
Name=Resources.GithubContent,
Domains = new List<string>{"githubusercontent.com","raw.github.com" },
//ToDomain = "aw.githubusercontent.com",
ProxyIPAddres = "151.101.88.133",
Hosts = new List<string>{
"raw.github.com",
"raw.githubusercontent.com",
"camo.githubusercontent.com",
"cloud.githubusercontent.com",
"avatars0.githubusercontent.com",
"avatars1.githubusercontent.com",
"avatars2.githubusercontent.com",
"avatars3.githubusercontent.com",
"user-images.githubusercontent.com",
},
DomainTag = DomainTag.GithubContent,
IsEnable= false,
},
new ProxyDomainModel{
Index=10,
Name=Resources.Pixiv,
Domains = new List<string>{ "pixiv.net" },
ToDomain="fanbox.cc",
//ProxyIPAddres = "210.140.131.219",
Hosts = new List<string>{
"www.pixiv.net",
"touch.pixiv.net",
"source.pixiv.net",
"accounts.pixiv.net",
"imgaz.pixiv.net",
"app-api.pixiv.net",
"oauth.secure.pixiv.net",
"dic.pixiv.net",
"comic.pixiv.net",
"factory.pixiv.net",
"g-client-proxy.pixiv.net",
"sketch.pixiv.net",
"payment.pixiv.net",
"sensei.pixiv.net",
"novel.pixiv.net",
"ssl.pixiv.net",
"times.pixiv.net",
"recruit.pixiv.net",
"pixiv.net",
"p2.pixiv.net",
"matsuri.pixiv.net",
"m.pixiv.net",
"iracon.pixiv.net",
"inside.pixiv.net",
"i1.pixiv.net",
"help.pixiv.net",
"goods.pixiv.net",
"genepixiv.pr.pixiv.net",
"festa.pixiv.net",
"en.dic.pixiv.net",
"dev.pixiv.net",
"chat.pixiv.net",
"blog.pixiv.net",
"embed.pixiv.net",
"comic-api.pixiv.net",
"pay.pixiv.net",
},
DomainTag = DomainTag.GithubContent,
IsEnable= false,
},
});
        public Lazy<IReadOnlyCollection<ProxyDomainModel>> ProxyDomains
        {
            get => _ProxyDomains;
            set
            {
                if (_ProxyDomains != value)
                {
                    _ProxyDomains = value;
                    Proxy.ProxyDomains = value.Value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool IsEnableScript
        {
            get => ProxySettings.IsEnableScript.Value;
            set
            {
                if (ProxySettings.IsEnableScript.Value != value)
                {
                    ProxySettings.IsEnableScript.Value = value;
                    Proxy.IsEnableScript = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #region 代理状态变更通知
        public bool ProxyStatus
        {
            get { return Proxy.ProxyRunning; }
            set
            {
                if (value != Proxy.ProxyRunning)
                {
                    if (value)
                    {
                        var isRun = Proxy.StartProxy(ProxySettings.IsProxyGOG.Value);
                        if (isRun)
                        {
                            StatusService.Current.Notify(SteamTools.Properties.Resources.ProxyRun);
                        }
                        else
                        {
                            //WindowService.Current.ShowDialogWindow("启动加速服务失败，请检查443端口是否被占用或者证书安装失败。");
                            StatusService.Current.Notify("启动加速服务失败，请检查443端口是否被占用或者证书安装失败。");
                        }
                    }
                    else
                    {
                        Proxy.StopProxy();
                        StatusService.Current.Notify(SteamTools.Properties.Resources.ProxyStop);
                    }
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        private List<ProxyScript> _ProxyScripts;
        public List<ProxyScript> ProxyScripts
        {
            get => _ProxyScripts;
            set
            {
                if (_ProxyScripts != value)
                {
                    _ProxyScripts = value;
                    Proxy.Scripts = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// 加载js脚本列表
        /// </summary>
        public void InitJsScript()
        {
            var scripts = new List<ProxyScript>();
            var dir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, Const.SCRIPT_DIR));
            if (!dir.Exists)
            {
                dir.Create();
                return;
            }
            foreach (var file in dir.GetFiles())
            {
                if (file.Extension.Equals(".js", StringComparison.OrdinalIgnoreCase))
                {
                    if (ProxyScript.TryParse(file.FullName, out var script))
                        scripts.Add(script);
                    else
                        scripts.Add(new ProxyScript { FilePath = file.FullName, Name = Resources.Not_Support_JS });
                }
            }
            ProxyScripts = scripts;
        }

        public void Shutdown()
        {
            Current.Proxy?.Dispose();
        }
    }
}
