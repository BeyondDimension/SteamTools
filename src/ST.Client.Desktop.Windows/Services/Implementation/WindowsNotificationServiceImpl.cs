using System.Windows;
using static System.Application.Services.INotificationService;
using System.Application.Models;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    internal sealed class WindowsNotificationServiceImpl : INotificationService
    {
        static NotifyIcon? NotifyIcon => DI.Get_Nullable<NotifyIcon>();

        static void HideBalloonTip()
        {
            if (!NotifyIconHelper.IsInitialized) return;
            NotifyIcon?.HideBalloonTip();
        }

        static void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            if (!NotifyIconHelper.IsInitialized) return;
            NotifyIcon?.ShowBalloonTip(title, text, icon);
        }

        void INotificationService.Cancel(NotificationType _) => HideBalloonTip();

        void INotificationService.CancelAll() => HideBalloonTip();

        void INotificationService.Notify(NotificationBuilder.IInterface builder)
        {
            ShowBalloonTip(builder.Title, builder.Content, ToolTipIcon.None);
        }

        void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
        {
            title ??= DefaultTitle;
            ShowBalloonTip(title, text, ToolTipIcon.None);
        }
    }
}
