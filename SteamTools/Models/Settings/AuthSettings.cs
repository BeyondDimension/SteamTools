using MetroTrilithon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public static class AuthSettings
    {
        /// <summary>
        /// 令牌数据(压缩存储）
        /// </summary>
        public static SerializableProperty<string> Authenticators { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local) { AutoSave = true };

        /// <summary>
        /// 是否将令牌数据存储在程序路径下
        /// </summary>
        public static SerializableProperty<bool> IsCurrentDirectorySaveAuthData { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Roaming) { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(AuthSettings) + "." + propertyName;
        }
    }
}
