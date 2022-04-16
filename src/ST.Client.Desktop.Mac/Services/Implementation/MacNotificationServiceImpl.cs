//using UserNotifications;

//// https://docs.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/local-notifications#create-the-ios-interface-implementation
//// https://github.com/davidortinau/WeatherTwentyOne/blob/main/src/WeatherTwentyOne/Platforms/MacCatalyst/NotificationService.cs
//// https://docs.microsoft.com/en-us/dotnet/api/usernotifications.unusernotificationcenter?view=xamarin-ios-sdk-12

//namespace System.Application.Services.Implementation
//{
//    /// <inheritdoc cref="INotificationService"/>
//    internal sealed class MacNotificationServiceImpl : INotificationService
//    {
//        void INotificationService.Cancel(NotificationType notificationType)
//        {

//        }

//        void INotificationService.CancelAll()
//        {

//        }

//        void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
//        {
//            title ??= INotificationService.DefaultTitle;

//            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
//            {
//                if (!approved)
//                    return;

//                var content = new UNMutableNotificationContent
//                {
//                    Title = title,
//                    Body = text,
//                };

//                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);
//                var request = UNNotificationRequest.FromIdentifier(Guid.NewGuid().ToString(), content, trigger);
//                UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
//                {
//                    if (err != null)
//                        throw new Exception($"Failed to schedule notification: {err}");
//                });
//            });
//        }
//    }
//}
