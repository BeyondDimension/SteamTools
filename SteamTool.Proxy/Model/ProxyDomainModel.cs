using System;
using System.Collections.Generic;
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
        /// 代理域名
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 443;

        /// <summary>
        /// 转发域名
        /// </summary>
        public string ProxyDomain { get; set; }

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
        public bool IsEnbale { get; set; }

        public IEnumerable<string> Hosts { get; set; }

        public DomainTag DomainTag { get; set; }
    }

    public enum DomainTag : byte
    {
        /// <summary>
        /// steam社区
        /// </summary>
        SteamCommunity = 0,
        /// <summary>
        /// steam商店
        /// </summary>
        SteamStore = 1,

        SteamImage = 2,

        SteamChat = 3,

        Discord = 4,

        Twitch = 5,

        OriginGameDownload = 6,

        UplayUpdate = 7,

        GoogleCode = 8,
    }
}
