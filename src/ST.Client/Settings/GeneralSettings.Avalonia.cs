using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Properties;
using System.Application.Services;
using System.Application.UI;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif


namespace System.Application.Settings
{
    partial class GeneralSettings
    {
        readonly static SerializableProperty<bool>? _WindowsStartupAutoRun = IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 程序是否开机自启动
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> WindowsStartupAutoRun => _WindowsStartupAutoRun ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsStartupAppMinimized = IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 程序启动时最小化
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool>? IsStartupAppMinimized => _IsStartupAppMinimized ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsSteamAppListLocalCache
            = IsDesktopPlatform ? GetProperty(defaultValue: true, autoSave: true) : null;
        /// <summary>
        /// 启用游戏列表本地缓存
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool>? IsSteamAppListLocalCache => _IsSteamAppListLocalCache ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<IReadOnlyDictionary<Platform, string>>? _TextReaderProvider = IsDesktopPlatform ? GetProperty(defaultValue: (IReadOnlyDictionary<Platform, string>?)null, autoSave: true) : null;
        /// <summary>
        /// 用户设置的文本阅读器提供商，根据平台值不同，值格式为 枚举字符串 或 程序路径
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<IReadOnlyDictionary<Platform, string>> TextReaderProvider => _TextReaderProvider ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<IHostsFileService.EncodingType>? _HostsEncodingType = IsDesktopPlatform ? GetProperty(defaultValue: default(IHostsFileService.EncodingType), autoSave: true) : null;
        /// <summary>
        /// Hosts 文件编码类型
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<IHostsFileService.EncodingType> HostsEncodingType => _HostsEncodingType ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _UseGPURendering = IsDesktopPlatform ? GetProperty(defaultValue: !OperatingSystem2.IsWindows7, autoSave: true) : null;
        /// <summary>
        /// 使用硬件加速
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool>? UseGPURendering => _UseGPURendering ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _UseWgl = OperatingSystem2.IsWindows ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// (仅 Windows)Avalonia would try to use native Widows OpenGL when set to true. The default value is false.
        /// </summary>
        [SupportedOSPlatform("Windows")]
        public static SerializableProperty<bool>? UseWgl => _UseWgl ?? throw new PlatformNotSupportedException();


        ///// <summary>
        ///// 使用 Direct2D1 渲染(仅 Windows)
        ///// </summary>
        //public static SerializableProperty<bool> UseDirect2D1 { get; }
        //    = GetProperty(defaultValue: false, autoSave: true);

        ///// <summary>
        ///// 创建桌面快捷方式
        ///// </summary>
        //public static SerializableProperty<bool> CreateDesktopShortcut { get; }
        //    = new SerializableProperty<bool>(GetKey(), Providers.Roaming, false) { AutoSave = true };

        ///// <summary>
        ///// 是否显示起始页
        ///// </summary>
        //public static SerializableProperty<bool> IsShowStartPage { get; }
        //    = GetProperty(defaultValue: true, autoSave: true);

        ///// <summary>
        ///// 启用错误日志记录
        ///// </summary>
        //public static SerializableProperty<bool> IsEnableLogRecord { get; }
        //    = GetProperty(defaultValue: false, autoSave: true);
    }
}
