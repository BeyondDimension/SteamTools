using DynamicData;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI;
using System.Application.UI.ViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Settings
{
    public sealed partial class UISettings : SettingsHost2<UISettings>
    {
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
        public static SerializableProperty<HashSet<MessageBox.DontPromptType>?> DoNotShowMessageBoxs { get; }
            = GetProperty<HashSet<MessageBox.DontPromptType>?>(defaultValue: null, autoSave: false);
    }
}

//static void EnableDesktopBackground_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
//{
//    if (e.NewValue)
//    {
//        IApplication.Instance.SetDesktopBackgroundWindow();
//    }
//    else
//    {
//        INativeWindowApiService.Instance.ResetWallerpaper();
//    }
//}

//static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
//{
//    // 当前 Avalonia App 主题切换存在问题
//    //if (OperatingSystem2.Application.UseAvalonia) return;
//    if (e.NewValue != e.OldValue)
//    {
//        var value = (AppTheme)e.NewValue;
//        if (value.IsDefined())
//        {
//            IApplication.Instance.Theme = value;
//        }
//    }
//}