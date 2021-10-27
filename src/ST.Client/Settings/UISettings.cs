using System;
using System.Application.Models;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Application.Services;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using DynamicData;

namespace System.Application.Settings
{
    public sealed class UISettings : SettingsHost2<UISettings>
    {
        static UISettings()
        {
            Theme.ValueChanged += Theme_ValueChanged;
            AppGridSize.ValueChanged += AppGridSize_ValueChanged;

            if (OperatingSystem2.IsWindows)
            {
                EnableDesktopBackground.ValueChanged += EnableDesktopBackground_ValueChanged;
            }

            WindowBackgroundMateria.ValueChanged += WindowBackgroundMateria_ValueChanged;

            if (WindowViewModel.IsSupportedSizePosition)
            {
                _WindowSizePositions = GetProperty(defaultValue: new ConcurrentDictionary<string, SizePosition>(), autoSave: false);
            }
        }

        static void WindowBackgroundMateria_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            IApplication.Instance.SetAllWindowransparencyMateria(e.NewValue);
        }

        static void EnableDesktopBackground_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
        {
            if (e.NewValue)
            {
                IApplication.Instance.SetDesktopBackgroundWindow();
            }
            else
            {
                INativeWindowApiService.Instance.ResetWallerpaper();
            }
        }

        static void AppGridSize_ValueChanged(object? sender, ValueChangedEventArgs<int> e)
        {
            SteamConnectService.Current.SteamApps.Refresh();
        }

        static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
        {
            // 当前 Avalonia App 主题切换存在问题
            //if (OperatingSystem2.Application.UseAvalonia) return;
            if (e.NewValue != e.OldValue)
            {
                var value = (AppTheme)e.NewValue;
                if (value.IsDefined())
                {
                    IApplication.Instance.Theme = value;
                }
            }
        }



        /// <summary>
        /// 主题
        /// </summary>
        public static SerializableProperty<short> Theme { get; }
            = GetProperty(defaultValue: (short)0, autoSave: true);

        /// <summary>
        /// 语言
        /// </summary>
        public static SerializableProperty<string> Language { get; }
            = GetProperty(defaultValue: string.Empty, autoSave: true);

        /// <summary>
        /// 不再提示的消息框数组
        /// </summary>
        public static SerializableProperty<List<MessageBox.RememberChoose>> DoNotShowMessageBoxs { get; }
            = GetProperty(defaultValue: new List<MessageBox.RememberChoose>(), autoSave: false);

        static readonly SerializableProperty<ConcurrentDictionary<string, SizePosition>>? _WindowSizePositions;
        /// <summary>
        /// 所有窗口位置记忆字典集合
        /// </summary>
        public static SerializableProperty<ConcurrentDictionary<string, SizePosition>> WindowSizePositions => _WindowSizePositions ?? throw new PlatformNotSupportedException();

        /// <summary>
        /// 字体
        /// </summary>
        public static SerializableProperty<string> FontName { get; }
            = GetProperty(defaultValue: "Default", autoSave: true);

        /// <summary>
        /// 窗口背景透明度
        /// </summary>
        public static SerializableProperty<double> AcrylicOpacity { get; }
            = GetProperty(defaultValue: .8, autoSave: true);

        /// <summary>
        /// 窗口背景材质
        /// </summary>
        public static SerializableProperty<int> WindowBackgroundMateria { get; }
            = GetProperty(defaultValue: OperatingSystem2.IsWindows11AtLeast ? 4 : 3, autoSave: true);

        /// <summary>
        /// 库存游戏封面大小
        /// </summary>
        public static SerializableProperty<int> AppGridSize { get; }
            = GetProperty(defaultValue: 150, autoSave: true);

        /// <summary>
        /// Steam账号备注替换名称显示
        /// </summary>
        public static SerializableProperty<bool> SteamAccountRemarkReplaceName { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用圆角界面
        /// </summary>
        public static SerializableProperty<bool> EnableFilletUI { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用动态桌面背景
        /// </summary>
        public static SerializableProperty<bool> EnableDesktopBackground { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 启用自定义背景图片
        /// </summary>
        public static SerializableProperty<bool> EnableCustomBackgroundImage { get; }
            = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 背景图片路径
        /// </summary>
        public static SerializableProperty<string> BackgroundImagePath { get; }
            = GetProperty(defaultValue: "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/AppResources/Placeholders/0.png", autoSave: true);

        /// <summary>
        /// 主题颜色(十六进制字符串)
        /// </summary>
        public static SerializableProperty<string> ThemeAccent { get; }
            = GetProperty(defaultValue: "#FF0078D7", autoSave: true);

        /// <summary>
        /// 主题颜色从系统获取
        /// </summary>
        public static SerializableProperty<bool> GetUserThemeAccent { get; }
            = GetProperty(defaultValue: true, autoSave: true);

        /// <summary>
        /// 主菜单展开状态
        /// </summary>
        public static SerializableProperty<bool> MainMenuExpandedState { get; }
            = GetProperty(defaultValue: false, autoSave: true);
    }
}
