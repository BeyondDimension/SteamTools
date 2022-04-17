using System.Application.Models;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    internal sealed class NotificationServiceImpl : INotificationService
    {
        void INotificationService.Cancel(NotificationType notificationType)
        {

        }

        void INotificationService.CancelAll()
        {

        }

        void INotificationService.Notify(NotificationBuilder.IInterface builder)
        {

        }

        void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
        {

        }
    }
}
