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
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false);


        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GeneralSettings) + "." + propertyName;
        }
    }
}
