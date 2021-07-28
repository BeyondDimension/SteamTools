using System.Application.Serialization;
using System.Collections.Generic;
using System.Net;

namespace System.Application.Models.Settings
{
    public sealed class ProxySettings : SettingsHost2<ProxySettings>
    {
        /// <summary>
        /// 启用脚本自动检查更新
        /// </summary>
        public static SerializableProperty<bool> IsAutoCheckScriptUpdate { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 启用GOG插件代理
        /// </summary>
        public static SerializableProperty<bool> IsProxyGOG { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用windows系统代理模式
        /// </summary>
        public static SerializableProperty<bool> EnableWindowsProxy { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用代理脚本
        /// </summary>
        public static SerializableProperty<bool> IsEnableScript { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 程序启动时自动启动代理
        /// </summary>
        public static SerializableProperty<bool> ProgramStartupRunProxy { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 是否只针对Steam内置浏览器启用脚本
        /// </summary>
        public static SerializableProperty<bool> IsOnlyWorkSteamBrowser { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 代理服务启用状态
        /// </summary>
        public static SerializableProperty<IReadOnlyCollection<string>> SupportProxyServicesStatus { get; }
            = GetProperty(defaultValue: (IReadOnlyCollection<string>)new List<string>(), autoSave: false);

        /// <summary>
        /// 系统代理模式端口
        /// </summary>
        public static SerializableProperty<int> SystemProxyPortId { get; }
            = GetProperty(defaultValue: 26501, autoSave: false);

        /// <summary>
        /// 系统代理模式IP
        /// </summary>
        public static SerializableProperty<string> SystemProxyIp { get; }
            = GetProperty(defaultValue: IPAddress.Loopback.ToString(), autoSave: false);

        /// <summary>
        /// 脚本启用状态
        /// </summary>
        public static SerializableProperty<IReadOnlyCollection<int>> ScriptsStatus { get; }
            = GetProperty(defaultValue: (IReadOnlyCollection<int>)new List<int>(), autoSave: true);
    }
}