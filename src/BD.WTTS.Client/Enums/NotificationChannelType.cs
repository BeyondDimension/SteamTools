using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Enums;

/// <summary>
/// 通知渠道类型(一个渠道可包含一个或多个<see cref="NotificationType"/>)
/// <list type="bullet">
///   <item>
///     <term>iOS：</term>
///   </item>
///   <item>
///     <term>Android：将枚举通过 GetChannelId 转换为 <see cref="string"/> 用作 NotificationCompat.Builder 中的 channelId</term>
///   </item>
///   <item>
///     <term>UWP：</term>
///   </item>
///   <item>
///     <term>Win32：</term>
///   </item>
/// </list>
/// <para>添加新的枚举常量注意事项：</para>
/// <para>在<see cref="NotificationType_Channel_EnumExtensions.GetName(NotificationChannelType)"/>中添加渠道名称</para>
/// <para>在<see cref="NotificationType_Channel_EnumExtensions.GetDescription(NotificationChannelType)"/>中添加渠道描述</para>
/// <para>在<see cref="NotificationType_Channel_EnumExtensions.GetImportanceLevel(NotificationChannelType)"/>中添加渠道重要性级别</para>
/// </summary>
public enum NotificationChannelType
{
    /// <inheritdoc cref="NotificationType.NewVersion"/>
    NewVersion = 2,

    /// <inheritdoc cref="NotificationType.Announcement"/>
    Announcement,

    /// <inheritdoc cref="NotificationType.Message"/>
    Message,

    /// <summary>
    /// Android 前台服务
    /// <para><see cref="NotificationType.ProxyForegroundService"/></para>
    /// <para><see cref="NotificationType.ArchiSteamFarmForegroundService"/></para>
    /// </summary>
    ForegroundService,
}

/// <summary>
/// Enum 扩展 <see cref="NotificationType"/> AND <see cref="NotificationChannelType"/>
/// </summary>
public static partial class NotificationType_Channel_EnumExtensions
{
    /// <summary>
    /// 获取所属的通知渠道
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotificationChannelType GetChannel(this NotificationType value) => value switch
    {
        NotificationType.Announcement => NotificationChannelType.Announcement,
        NotificationType.NewVersion => NotificationChannelType.NewVersion,
        NotificationType.Message => NotificationChannelType.Message,
        NotificationType.ProxyForegroundService or
        NotificationType.ArchiSteamFarmForegroundService => NotificationChannelType.ForegroundService,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>
    /// 获取渠道的用户可见名称
    /// <para>建议的最大长度为40个字符，如果该值太长，可能会被截断</para>
    /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setName%28java.lang.CharSequence%29 </para>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this NotificationChannelType value)
    {
        return value switch
        {
            NotificationChannelType.Announcement => AppResources.NotificationChannelType_Name_Announcement,
            NotificationChannelType.NewVersion => AppResources.NotificationChannelType_Name_NewVersion,
            NotificationChannelType.Message => AppResources.NotificationChannelType_Name_Message,
            NotificationChannelType.ForegroundService => AppResources.NotificationChannelType_Name_ForegroundService,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }

    /// <summary>
    /// 获取渠道的用户可见描述
    /// <para>建议的最大长度为300个字符，如果该值太长，可能会被截断</para>
    /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setDescription%28java.lang.String%29 </para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescription(this NotificationChannelType value)
    {
        return value switch
        {
            NotificationChannelType.Announcement => AppResources.NotificationChannelType_Description_Announcement,
            NotificationChannelType.NewVersion => AppResources.NotificationChannelType_Description_NewVersion,
            NotificationChannelType.Message => AppResources.NotificationChannelType_Description_Message,
            NotificationChannelType.ForegroundService => AppResources.NotificationChannelType_Description_ForegroundService,
            _ => string.Empty,
        };
    }

    /// <summary>
    /// 获取渠道的重要性级别
    /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#getImportance%28%29 </para>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NotificationImportanceLevel GetImportanceLevel(this NotificationChannelType value)
        => value switch
        {
            NotificationChannelType.Announcement => NotificationImportanceLevel.High,
            _ => NotificationImportanceLevel.Medium,
        };
}