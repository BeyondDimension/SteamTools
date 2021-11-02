using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Properties;
using System.Application.Services;
using System.Application.UI;

namespace System.Application.Settings
{
    public sealed class GeneralSettings : SettingsHost2<GeneralSettings>
    {
        static GeneralSettings()
        {

        }

        /// <summary>
        /// 自动检查更新
        /// </summary>
        public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 下载更新渠道
        /// </summary>
        public static SerializableProperty<UpdateChannelType> UpdateChannel { get; }
            = GetProperty(defaultValue: (UpdateChannelType)default, autoSave: true);

        /// <summary>
        /// 程序是否开机自启动
        /// </summary>
        public static SerializableProperty<bool> WindowsStartupAutoRun { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        ///// <summary>
        ///// 创建桌面快捷方式
        ///// </summary>
        //public static SerializableProperty<bool> CreateDesktopShortcut { get; }
        //    = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

        /// <summary>
        /// 程序启动时最小化
        /// </summary>
        public static SerializableProperty<bool> IsStartupAppMinimized { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 是否显示起始页
        /// </summary>
        public static SerializableProperty<bool> IsShowStartPage { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 启用游戏列表本地缓存
        /// </summary>
        public static SerializableProperty<bool> IsSteamAppListLocalCache { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 启用错误日志记录
        /// </summary>
        public static SerializableProperty<bool> IsEnableLogRecord { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 用户设置的文本阅读器提供商，根据平台值不同，值格式为 枚举字符串 或 程序路径
        /// </summary>
        public static SerializableProperty<IReadOnlyDictionary<Platform, string>> TextReaderProvider { get; }
            = GetProperty(defaultValue: (IReadOnlyDictionary<Platform, string>?)null, autoSave: true);

        /// <summary>
        /// 使用硬件加速
        /// </summary>
        public static SerializableProperty<bool> UseGPURendering { get; }
            = GetProperty(defaultValue: !OperatingSystem2.IsWindows7, autoSave: true);

        /// <summary>
        /// Hosts 文件编码类型
        /// </summary>
        public static SerializableProperty<IHostsFileService.EncodingType> HostsEncodingType { get; }
            = GetProperty(defaultValue: default(IHostsFileService.EncodingType), autoSave: true);

        ///// <summary>
        ///// 使用 Direct2D1 渲染(仅 Windows)
        ///// </summary>
        //public static SerializableProperty<bool> UseDirect2D1 { get; }
        //    = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// (仅 Windows)Avalonia would try to use native Widows OpenGL when set to true. The default value is false.
        /// </summary>
        public static SerializableProperty<bool> UseWgl { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 屏幕捕获(允许截图)
        /// </summary>
        public static SerializableProperty<bool> CaptureScreen { get; }
            = GetProperty(defaultValue: false, autoSave: true);
    }
}
