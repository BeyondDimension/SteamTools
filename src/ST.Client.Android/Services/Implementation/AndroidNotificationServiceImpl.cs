using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using AndroidApplication = Android.App.Application;
using CC = System.Common.Constants;
using JClass = Java.Lang.Class;
using NativeService = System.Application.Services.Native.IServiceBase;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    internal sealed class AndroidNotificationServiceImpl : INotificationService
    {
        public static AndroidNotificationServiceImpl Instance => (AndroidNotificationServiceImpl)INotificationService.Instance;

        readonly IAndroidApplication app;
        public AndroidNotificationServiceImpl(IAndroidApplication app)
        {
            this.app = app;
        }

        public static bool IsSupportedNotificationChannel
            => Build.VERSION.SdkInt >= BuildVersionCodes.O;

        JClass? GetEntrance(Entrance entrance)
        {
            var notificationEntrance = (entrance != default ? GetActivityType(entrance)?.GetJClass() : null) ?? app.NotificationEntrance;
            return notificationEntrance;
        }

        Type? GetActivityType(Entrance entrance) => entrance switch
        {
            Entrance.Main => app.MainActivityType,
            _ => null,
        };

        static int GetNotifyId(NotificationType notificationType) => Enum2.ConvertToInt32(notificationType);

        public bool AreNotificationsEnabled()
        {
            // https://www.jianshu.com/p/1e27efb1dcac
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            return manager.AreNotificationsEnabled();
        }

        public void Cancel(NotificationType notificationType)
        {
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            var id = GetNotifyId(notificationType);
            manager.Cancel(id);
        }

        public void CancelAll()
        {
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            manager.CancelAll();
        }

        /// <summary>
        /// 获取渠道的ID
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#public-constructors </para>
        /// </summary>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        static string GetChannelId(NotificationChannelType notificationChannelType)
        {
            var valueInt = Enum2.ConvertToInt32(notificationChannelType);
            return "chan_" + valueInt;
        }

        static void CreateNotificationChannel(NotificationChannelType notificationChannelType, NotificationChannel notificationChannel)
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

        /// <summary>
        /// 创建通知渠道 >= Android O
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        static NotificationChannel? CreateNotificationChannel(NotificationManagerCompat manager,
            NotificationChannelType notificationChannelType, out string channelId)
        {
            channelId = GetChannelId(notificationChannelType);
            if (Build.VERSION.SdkInt < BuildVersionCodes.O) return null;
            var notificationChannel = manager.GetNotificationChannel(channelId);
            if (notificationChannel == null)
            {
                var name = notificationChannelType.GetName();
                var description = notificationChannelType.GetDescription();
                var level = GetNotificationImportance(notificationChannelType.GetImportanceLevel());
                notificationChannel = new NotificationChannel(channelId, name, level);
                if (!string.IsNullOrWhiteSpace(description)) notificationChannel.Description = description;
                CreateNotificationChannel(notificationChannelType, notificationChannel);
                manager.CreateNotificationChannel(notificationChannel);
            }
            return notificationChannel;
        }

        /// <summary>
        /// 获取渠道的优先级 Android 7.1 and lower
        /// <para>参考：https://developer.android.google.cn/training/notify-user/channels#importance </para>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        static int GetNotificationPriority(NotificationImportanceLevel level) => level switch
        {
            NotificationImportanceLevel.Low => NotificationCompat.PriorityMin,
            NotificationImportanceLevel.Medium => NotificationCompat.PriorityLow,
            NotificationImportanceLevel.High => NotificationCompat.PriorityDefault,
            NotificationImportanceLevel.Urgent => NotificationCompat.PriorityHigh,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };

        /// <summary>
        /// 获取渠道的重要性级别 Android 8.0 and higher
        /// <para>参考：https://developer.android.google.cn/training/notify-user/channels#importance </para>
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        static NotificationImportance GetNotificationImportance(NotificationImportanceLevel level) => level switch
        {
            NotificationImportanceLevel.Low => NotificationImportance.Min,
            NotificationImportanceLevel.Medium => NotificationImportance.Low,
            NotificationImportanceLevel.High => NotificationImportance.Default,
            NotificationImportanceLevel.Urgent => NotificationImportance.High,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
        };

        static (NotificationCompat.Builder builder, NotificationManagerCompat manager) BuildNotify(
            Context context,
            NotificationManagerCompat? manager,
            string text,
            NotificationType notificationType,
            bool? autoCancel = null,
            string? title = null,
            JClass? entrance = null,
            string? entranceAction = null,
            IReadOnlyCollection<NotificationCompat.Action>? actions = null)
        {
            var channelType = notificationType.GetChannel();
            manager ??= NotificationManagerCompat.From(context);
            CreateNotificationChannel(manager, channelType, out var channelId);
            var builder = new NotificationCompat.Builder(context, channelId);
            var level = channelType.GetImportanceLevel();
            builder.SetPriority(GetNotificationPriority(level));
            var status_bar_icon = IAndroidApplication.Instance.NotificationSmallIconResId;
            if (status_bar_icon.HasValue) builder.SetSmallIcon(status_bar_icon.Value);
            title ??= Constants.HARDCODED_APP_NAME;
            builder.SetContentTitle(title);
            builder.SetContentText(text);
            if (autoCancel.HasValue) builder.SetAutoCancel(autoCancel.Value);
            if (actions != default && actions.Count > 0 && actions.Count <= 3)
            {
                foreach (var item in actions)
                {
                    builder.AddAction(item);
                }
            }
            if (entrance != null)
            {
                var intent = new Intent(context, entrance);
                if (!string.IsNullOrWhiteSpace(entranceAction)) intent.SetAction(entranceAction);
                var pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
                builder.SetContentIntent(pendingIntent);
            }
            return (builder, manager);
        }

        public void Notify(string text,
           NotificationType notificationType,
           bool autoCancel,
           string? title,
           Entrance entrance)
        {
            var notificationEntrance = GetEntrance(entrance);
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            var (builder, _) = BuildNotify(context, manager, text, notificationType,
                autoCancel, title, entrance: notificationEntrance);
            var notifyId = GetNotifyId(notificationType);
            manager.Notify(notifyId, builder.Build());
        }

        public Progress<float> NotifyDownload(
            Func<string> text,
            NotificationType notificationType,
            string? title)
        {
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            var (builder, _) = BuildNotify(context, manager,
                text: text(),
                notificationType,
                title: title);
            // 进度单位说明
            // 通用层采用 float 浮点数作为进度值，范围从0~100，保留两位小数
            // 平台层采用 int 整型作为进度值，范围从0~10000，转换需要 乘 100
            const int unit_convert_multiple = 100; // 从float到int转换单位倍数
            const float PROGRESS_MAX = CC.MaxProgress * unit_convert_multiple; // 进度条满值
            int PROGRESS_MAX_INT32 = PROGRESS_MAX.ToInt32();
            // 发出零进度的初始通知
            // Issue the initial notification with zero progress
            builder.SetProgress(PROGRESS_MAX_INT32, 0, false);
            var notifyId = GetNotifyId(notificationType);
            manager.Notify(notifyId, builder.Build());
            // 在这里完成跟踪进度的工作。
            // Do the job here that tracks the progress.
            // 通常，这应该在一个
            // Usually, this should be in a
            // 工作线程
            // worker thread
            // 要显示进度，请更新PROGRESS_CURRENT并使用以下命令更新通知：
            // To show progress, update PROGRESS_CURRENT and update the notification with:
            // builder.setProgress(PROGRESS_MAX, PROGRESS_CURRENT, false);
            // notificationManager.notify(notificationId, builder.build());
            // 完成后，再次更新通知以删除进度条
            // When done, update the notification one more time to remove the progress bar
            void Handler(float current)
            {
                var currentInt32 = (current * unit_convert_multiple).ToInt32();
                if (currentInt32 >= PROGRESS_MAX_INT32)
                {
                    // 这将使100%时直接取消通知，并不会在UI上显示
                    // 报告进度值满的操作应当是幂等的
                    if (manager != null)
                    {
                        manager.Cancel(notifyId);
                        manager = null;
                    }
                    // 手动释放相关资源
                    context = null;
                    builder = null;
                }
                else
                {
                    // 在报告进度值满后不可再更改进度
                    if (builder == null) throw new ArgumentNullException(nameof(builder));
                    if (manager == null) throw new ArgumentNullException(nameof(manager));
                    builder.SetProgress(PROGRESS_MAX_INT32, currentInt32, false);
                    builder.SetContentText(text());
                    manager.Notify(notifyId, builder.Build());
                }
            }
            return new Progress<float>(Handler);
        }

        /// <summary>
        /// 停止服务按钮
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        NotificationCompat.Action BuildStopServiceAction(Service service)
        {
            var intent = new Intent(service, service.GetType());
            intent.SetAction(NativeService.STOP);
            const int requestCode = 0;
            const PendingIntentFlags flags = PendingIntentFlags.Immutable;
            var pendingIntent = Build.VERSION.SdkInt >= BuildVersionCodes.O ?
                PendingIntent.GetForegroundService(service, requestCode, intent, flags) :
                PendingIntent.GetService(service, requestCode, intent, flags);
            const int icon = 0;
            var builder = new NotificationCompat.Action.Builder(icon,
                AppResources.StopService, pendingIntent);
            return builder.Build();
        }

        /// <summary>
        /// 使用通知栏来设置前台服务
        /// </summary>
        /// <param name="service"></param>
        /// <param name="notificationType"></param>
        /// <param name="text"></param>
        /// <param name="entranceAction"></param>
        public (NotificationCompat.Builder builder, NotificationManagerCompat manager, int notifyId) StartForeground(Service service,
            NotificationType notificationType,
            string text,
            string? entranceAction = null)
        {
            var stopAction = BuildStopServiceAction(service);
            var (builder, manager) = BuildNotify(service, null,
                text, notificationType,
                entrance: GetEntrance(Entrance.Main),
                actions: new NotificationCompat.Action[] {
                    stopAction,
                },
                entranceAction: entranceAction);
            var notification = builder.Build();
            var notifyId = GetNotifyId(notificationType);
            service.StartForeground(notifyId, notification);
            return (builder, manager, notifyId);
        }
    }
}
