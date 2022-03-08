using System;
using System.Application.Models;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Application.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using DynamicData;
using System.Runtime.Versioning;

namespace System.Application.Settings
{
    partial class UISettings
    {
        static readonly SerializableProperty<ConcurrentDictionary<string, SizePosition>?>? _WindowSizePositions = WindowViewModel.IsSupportedSizePosition ?
            GetProperty<ConcurrentDictionary<string, SizePosition>?>(defaultValue: null, autoSave: false) : null;
        /// <summary>
        /// 所有窗口位置记忆字典集合
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<ConcurrentDictionary<string, SizePosition>?> WindowSizePositions => _WindowSizePositions ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<string>? _FontName = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: IFontManager.KEY_Default, autoSave: true) : null;
        /// <summary>
        /// 字体
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> FontName => _FontName ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<double>? _AcrylicOpacity = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: .8, autoSave: true) : null;
        /// <summary>
        /// 窗口背景透明度
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<double> AcrylicOpacity => _AcrylicOpacity ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<int>? _WindowBackgroundMateria = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: OperatingSystem2.IsWindows7 ? 0 : (OperatingSystem2.IsWindows11AtLeast ? 4 : 3), autoSave: true) : null;
        /// <summary>
        /// 窗口背景材质
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<int> WindowBackgroundMateria => _WindowBackgroundMateria ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<int>? _AppGridSize = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: 150, autoSave: true) : null;
        /// <summary>
        /// 库存游戏封面大小
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<int> AppGridSize => _AppGridSize ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _SteamAccountRemarkReplaceName = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// Steam账号备注替换名称显示
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> SteamAccountRemarkReplaceName => _SteamAccountRemarkReplaceName ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _EnableFilletUI = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 启用圆角界面
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> EnableFilletUI => _EnableFilletUI ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _EnableDesktopBackground = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 启用动态桌面背景
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> EnableDesktopBackground => _EnableDesktopBackground ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _EnableCustomBackgroundImage
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 启用自定义背景图片
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> EnableCustomBackgroundImage => _EnableCustomBackgroundImage ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<string>? _BackgroundImagePath = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/AppResources/Placeholders/0.png", autoSave: true) : null;
        /// <summary>
        /// 背景图片路径
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> BackgroundImagePath => _BackgroundImagePath ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<string>? _ThemeAccent = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: "#FF0078D7", autoSave: true) : null;
        /// <summary>
        /// 主题颜色(十六进制字符串)
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<string> ThemeAccent => _ThemeAccent ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _GetUserThemeAccent = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: true, autoSave: true) : null;
        /// <summary>
        /// 主题颜色从系统获取
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> GetUserThemeAccent => _GetUserThemeAccent ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _MainMenuExpandedState = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: false, autoSave: true) : null;
        /// <summary>
        /// 主菜单展开状态
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> MainMenuExpandedState => _MainMenuExpandedState ?? throw new PlatformNotSupportedException();
    }
}
