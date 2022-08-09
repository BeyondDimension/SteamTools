using System.Collections.Generic;

namespace System.Application.Settings;

public sealed partial class UISettings : SettingsHost2<UISettings>
{
    /// <summary>
    /// 主题
    /// </summary>
    public static SerializableProperty<short> Theme { get; }
        = GetProperty(defaultValue: (short)0);

    /// <summary>
    /// 语言
    /// </summary>
    public static SerializableProperty<string> Language { get; }
        = GetProperty(defaultValue: string.Empty);

    /// <summary>
    /// 不再提示的消息框数组
    /// </summary>
    public static SerializableProperty<HashSet<MessageBox.DontPromptType>?> DoNotShowMessageBoxs { get; }
        = GetProperty<HashSet<MessageBox.DontPromptType>?>(defaultValue: null, autoSave: false);

    /// <summary>
    /// 是否显示广告
    /// </summary>
    public static SerializableProperty<bool> IsShowAdvertise { get; }
        = GetProperty(defaultValue: true);
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
//    //if (OperatingSystem2.Application.UseAvalonia()) return;
//    if (e.NewValue != e.OldValue)
//    {
//        var value = (AppTheme)e.NewValue;
//        if (value.IsDefined())
//        {
//            IApplication.Instance.Theme = value;
//        }
//    }
//}