#if (DEBUG && !UI_DEMO) || (!DEBUG && UI_DEMO)
using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    partial class MockCloudServiceClient
    {
        public Task<IApiResponse<List<ScriptDTO>>> Scripts()
        {
            var list = new List<ScriptDTO>
            {
                new ScriptDTO
                {
                    Name = "GM",
                    Version = "0.1",
                    Author = "软妹币玩家",
                    Description = "基础脚本框架(不建议取消勾选，会导致某些脚本无法运行)",
                },
            };
            return Task.FromResult(ApiResponse.Ok(list));
        }

        public Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All()
        {
            var list = new List<AccelerateProjectGroupDTO>
            {
                new AccelerateProjectGroupDTO
                {
                    Name = "Steam 服务",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("bd30bffd-0c1c-450f-870a-a3143bf4b6fa"),
                            Name = "Steam社区",
                            DomainNames = "steamcommunity.com",
                            ForwardDomainName = "steamcommunity.rmbgame.net",
                            Hosts = "steamcommunity.com;www.steamcommunity.com",
                            Enable  = true,
                        },
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("bb6cf2ee-32a1-451f-acaf-9d39945c4a75"),
                            Name = "Steam商店",
                            DomainNames="store.steampowered.com;api.steampowered.com",
                            ForwardDomainName = "steamstore.rmbgame.net",
                            Hosts = "store.steampowered.com;api.steampowered.com",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("93ed4df8-19d2-464d-9f2e-2a583fd21f3f"),
                            Name = "Steam更新",
                            DomainNames="media.steampowered.com",
                            ForwardDomainName="steammedia.rmbgame.net",
                            Hosts = "media.steampowered.com",
                            Enable = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Steam图片",
                            DomainNames="steamcdn-a.akamaihd.net;steamuserimages-a.akamaihd.net;cdn.akamai.steamstatic.com",
                            ForwardDomainName="steamimage.rmbgame.net",
                            Hosts = "steamcdn-a.akamaihd.net;steamuserimages-a.akamaihd.net;cdn.akamai.steamstatic.com",
                            Enable = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Steam图片上传",
                            DomainNames="steamcloud-ugc-hkg.oss-cn-hongkong.aliyuncs.com",
                            ForwardDomainName="steamcloud-ugc.rmbgame.net",
                            //ForwardDomainIP = "47.97.233.33",
                            Hosts = "steamcloud-ugc-hkg.oss-cn-hongkong.aliyuncs.com",
                            Enable = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Steam好友聊天",
                            DomainNames="steam-chat.com",
                            ForwardDomainName="steamchat.rmbgame.net",
                            Hosts = "steam-chat.com",
                            Enable = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Discord 语音聊天",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("3e5ec009-a69d-4344-ac97-84afb3a7e657"),
                            Name = "Discord 语音",
                            DomainNames = "discordapp.com",
                            //ForwardDomainName = "discord.rmbgame.net",
                            ForwardDomainIP = "162.159.130.233",
                            Hosts = "discordapp.com;support.discordapp.com;url9177.discordapp.com;canary-api.discordapp.com;cdn-ptb.discordapp.com;ptb.discordapp.com;status.discordapp.com;cdn-canary.discordapp.com;cdn.discordapp.com;streamkit.discordapp.com;i18n.discordapp.com;url9624.discordapp.com;url7195.discordapp.com;merch.discordapp.com;printer.discordapp.com;canary.discordapp.com;apps.discordapp.com;pax.discordapp.com;",
                            //104.20.96.100 dl.discordapp.net;104.20.96.100 media.discordapp.net;104.20.96.100 images-ext-2.discordapp.net;104.20.96.100 images-ext-1.discordapp.net
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("434ff726-03e7-4430-ab96-de6b8eb52520"),
                            Name = "Discord 语音",
                            DomainNames = "discord.com",
                            ForwardDomainName = "discordcom.rmbgame.net",
                            Hosts = "discord.com",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Id=Guid.Parse("88aecefd-24e4-4407-a02e-9cd48b7c6462"),
                            Name = "Discord 图片加载",
                            DomainNames = "discordapp.net",
                            ForwardDomainName = "discordnet.rmbgame.net",
                            //ForwardDomainIP = "162.159.128.232",
                            Hosts = "dl.discordapp.net;media.discordapp.net;images-ext-2.discordapp.net;images-ext-1.discordapp.net;images-2.discordapp.net;images-1.discordapp.net",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Twitch 直播",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 直播聊天",
                            DomainNames = "irc-ws.chat.twitch.tv",
                            ForwardDomainName = "twitchchat.rmbgame.net",
                            Hosts = "irc-ws.chat.twitch.tv;irc-ws-r.chat.twitch.tv",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 登录",
                            DomainNames = "passport.twitch.tv",
                            //ForwardDomainName = "twitchchat.rmbgame.net",
                            ForwardDomainIP = "54.213.193.53",
                            Hosts = "passport.twitch.tv",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 第三方关联登录",
                            DomainNames = "id.twitch.tv",
                            //ForwardDomainName = "twitchchat.rmbgame.net",
                            ForwardDomainIP = "34.213.92.235",
                            Hosts = "id.twitch.tv",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 直播掉宝服务(1)",
                            DomainNames = "countess.twitch.tv",
                            //ForwardDomainName = "twitchchat.rmbgame.net",
                            ForwardDomainIP = "52.32.137.233",
                            Hosts = "countess.twitch.tv",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 直播掉宝服务(2)",
                            DomainNames = "video-edge-d6c541.pdx01.abs.hls.ttvnw.net",
                            //ForwardDomainName = "twitchchat.rmbgame.net",
                            ForwardDomainIP = "54.186.211.17",
                            Hosts = "video-edge-d6c541.pdx01.abs.hls.ttvnw.net",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 直播掉宝服务(3)",
                            DomainNames = "pubsub-edge.twitch.tv",
                            //ForwardDomainName = "twitchchat.rmbgame.net",
                            ForwardDomainIP = "54.186.146.179",
                            Hosts = "pubsub-edge.twitch.tv",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Twitch 直播网页加载",
                            DomainNames = "twitch.tv",
                            ForwardDomainName = "twitch.rmbgame.net",
                            Hosts = "twitch.tv;www.twitch.tv;m.twitch.tv;app.twitch.tv;music.twitch.tv;badges.twitch.tv;blog.twitch.tv;inspector.twitch.tv;stream.twitch.tv;dev.twitch.tv;clips.twitch.tv;spade.twitch.tv;gql.twitch.tv;vod-secure.twitch.tv;vod-storyboards.twitch.tv;trowel.twitch.tv;countess.twitch.tv;extension-files.twitch.tv;vod-metro.twitch.tv;pubster.twitch.tv;help.twitch.tv;link.twitch.tv;id-cdn.twitch.tv;player.twitch.tv;api.twitch.tv;cvp.twitch.tv;clips-media-assets2.twitch.tv;client-event-reporter.twitch.tv;gds-vhs-drops-campaign-images.twitch.tv;us-west-2.uploads-regional.twitch.tv;assets.help.twitch.tv;discuss.dev.twitch.tv",
                            //"platform.twitter.com",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Origin",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Origin 游戏下载(akamai)",
                            DomainNames = "origin-a.akamaihd.net",
                            ForwardDomainName = "originakamaidownload.rmbgame.net",
                            Hosts = "origin-a.akamaihd.net",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Uplay",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Uplay 更新(akamai)",
                            DomainNames = "static3.cdn.ubi.com",
                            ForwardDomainName = "ubisoftstatic3.rmbgame.net",
                            Hosts = "static3.cdn.ubi.com",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "GOG",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "GOG 游戏平台",
                            DomainNames = "gog.com",
                            ForwardDomainName = "gog.rmbgame.net",
                            Hosts = "gog.com;www.gog.com;images.gog.com;images-1.gog.com;images-2.gog.com;images-3.gog.com;images-4.gog.com;webinstallers.gog.com;menu.gog.com;auth.gog.com;login.gog.com;api.gog.com;reviews.gog.com;insights-collector.gog.com;remote-config.gog.com;external-accounts.gog.com;chat.gog.com;presence.gog.com;external-users.gog.com;gamesdb.gog.com;gameplay.gog.com;cfg.gog.com;notifications.gog.com;users.gog.com",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Google 验证码",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Google(Recaptcha)验证码",
                            DomainNames = "www.google.com",
                            ForwardDomainName = "www.recaptcha.net",
                            Hosts = "www.google.com",
                            Enable  = false,
                            Redirect=true,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "Github",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Github 图片Raw加载",
                            DomainNames = "githubusercontent.com;raw.github.com",
                            ForwardDomainName = "githubusercontent.rmbgame.net",
                            Hosts = "raw.github.com;githubusercontent.com;raw.githubusercontent.com;camo.githubusercontent.com;cloud.githubusercontent.com;avatars.githubusercontent.com;avatars0.githubusercontent.com;avatars1.githubusercontent.com;avatars2.githubusercontent.com;avatars3.githubusercontent.com;user-images.githubusercontent.com",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Github Gist",
                            DomainNames = "gist.github.com",
                            ForwardDomainName = "github.rmbgame.net",
                            Hosts = "gist.github.com",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Github 网站",
                            DomainNames = "github.com",
                            ForwardDomainName = "github.rmbgame.net",
                            Hosts = "github.com",
                            Enable  = false,
                        },
                    },
                },
                new AccelerateProjectGroupDTO
                {
                    Name = "图片站点合集",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Pixiv 网站",
                            DomainNames = "pixiv.net",
                            ForwardDomainName = "pixiv.rmbgame.net",
                            Hosts = "www.pixiv.net;touch.pixiv.net;source.pixiv.net;accounts.pixiv.net;imgaz.pixiv.net;app-api.pixiv.net;oauth.secure.pixiv.net;dic.pixiv.net;comic.pixiv.net;factory.pixiv.net;g-client-proxy.pixiv.net;sketch.pixiv.net;payment.pixiv.net;sensei.pixiv.net;novel.pixiv.net;ssl.pixiv.net;times.pixiv.net;recruit.pixiv.net;pixiv.net;p2.pixiv.net;matsuri.pixiv.net;m.pixiv.net;iracon.pixiv.net;inside.pixiv.net;i1.pixiv.net;help.pixiv.net;goods.pixiv.net;genepixiv.pr.pixiv.net;festa.pixiv.net;en.dic.pixiv.net;dev.pixiv.net;chat.pixiv.net;blog.pixiv.net;embed.pixiv.net;comic-api.pixiv.net;pay.pixiv.net",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Pixiv 图片加载",
                            DomainNames = "pximg.net",
                            ForwardDomainName = "pximg.rmbgame.net",
                            Hosts = "pximg.net;i.pximg.net;s.pximg.net;img-sketch.pximg.net;source.pximg.net;booth.pximg.net;i-f.pximg.net;imp.pximg.net;public-img-comic.pximg.net",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Pinterest",
                            DomainNames = "pinterest.com;pinimg.com",
                            ForwardDomainName = "pinterest.rmbgame.net",
                            Hosts = "pinterest.com;www.pinterest.com;pinimg.com;sm.pinimg.com;s.pinimg.com;i.pinimg.com",
                            Enable  = false,
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Artstation",
                            DomainNames = "artstation.com",
                            ForwardDomainIP = "104.16.226.51",
                            Hosts = "artstation.com",
                            Enable  = false,
                        },
                    },
                },
            };
            return Task.FromResult(ApiResponse.Ok(list));
        }
    }
}
#endif