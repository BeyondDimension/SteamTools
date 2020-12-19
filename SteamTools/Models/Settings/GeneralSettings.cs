using MetroTrilithon.Serialization;
using SteamTool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public static class GeneralSettings
    {
        static GeneralSettings()
        {
            WindowsStartupAutoRun.ValueChanged += WindowsStartupAutoRun_ValueChanged;
        }

        private static void WindowsStartupAutoRun_ValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            var steamService = SteamToolCore.Instance.Get<SteamToolService>();
            steamService.SetWindowsStartupAutoRun(e.NewValue, ProductInfo.Title);
        }

        /// <summary>
        /// 多语言设置
        /// </summary>
        public static SerializableProperty<string> Culture { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Roaming) { AutoSave = true };

        /// <summary>
        /// 程序是否开机自启动
        /// </summary>
        public static SerializableProperty<bool> WindowsStartupAutoRun { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

        /// <summary>
        /// 程序启动时最小化
        /// </summary>
        public static SerializableProperty<bool> IsStartupAppMinimized { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

        /// <summary>
        /// 是否显示起始页
        /// </summary>
        public static SerializableProperty<bool> IsShowStartPage { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Roaming, true) { AutoSave = true };

        /// <summary>
        /// 游戏列表本地缓存
        /// </summary>
        public static SerializableProperty<bool> IsSteamAppListLocalCache { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Roaming, true) { AutoSave = true };

        /// <summary>
        /// Steam启动参数
        /// </summary>
        public static SerializableProperty<string> SteamStratParameter { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Roaming, string.Empty) { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GeneralSettings) + "." + propertyName;
        }
    }
}
