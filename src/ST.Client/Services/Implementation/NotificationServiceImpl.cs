using System;
using System.Collections.Generic;
using System.Text;
using System.Properties;
using System.Windows;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    internal sealed class NotificationServiceImpl : INotificationService
    {
        readonly NotifyIcon notifyIcon;

        public NotificationServiceImpl(NotifyIcon notifyIcon)
        {
            this.notifyIcon = notifyIcon;
        }

        bool INotificationService<NotificationType, Entrance, INotificationService>.AreNotificationsEnabled() => true;

        void INotificationService<NotificationType, Entrance, INotificationService>.Cancel(NotificationType notificationType)
        {
            if (OperatingSystem2.IsWindows)
            {
                notifyIcon.HideBalloonTip();
            }
        }

        void INotificationService<NotificationType, Entrance, INotificationService>.CancelAll()
        {
            if (OperatingSystem2.IsWindows)
            {
                notifyIcon.HideBalloonTip();
            }
        }

        void INotificationService<NotificationType, Entrance, INotificationService>.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance)
        {
            if (OperatingSystem2.IsWindows)
            {
                title ??= Constants.HARDCODED_APP_NAME;
                // 调用托盘显示通知
                notifyIcon.ShowBalloonTip(title, text, ToolTipIcon.None);
            }
        }

        Progress<float> INotificationService<NotificationType, Entrance, INotificationService>.NotifyDownload(Func<string> text, NotificationType notificationType, string? title)
        {
            // 桌面端不支持带进度的通知，下载新版本改用桌面端的窗口进度条控件，由更新服务桌面端版实现
            throw new PlatformNotSupportedException();
        }
    }
}
