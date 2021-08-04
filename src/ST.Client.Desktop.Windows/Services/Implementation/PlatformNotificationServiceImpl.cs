using System.Properties;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService{TNotificationType, TEntrance}"/>
    internal sealed class PlatformNotificationServiceImpl : INotificationService
    {
        readonly IDesktopPlatformService dps;

        public PlatformNotificationServiceImpl(IDesktopPlatformService dps)
        {
            this.dps = dps;
        }

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
            // 调用托盘显示通知
            throw new NotImplementedException(title);
        }

        public Progress<float> NotifyDownload(string text, NotificationType notificationType, string? title)
        {
            // 桌面端不支持带进度的通知，下载新版本改用桌面端的窗口进度条控件，由更新服务桌面端版实现
            //title ??= ThisAssembly.AssemblyTrademark;
            //throw new NotImplementedException(title);
            throw new PlatformNotSupportedException();
        }
    }
}