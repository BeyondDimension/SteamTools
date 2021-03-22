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
    /// <para>(客户端)如果要显示在设备的通知栏上，则需要在<see cref="NotificationType_Channel_EnumExtensions.GetChannel(NotificationType)"/>中添加对应渠道</para>
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 新版本下载进度
        /// </summary>
        NewVersionDownloadProgress = 7,
    }
}