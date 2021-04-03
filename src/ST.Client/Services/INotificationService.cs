namespace System.Application.Services
{
    /// <inheritdoc cref="INotificationService{TNotificationType, TEntrance}"/>
    public interface INotificationService : INotificationService<NotificationType, Entrance>
    {
    }
}