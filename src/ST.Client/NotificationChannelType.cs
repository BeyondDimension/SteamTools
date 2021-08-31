namespace System.Application
{
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
        /// <summary>
        /// 新版本
        /// <para>例如：</para>
        /// <para>新版本下载进度</para>
        /// </summary>
        NewVersion = 2,

        /// <summary>
        /// 公告
        /// </summary>
        Announcement,
    }
}