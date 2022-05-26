namespace System.Application.Services;

/// <summary>
/// 通知重要性级别
/// <para>Android: https://developer.android.google.cn/training/notify-user/channels#importance </para>
/// </summary>
public enum NotificationImportanceLevel : byte
{
    /// <summary>
    /// 低 没有声音，也没有出现在状态栏中。
    /// </summary>
    Low,

    /// <summary>
    /// 中等 没有声音
    /// </summary>
    Medium,

    /// <summary>
    /// 高 发出声音
    /// </summary>
    High,

    /// <summary>
    /// 紧急 发出声音，并显示为浮动通知
    /// <para>浮动通知: 从 Android 5.0 开始，通知可以在称为浮动通知的浮动窗口中短暂显示。此行为通常针对用户应立即了解的重要通知，并且仅在设备解锁后才会出现。https://developer.android.google.cn/guide/topics/ui/notifiers/notifications?hl=zh-cn#Heads-up </para>
    /// </summary>
    Urgent,
}