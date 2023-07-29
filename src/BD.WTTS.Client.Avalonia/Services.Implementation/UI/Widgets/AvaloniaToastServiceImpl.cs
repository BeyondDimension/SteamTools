using Avalonia.Controls.Notifications;
using BD.WTTS.UI.Views.Controls;
using AvaloniaNotification = Avalonia.Controls.Notifications.Notification;
using AvaNotificationType = Avalonia.Controls.Notifications.NotificationType;

namespace BD.WTTS.Services.Implementation;

[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright")]
sealed class AvaloniaToastServiceImpl : IToastService
{
    public SnackbarManager? NotificationManager { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static AvaNotificationType GetNotificationType(ToastIcon icon) => icon switch
    {
        ToastIcon.Info => AvaNotificationType.Information,
        ToastIcon.Success => AvaNotificationType.Success,
        ToastIcon.Warning => AvaNotificationType.Warning,
        ToastIcon.Error => AvaNotificationType.Error,
        _ => AvaNotificationType.Information,
    };

    /// <summary>
    /// 显示Toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="duration"></param>
    public void Show(ToastIcon icon, string text, int? duration = null)
    {
        var host = AvaloniaWindowManagerImpl.GetWindowTopLevel();

        NotificationManager ??= new SnackbarManager(host)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 5,
        };

        var notificationType = GetNotificationType(icon);
        NotificationManager.Show(text, notificationType: notificationType);
    }

    /// <inheritdoc cref="Show(string, int?)"/>
    public void Show(ToastIcon icon, string text, ToastLength duration)
    {
        Show(icon, text, null);
    }
}
