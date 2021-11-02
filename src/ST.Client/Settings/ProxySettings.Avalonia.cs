using System;
using System.Application.Columns;
using System.Application.Services;
using System.Collections.Generic;
using IPAddress = System.Net.IPAddress;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Settings
{
    partial class ProxySettings
    {
        readonly static SerializableProperty<bool>? _IsProxyGOG
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 启用GOG插件代理
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsProxyGOG => _IsProxyGOG ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _EnableWindowsProxy
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 启用系统代理模式
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> EnableWindowsProxy => _EnableWindowsProxy ?? throw new PlatformNotSupportedException();

        readonly static SerializableProperty<bool>? _IsOnlyWorkSteamBrowser
             = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 是否只针对Steam内置浏览器启用脚本
        /// </summary>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsOnlyWorkSteamBrowser => _IsOnlyWorkSteamBrowser ?? throw new PlatformNotSupportedException();
    }
}
