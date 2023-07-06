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
        Show(text, NotificationType.Information);
    }

    /// <inheritdoc cref="Show(string, int?)"/>
    public void Show(string text, ToastLength duration)
    {
        Show(text, NotificationType.Information);
    }

    public void Show(string text, NotificationType type)
    {
        var host = AvaloniaWindowManagerImpl.GetWindowTopLevel();

        NotificationManager ??= new SnackbarManager(host)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 5,
        };

        NotificationManager.Show(text);
    }

    /// <inheritdoc cref="Show(string, int?)"/>
    public void Show(string text, ToastLength duration)
    {
        Show(text, null);
    }
}
