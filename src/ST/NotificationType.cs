namespace System.Application
{
    /// <summary>
    /// 通知类型
    /// <list type="bullet">
    ///   <item>
    ///     <term>iOS：</term>
    ///   </item>
    ///   <item>
    ///     <term>Android：将枚举转换为 <see cref="int"/> 用作 Android.App.NotificationManager 中的 id</term>
    ///   </item>
    ///   <item>
    ///     <term>UWP：</term>
    ///   </item>
    ///   <item>
    ///     <term>Win32：</term>
    ///   </item>
    /// </list>
    /// <para>添加新的枚举常量注意事项：</para>
    /// <para>(客户端)如果要显示在设备的通知栏上，则需要在 src\ST.Client\Extensions\NotificationType_Channel_EnumExtensions.cs 中添加或指定已有渠道</para>
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 当有新版本可更新时
        /// <para>例如：</para>
        /// <para>新版本下载进度</para>
        /// </summary>
        NewVersion = 1,

        /// <summary>
        /// 由服务端下发的公告
        /// </summary>
        Announcement,

        /// <summary>
        /// 消息通知
        /// </summary>
        Message,

        /// <summary>
        /// 本地加速代理服务(Android 前台服务通知)
        /// </summary>
        ProxyForegroundService,

        /// <summary>
        /// ASF 服务(Android 前台服务通知)
        /// </summary>
        ArchiSteamFarmForegroundService,

        /* 添加新的枚举值注意事项：
         * 需要对该 通知类型 指定一个渠道，多个通知类型可对应一个通知渠道
         *  在 ST.Client NotificationChannelType
         *  NotificationType_Channel_EnumExtensions.GetChannel(NotificationType)
         * 如果为新增渠道，则需要增加以下内容
         *  渠道的用户可见名称，必填
         *  NotificationType_Channel_EnumExtensions.GetName(NotificationChannelType)
         *  渠道的用户可见描述，可选
         *  NotificationType_Channel_EnumExtensions.GetDescription(NotificationChannelType)
         *  渠道的重要性级别，可选，默认为 中等(Medium)
         *  NotificationType_Channel_EnumExtensions.GetImportanceLevel(NotificationChannelType)
         */
    }
}