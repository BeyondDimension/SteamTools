#if WINDOWS
using System.Windows;
using static BD.WTTS.Services.INotificationService;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="INotificationService"/>
sealed class WindowsNotificationServiceImpl : INotificationService
{
    //static NotifyIcon? NotifyIcon => Ioc.Get_Nullable<NotifyIcon>();

    static void HideBalloonTip()
    {
        //if (!NotifyIconHelper.IsInitialized) return;
        //NotifyIcon?.HideBalloonTip();
    }

    static void ShowBalloonTip(string title, string text/*, ToolTipIcon icon*/)
    {
        //if (!NotifyIconHelper.IsInitialized) return;
        //NotifyIcon?.ShowBalloonTip(title, text, icon);
    }

    void INotificationService.Cancel(NotificationType _) => HideBalloonTip();

    void INotificationService.CancelAll() => HideBalloonTip();

    void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
    {
        title ??= DefaultTitle;
        //ShowBalloonTip(title, text, ToolTipIcon.None);
    }
}
#endif