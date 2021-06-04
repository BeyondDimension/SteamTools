using System;
using System.Application.Serialization;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Application.Models.Settings
{
    public static class UISettings
    {
        static UISettings()
        {
            //Theme.ValueChanged += Theme_ValueChanged;
        }

        private static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
        {
            if (e.NewValue != e.OldValue)
            {
                var value = (AppTheme)e.NewValue;
                if (value.IsDefined())
                {
                    AppHelper.Current.Theme = value;
                }
            }
        }

        /// <summary>
        /// 主题
        /// </summary>
        public static SerializableProperty<short> Theme { get; }
            = new SerializableProperty<short>(GetKey(), Providers.Local, 0) { AutoSave = true };

        /// <summary>
        /// 语言
        /// </summary>
        public static SerializableProperty<string> Language { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "") { AutoSave = true };

        /// <summary>
        /// 字体
        /// </summary>
        public static SerializableProperty<string> FontName { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "Default") { AutoSave = true };

        /// <summary>
        /// 背景图片路径
        /// </summary>
        public static SerializableProperty<string> BackgroundImagePath { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "") { AutoSave = true };

        /// <summary>
        /// 主题选择
        /// </summary>
        public static SerializableProperty<int> ThemeAccent { get; }
            = new SerializableProperty<int>(GetKey(), Providers.Local, 0) { AutoSave = true };

        /// <summary>
        /// 窗口毛玻璃背景透明度
        /// </summary>
        public static SerializableProperty<double> AcrylicOpacity { get; }
            = new SerializableProperty<double>(GetKey(), Providers.Local, 0.80) { AutoSave = true };

        /// <summary>
        /// 库存游戏封面大小
        /// </summary>
        public static SerializableProperty<int> AppGridSize { get; }
            = new SerializableProperty<int>(GetKey(), Providers.Local, 150) { AutoSave = true };

        /// <summary>
        /// 所有窗口位置记忆集合
        /// </summary>
        public static SerializableProperty<Dictionary<string, WindowSizePosition>> WindowSizePositions { get; }
            = new SerializableProperty<Dictionary<string, WindowSizePosition>>(GetKey(), Providers.Local, new Dictionary<string, WindowSizePosition>()) { AutoSave = false };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(UISettings) + "." + propertyName;
        }
    }
}
