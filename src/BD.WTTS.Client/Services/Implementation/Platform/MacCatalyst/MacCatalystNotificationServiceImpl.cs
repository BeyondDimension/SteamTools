#if MACOS || MACCATALYST || IOS
using UserNotifications;

// https://docs.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/local-notifications#create-the-ios-interface-implementation
// https://github.com/davidortinau/WeatherTwentyOne/blob/main/src/WeatherTwentyOne/Platforms/MacCatalyst/NotificationService.cs
// https://docs.microsoft.com/en-us/dotnet/api/usernotifications.unusernotificationcenter?view=xamarin-ios-sdk-12

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="INotificationService"/>
sealed class MacCatalystNotificationServiceImpl : INotificationService
{
    const string TAG = "MacCatalystNotificationS";

    void INotificationService.Cancel(NotificationType notificationType)
    {
        UNUserNotificationCenter.Current.RemoveDeliveredNotifications(new[] { notificationType.ToString() });
    }

    void INotificationService.CancelAll()
    {
        UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
    }

    void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
    {
        title ??= INotificationService.DefaultTitle;
        UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
        {
            if (!approved)
                return;

            var content = new UNMutableNotificationContent
            {
                Title = title,
                Body = text,
            };

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
            var request = UNNotificationRequest.FromIdentifier(notificationType.ToString(), content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
            {
                if (err != null)
                {
                    Log.Error(TAG, $"Failed to schedule notification: {err}");
                }
            });
        });
    }
}
#endif