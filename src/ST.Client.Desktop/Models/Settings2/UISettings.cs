using DynamicData;
using System.Application.Serialization;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;

namespace System.Application.Models.Settings
{
    public sealed class UISettings : SettingsHost2<UISettings>
    {
        static UISettings()
        {
            //Theme.ValueChanged += Theme_ValueChanged;
            AppGridSize.ValueChanged += AppGridSize_ValueChanged;
        }

        private static void AppGridSize_ValueChanged(object? sender, ValueChangedEventArgs<int> e)
        {
            SteamConnectService.Current.SteamApps.Refresh();
        }

        //private static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
        //{
        //    if (e.NewValue != e.OldValue)
        //    {
        //        var value = (AppTheme)e.NewValue;
        //        if (value.IsDefined())
        //        {
        //            AppHelper.Current.Theme = value;
        //        }
        //    }
        //}

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

#if !__MOBILE__
        /// <summary>
        /// 字体
        /// </summary>
        public static SerializableProperty<string> FontName { get; }
            = GetProperty(defaultValue: "Default", autoSave: true);

        /// <summary>
        /// 背景图片路径
        /// </summary>
        public static SerializableProperty<string> BackgroundImagePath { get; }
            = GetProperty(defaultValue: string.Empty, autoSave: true);

        /// <summary>
        /// 主题选择
        /// </summary>
        public static SerializableProperty<int> ThemeAccent { get; }
            = GetProperty(defaultValue: 0, autoSave: true);

        /// <summary>
        /// 窗口毛玻璃背景透明度
        /// </summary>
        public static SerializableProperty<double> AcrylicOpacity { get; }
            = GetProperty(defaultValue: .8, autoSave: true);

        /// <summary>
        /// 库存游戏封面大小
        /// </summary>
        public static SerializableProperty<int> AppGridSize { get; }
            = GetProperty(defaultValue: 150, autoSave: true);

        /// <summary>
        /// 所有窗口位置记忆集合
        /// </summary>
        public static SerializableProperty<Dictionary<string, WindowSizePosition>> WindowSizePositions { get; }
            = GetProperty(defaultValue: new Dictionary<string, WindowSizePosition>(), autoSave: false);
#endif
    }
}