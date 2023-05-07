using Avalonia.Controls.Notifications;
using BD.WTTS.UI.Views.Controls;
using AvaloniaNotification = Avalonia.Controls.Notifications.Notification;

namespace BD.WTTS.Services.Implementation;

[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright")]
sealed class AvaloniaToastServiceImpl : IToastService
{
    public SnackbarManager? NotificationManager { get; set; }

    /// <summary>
    /// 显示Toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="duration"></param>
    public void Show(string text, int? duration = null)
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
