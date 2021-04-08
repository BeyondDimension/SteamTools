using System;
using System.Application.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Application.Models.Settings
{
    public static class GeneralSettings
    {
        static GeneralSettings()
        {
            WindowsStartupAutoRun.ValueChanged += WindowsStartupAutoRun_ValueChanged;
            IsEnableLogRecord.ValueChanged += IsEnableLogRecord_ValueChanged;
            //CreateDesktopShortcut.ValueChanged += CreateDesktopShortcut_ValueChanged;
        }

        private static void IsEnableLogRecord_ValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {

        }

        private static void WindowsStartupAutoRun_ValueChanged(object sender, ValueChangedEventArgs<bool> e)
        {

        }

        /// <summary>
        /// 程序是否开机自启动
        /// </summary>
        public static SerializableProperty<bool> WindowsStartupAutoRun { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        ///// <summary>
        ///// 创建桌面快捷方式
        ///// </summary>
        //public static SerializableProperty<bool> CreateDesktopShortcut { get; }
        //    = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

        /// <summary>
        /// 程序启动时最小化
        /// </summary>
        public static SerializableProperty<bool> IsStartupAppMinimized { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 是否显示起始页
        /// </summary>
        public static SerializableProperty<bool> IsShowStartPage { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, true) { AutoSave = true };

        /// <summary>
        /// 启用游戏列表本地缓存
        /// </summary>
        public static SerializableProperty<bool> IsSteamAppListLocalCache { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, true) { AutoSave = true };

        /// <summary>
        /// 自动检查更新
        /// </summary>
        public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, true) { AutoSave = true };

        /// <summary>
        /// 启用错误日志记录
        /// </summary>
        public static SerializableProperty<bool> IsEnableLogRecord { get; }
            = new SerializableProperty<bool>(GetKey(), Providers.Local, false) { AutoSave = true };

        /// <summary>
        /// 用户设置的文本阅读器提供商，根据平台值不同，值格式为 枚举字符串 或 程序路径
        /// </summary>
        public static SerializableProperty<Dictionary<Platform, string>?> TextReaderProvider { get; }
                = new SerializableProperty<Dictionary<Platform, string>?>(GetKey(), Providers.Local, null) { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GeneralSettings) + "." + propertyName;
        }
    }
}
