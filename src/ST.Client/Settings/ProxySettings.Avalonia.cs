using System.Collections.Generic;
using System.Application.UI;
using System.Runtime.Versioning;

namespace System.Application.Settings
{
    partial class ProxySettings
    {
        static readonly SerializableProperty<bool>? _IsProxyGOG
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;

        /// <summary>
        /// 启用GOG插件代理
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsProxyGOG => _IsProxyGOG ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _EnableWindowsProxy
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;

        /// <summary>
        /// 启用系统代理模式
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> EnableWindowsProxy => _EnableWindowsProxy ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _IsOnlyWorkSteamBrowser
             = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;

        /// <summary>
        /// 是否只针对Steam内置浏览器启用脚本
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsOnlyWorkSteamBrowser => _IsOnlyWorkSteamBrowser ?? throw new PlatformNotSupportedException();
    }
}
