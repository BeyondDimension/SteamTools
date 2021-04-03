using System.Properties;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService{TNotificationType, TEntrance}"/>
    internal sealed class PlatformNotificationServiceImpl : INotificationService
    {
        public bool AreNotificationsEnabled() => true;

        public void Cancel(NotificationType notificationType)
        {
        }

        public void CancelAll()
        {
        }

        public void Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance)
        {
            title ??= ThisAssembly.AssemblyTrademark;
            //if (主窗口显示中)
            //{
            //打开提示(公告)窗口
            //}
            //else
            //{
            //写入本地数据库待下一次打开主窗口时显示
            //}
            throw new NotImplementedException(title);
        }

        public Progress<float> NotifyDownload(string text, NotificationType notificationType, string? title)
        {
            // 桌面端不支持带进度的通知，下载新版本改用桌面端的窗口进度条控件，由更新服务桌面端版实现
            //title ??= ThisAssembly.AssemblyTrademark;
            throw new NotImplementedException(/*title*/);
        }
    }
}