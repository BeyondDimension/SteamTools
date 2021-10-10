using System;
using System.Application.Models;
using System.Application.UI.ViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Settings
{
    internal sealed class UISettings : Abstractions.UISettings<UISettings>
    {

    }
}

namespace System.Application.Settings.Abstractions
{
    public abstract class UISettings<TSettings> : SettingsHost2<TSettings> where TSettings : UISettings<TSettings>, new()
    {
        static UISettings()
        {
            if (WindowViewModel.IsSupportedSizePosition)
            {
                _WindowSizePositions = GetProperty(defaultValue: new ConcurrentDictionary<string, SizePosition>(), autoSave: false);
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
    }
}
