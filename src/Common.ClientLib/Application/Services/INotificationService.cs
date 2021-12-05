namespace System.Application.Services
{
    /// <summary>
    /// 通知栏服务
    /// </summary>
    /// <typeparam name="TNotificationType"></typeparam>
    /// <typeparam name="TEntrance"></typeparam>
    public interface INotificationService<TNotificationType, TEntrance, TNotificationService>
        where TNotificationType : notnull, Enum
        where TEntrance : notnull, Enum
        where TNotificationService : INotificationService<TNotificationType, TEntrance, TNotificationService>
    {
        /// <summary>
        /// 获取是否有通知权限
        /// </summary>
        /// <returns></returns>
        bool AreNotificationsEnabled();

        /// <summary>
        /// 显示通知
        /// </summary>
        /// <param name="text">通知内容</param>
        /// <param name="notificationType">通知类型</param>
        /// <param name="autoCancel"></param>
        /// <param name="title">通知标题</param>
        /// <param name="entrance">点击通知的入口点</param>
        void Notify(
            string text,
            TNotificationType notificationType,
            bool autoCancel = true,
            string? title = default,
            TEntrance? entrance = default);

        /// <summary>
        /// 取消通知
        /// </summary>
        /// <param name="notificationType"></param>
        void Cancel(TNotificationType notificationType);

        /// <summary>
        /// 取消所有通知
        /// </summary>
        void CancelAll();

        /// <summary>
        /// 下载进度通知
        /// </summary>
        Progress<float> NotifyDownload(
            Func<string> text,
            TNotificationType notificationType,
            string? title = default);
    }
}