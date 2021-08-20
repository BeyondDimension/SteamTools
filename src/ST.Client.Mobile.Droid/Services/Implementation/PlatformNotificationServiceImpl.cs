using Android.App;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformNotificationServiceImpl : PlatformNotificationServiceImpl<NotificationType, NotificationChannelType, Entrance, INotificationService>, INotificationService
    {
        protected override void CreateNotificationChannel(NotificationChannelType notificationChannelType, NotificationChannel notificationChannel)
        {
            switch (notificationChannelType)
            {
                case NotificationChannelType.NewVersion:
                    // 设置绕过请勿打扰模式
                    notificationChannel.SetBypassDnd(true);
                    // 设置显示桌面Launcher的消息角标
                    notificationChannel.SetShowBadge(false);
                    // 设置通知出现时的震动（如果 android 设备支持的话）
                    notificationChannel.EnableVibration(false);
                    break;
            }
        }

        protected override NotificationChannelType GetChannel(NotificationType notificationType)
        {
            return notificationType.GetChannel();
        }

        protected override string GetDescription(NotificationChannelType notificationChannelType)
        {
            return notificationChannelType.GetDescription();
        }

        protected override NotificationImportanceLevel GetImportanceLevel(NotificationChannelType notificationChannelType)
        {
            return notificationChannelType.GetImportanceLevel();
        }

        protected override string GetName(NotificationChannelType notificationChannelType)
        {
            return notificationChannelType.GetName();
        }
    }
}