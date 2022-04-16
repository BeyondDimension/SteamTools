using System.Windows;
using static System.Application.Services.INotificationService;
using System.Runtime.Versioning;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    internal sealed class WindowsNotificationServiceImpl : INotificationService
    {
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("macos")]
        [SupportedOSPlatform("linux")]
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

        void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
        {
            title ??= DefaultTitle;
            ShowBalloonTip(title, text, ToolTipIcon.None);
        }
    }
}
