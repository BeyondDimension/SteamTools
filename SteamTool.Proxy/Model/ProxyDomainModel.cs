using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;

namespace SteamTool.Proxy
{
    public class ProxyDomainModel
    {
        /// <summary>
        /// 显示在UI上的名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 代理域名合集
        /// </summary>
        public List<string> Domains { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 443;

        /// <summary>
        /// 转发域名
        /// </summary>
        public string ToDomain { get; set; }

        /// <summary>
        /// 转发域名IP
        /// </summary>
        public string ProxyIPAddres { get; set; }

        /// <summary>
        /// 伪装的ServerName
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 启用该域名代理
        /// </summary>
        public bool IsEnable { get; set; }


        public IEnumerable<string> Hosts { get; set; }

        public DomainTag DomainTag { get; set; }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DomainTag : byte
    {
        /// <summary>
        /// steam社区
        /// </summary>
        [Description("steam社区")]
        SteamCommunity = 0,
        /// <summary>
        /// steam商店
        /// </summary>
        [Description("steam商店")]
        SteamStore = 1,
        [Description("steam部分图片修复")]
        SteamImage = 2,
        [Description("steam好友服务")]
        SteamChat = 3,
        [Description("Discord语音")]
        Discord = 4,
        [Description("Twitch直播")]
        Twitch = 5,
        [Description("Origin下载加速(Akamai)")]
        OriginGameDownload = 6,
        [Description("Uplay更新防劫持")]
        UplayUpdate = 7,
        [Description("Google(Recaptcha)")]
        GoogleCode = 8,
    }
}
