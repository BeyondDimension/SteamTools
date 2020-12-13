using MetroTrilithon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public static class ProxySettings
    {
        /// <summary>
        /// 启用GOG代理
        /// </summary>
        public static SerializableProperty<bool> IsProxyGOG { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false);

        /// <summary>
        /// 启用代理脚本
        /// </summary>
        public static SerializableProperty<bool> IsEnableScript { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false);

        /// <summary>
        /// 程序启动时自动启动代理
        /// </summary>
        public static SerializableProperty<bool> ProgramStartupRunProxy { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false);

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GeneralSettings) + "." + propertyName;
        }
    }
}
