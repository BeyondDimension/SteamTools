using Avalonia.Controls.Notifications;
using AvaloniaNotification = Avalonia.Controls.Notifications.Notification;

namespace BD.WTTS.Services.Implementation;

[TemplatePart("PART_Items", typeof(Panel))]
[PseudoClasses(":topleft", ":topright", ":bottomleft", ":bottomright")]
sealed class AvaloniaToastServiceImpl : IToastService
{
    public WindowNotificationManager? NotificationManager { get; set; }

    /// <summary>
    /// 显示Toast
    /// </summary>
    /// <param name="text"></param>
    /// <param name="duration"></param>
    public void Show(string text, int? duration = null)
    {
        TopLevel? host = null;

        if (App.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                host = TopLevel.GetTopLevel(window);
            }
        }
        else if (App.Instance.ApplicationLifetime is ISingleViewApplicationLifetime view)
        {
            var mainView = view.MainView;
            if (mainView != null)
            {
                host = TopLevel.GetTopLevel(mainView);
            }
        }

        NotificationManager ??= new WindowNotificationManager(host)
        {
            Position = NotificationPosition.BottomRight,
            MaxItems = 4,
        };

        var info = new InfoBar()
        {
            Title = "Welcome",
            Message = "Avalonia now supports Notifications.",
            Severity = InfoBarSeverity.Informational,
            IsClosable = true,
        };

        NotificationManager.Show(info);
    }

    /// <inheritdoc cref="Show(string, int?)"/>
    public void Show(string text, ToastLength duration)
    {
        Show(text, null);
    }
}
