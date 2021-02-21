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
                    if (ProxySettings.SupportProxyServicesStatus.Value.TryGetValue((int)item.DomainTag, out bool value))
                    {
                        item.IsEnable = value;
                    }
                }
            }
            Proxy = new HttpProxy(ProxyDomains.Value, ProductInfo.Product)
            {
                IsEnableScript = IsEnableScript,
                IsOnlyWorkSteamBrowser = IsOnlyWorkSteamBrowser
            };
            InitJsScript();
            if (ProxySettings.ProgramStartupRunProxy.Value)
            {
                ProxyStatus = true;
            }
        }

        private Lazy<IReadOnlyCollection<ProxyDomainModel>> _ProxyDomains = new Lazy<IReadOnlyCollection<ProxyDomainModel>>(() => new List<ProxyDomainModel>
{
new ProxyDomainModel{
Name=Resources.SteamCommunity,
Domains = new List<string>{"steamcommunity.com" },
ToDomain = "steamcommunity.rmbgame.net",
Hosts = new List<string>{ "steamcommunity.com", "www.steamcommunity.com"},
DomainTag = DomainTag.SteamCommunity,
IsEnable= true,
},
new ProxyDomainModel{
Name=Resources.SteamStore,
Domains = new List<string>{"steampowered.com" },
ToDomain = "steamstore.rmbgame.net",
Hosts = new List<string>{ "store.steampowered.com", "api.steampowered.com"},
DomainTag = DomainTag.SteamStore,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.SteamImage,
Domains = new List<string>{"steamcdn-a.akamaihd.net","steamuserimages-a.akamaihd.net" },
//ToDomain = "cdn.akamai.steamstatic.com",
ToDomain = "steamimage.rmbgame.net",
Hosts = new List<string>{ "steamcdn-a.akamaihd.net","steamuserimages-a.akamaihd.net"},
DomainTag = DomainTag.SteamImage,
IsEnable= false,
},
//new ProxyDomainModel{
//Name=Resources.SteamImageUpload,
//Domains = new List<string>{"steamcloud-ugc-hkg.oss-cn-hongkong.aliyuncs.com"},
////ToDomain = "steamcloud-ugc.rmbgame.net",
//ProxyIPAddres = "47.97.233.33",
//Hosts = new List<string>{ "steamcloud-ugc-hkg.oss-cn-hongkong.aliyuncs.com"},
//DomainTag = DomainTag.SteamImage,
//IsEnable= false,
//},
new ProxyDomainModel{
Name=Resources.SteamChat,
Domains = new List<string>{"steam-chat.com" },
ToDomain = "steamchat.rmbgame.net",
Hosts = new List<string>{ "steam-chat.com"},
DomainTag = DomainTag.SteamChat,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.Discord,
Domains = new List<string>{ "discordapp.com"},
ToDomain = "discord.rmbgame.net",
//ProxyIPAddres="162.159.130.233",
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
"1.0.0.1 dl.discordapp.net",
"1.0.0.1 media.discordapp.net",
"1.0.0.1 images-ext-2.discordapp.net",
"1.0.0.1 images-ext-1.discordapp.net",
},
//ServerName= "discord.com",
DomainTag = DomainTag.Discord,
IsEnable= false,
},
//new ProxyDomainModel{
//Name=Resources.DiscordNet,
//Domains = new List<string>{ "discordapp.net" },
////ProxyIPAddres="162.159.128.232",
//ToDomain = "discordnet.rmbgame.net",
//Hosts = new List<string>{
//"1.0.0.1 dl.discordapp.net",
//"media.discordapp.net",
//"images-ext-2.discordapp.net",
//"images-ext-1.discordapp.net",
//},
//DomainTag = DomainTag.DiscordNet,
//IsEnable= false,
//},
new ProxyDomainModel{
Name=Resources.TwitchChat,
Domains = new List<string>{ "irc-ws.chat.twitch.tv" },
//ProxyIPAddres="52.38.70.182",
ToDomain = "twitchchat.rmbgame.net",
Hosts = new List<string>{
"irc-ws.chat.twitch.tv",
"irc-ws-r.chat.twitch.tv",
},
DomainTag = DomainTag.TwitchChat,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.Twitch,
Domains = new List<string>{ "twitch.tv" },
ToDomain="twitch.rmbgame.net",
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
},
DomainTag = DomainTag.Twitch,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.OriginDownload,
Domains = new List<string>{"origin-a.akamaihd.net" },
ToDomain = "originakamaidownload.rmbgame.net",
Hosts = new List<string>{ "origin-a.akamaihd.net"},
DomainTag = DomainTag.OriginGameDownload,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.UplayUpdate,
Domains = new List<string>{"static3.cdn.ubi.com" },
//ToDomain = "ubisoftstatic3.rmbgame.net",
ProxyIPAddres = "116.211.99.137",
Hosts = new List<string>{ "static3.cdn.ubi.com"},
DomainTag = DomainTag.UplayUpdate,
IsEnable= false,
},
//new ProxyDomainModel{
//Name=Resources.GOG,
//Domains = new List<string>{ "gog.com" },
//ToDomain = "gog.rmbgame.net",
//Hosts = new List<string>{
//"gog.com",
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
//"api.gog.com",
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
Name=Resources.GoogleRecaptchaCode,
Domains = new List<string>{"www.google.com" },
ToDomain = "www.recaptcha.net",
//ToDomain = "googlerecaptchacode.rmbgame.net",
IsRedirect = true,
Hosts = new List<string>{ "www.google.com"},
DomainTag = DomainTag.GoogleCode,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.GithubContent,
Domains = new List<string>{"githubusercontent.com","raw.github.com"},
ToDomain = "githubusercontent.rmbgame.net",
//ProxyIPAddres = "151.101.88.133",
Hosts = new List<string>{
"raw.github.com",
"githubusercontent.com",
"raw.githubusercontent.com",
"camo.githubusercontent.com",
"cloud.githubusercontent.com",
"avatars.githubusercontent.com",
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
Name=Resources.GithubGist,
Domains = new List<string>{"gist.github.com" },
ToDomain = "github.com",
Hosts = new List<string>{
"gist.github.com",
},
DomainTag = DomainTag.GithubGist,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.Pixiv,
Domains = new List<string>{ "pixiv.net" },
ToDomain="pixiv.rmbgame.net",
//ProxyIPAddres = "210.140.131.182",
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
DomainTag = DomainTag.Pixiv,
IsEnable= false,
},
new ProxyDomainModel{
Name=Resources.PixivImgae,
Domains = new List<string>{ "pximg.net" },
ToDomain="pximg.rmbgame.net",
Hosts = new List<string>{
"pximg.net",
"i.pximg.net",
"s.pximg.net",
"img-sketch.pximg.net",
"source.pximg.net",
"booth.pximg.net",
"i-f.pximg.net",
"imp.pximg.net",
"public-img-comic.pximg.net",
},
DomainTag = DomainTag.PixivImage,
IsEnable = false,
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
        public bool IsOnlyWorkSteamBrowser
        {
            get => ProxySettings.IsOnlyWorkSteamBrowser.Value;
            set
            {
                if (ProxySettings.IsOnlyWorkSteamBrowser.Value != value)
                {
                    ProxySettings.IsOnlyWorkSteamBrowser.Value = value;
                    Proxy.IsOnlyWorkSteamBrowser = value;
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
                            StatusService.Current.Notify("启动加速服务失败，请检查443端口是否被占用或者证书安装失败。\n\n如果安装了vmware虚拟机，可能会导致443端口被占用。");
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
