using System.Application.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;

namespace System.Application.Models.Settings
{
    public static class ProxySettings
    {
        /// <summary>
        /// 启用脚本自动检查更新
        /// </summary>
        public static SerializableProperty<bool> IsAutoCheckScriptUpdate { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, true) { AutoSave = true };
        /// <summary>
        /// 启用GOG插件代理
        /// </summary>
        public static SerializableProperty<bool> IsProxyGOG { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 启用windows系统代理模式
        /// </summary>
        public static SerializableProperty<bool> EnableWindowsProxy { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 启用代理脚本
        /// </summary>
        public static SerializableProperty<bool> IsEnableScript { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 程序启动时自动启动代理
        /// </summary>
        public static SerializableProperty<bool> ProgramStartupRunProxy { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 是否只针对Steam内置浏览器启用脚本
        /// </summary>
        public static SerializableProperty<bool> IsOnlyWorkSteamBrowser { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 代理服务启用状态
        /// </summary>
        public static SerializableProperty<IReadOnlyCollection<string>> SupportProxyServicesStatus { get; }
            = new SerializableProperty<IReadOnlyCollection<string>>(GetKey(), Providers.Local, new List<string>());

        /// <summary>
        /// 系统代理模式端口
        /// </summary>
        public static SerializableProperty<int> SystemProxyPortId { get; }
            = new SerializableProperty<int>(GetKey(), Providers.Local, 26501);

        /// <summary>
        /// 系统代理模式IP
        /// </summary>
        public static SerializableProperty<string> SystemProxyIp { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, IPAddress.Loopback.ToString());

        /// <summary>
        /// 脚本启用状态
        /// </summary>
        public static SerializableProperty<IReadOnlyCollection<int>> ScriptsStatus { get; }
            = new SerializableProperty<IReadOnlyCollection<int>>(GetKey(), Providers.Local, new List<int>()) { AutoSave = true };

        
        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(ProxySettings) + "." + propertyName;
        }
    }
}
