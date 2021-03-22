using System;
using System.Application.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Application.Models.Settings
{
    public static class SteamSettings
    {
        /// <summary>
        /// Steam启动参数
        /// </summary>
        public static SerializableProperty<string> SteamStratParameter { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, string.Empty) { AutoSave = true };

        /// <summary>
        /// Steam皮肤
        /// </summary>
        public static SerializableProperty<string> SteamSkin { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, string.Empty) { AutoSave = true };

        /// <summary>
        /// 自动运行Steam
        /// </summary>
        public static SerializableProperty<bool> IsAutoRunSteam { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 检测到Steam启动时弹出消息通知
        /// </summary>
        public static SerializableProperty<bool> IsEnableSteamLaunchNotification { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(SteamSettings) + "." + propertyName;
        }
    }
}
