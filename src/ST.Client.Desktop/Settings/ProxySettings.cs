using System.Application.Services;
using System.Collections.Generic;
using System.Net;

namespace System.Application.Settings
{
    public sealed class ProxySettings : Abstractions.ProxySettings<ProxySettings>
    {
        /// <summary>
        /// 启用GOG插件代理
        /// </summary>
        public static SerializableProperty<bool> IsProxyGOG { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用系统代理模式
        /// </summary>
        public static SerializableProperty<bool> EnableWindowsProxy { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 是否只针对Steam内置浏览器启用脚本
        /// </summary>
        public static SerializableProperty<bool> IsOnlyWorkSteamBrowser { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        #region 代理设置

        /// <summary>
        /// Host代理模式端口
        /// </summary>
        public static SerializableProperty<int> HostProxyPortId { get; }
            = GetProperty(defaultValue: 443, autoSave: false);

        #endregion
    }
}