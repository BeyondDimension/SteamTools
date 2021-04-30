using Android.App;
using Android.Content;
using AndroidX.Core.App;
using System.Collections.Generic;
using System.Common;
using AndroidApplication = Android.App.Application;
using JClass = Java.Lang.Class;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService{TNotificationType, TEntrance}"/>
    public abstract class PlatformNotificationServiceImpl<TNotificationType, TNotificationChannelType, TEntrance> : INotificationService<TNotificationType, TEntrance>
        where TNotificationType : notnull, Enum
        where TNotificationChannelType : notnull, Enum
        where TEntrance : notnull, Enum
    {
        protected virtual Type? GetActivityType(TEntrance entrance) => null;

        protected virtual int GetNotifyId(TNotificationType notificationType)
            => Enum2.ConvertToInt32(notificationType);

        public bool AreNotificationsEnabled()
        {
            // https://www.jianshu.com/p/1e27efb1dcac
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            return manager.AreNotificationsEnabled();
        }

        public void Cancel(TNotificationType notificationType)
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
        /// 初始化通知渠道
        /// </summary>
        /// <param name="context"></param>
        public void InitNotificationChannels(Context context)
        {
            var manager = NotificationManagerCompat.From(context);
            var items = Enum.GetValues(typeof(TNotificationChannelType));
            foreach (TNotificationChannelType item in items)
            {
                CreateNotificationChannel(manager, item);
            }
        }

        /// <summary>
        /// 获取渠道的ID
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#public-constructors </para>
        /// </summary>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        protected virtual string GetChannelId(TNotificationChannelType notificationChannelType)
        {
            var valueInt = Enum2.ConvertToInt32(notificationChannelType);
            return "chan_" + valueInt;
        }

        /// <summary>
        /// 获取渠道的用户可见名称
        /// <para>建议的最大长度为40个字符，如果该值太长，可能会被截断</para>
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setName%28java.lang.CharSequence%29 </para>
        /// </summary>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        protected abstract string GetName(TNotificationChannelType notificationChannelType);

        /// <summary>
        /// 获取渠道的用户可见描述
        /// <para>建议的最大长度为300个字符，如果该值太长，可能会被截断</para>
        /// <para>参考：https://developer.android.google.cn/reference/android/app/NotificationChannel?hl=en#setDescription%28java.lang.String%29 </para>
        /// </summary>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        protected abstract string GetDescription(TNotificationChannelType notificationChannelType);

        protected abstract void CreateNotificationChannel(TNotificationChannelType notificationChannelType, NotificationChannel notificationChannel);

        /// <summary>
        /// 创建通知渠道 >= Android O
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        protected NotificationChannel CreateNotificationChannel(NotificationManagerCompat manager,
           TNotificationChannelType notificationChannelType)
        {
            var channelId = GetChannelId(notificationChannelType);
            var name = GetName(notificationChannelType);
            var description = GetDescription(notificationChannelType);
            var level = GetNotificationImportance(GetImportanceLevel(notificationChannelType));
            var notificationChannel = manager.GetNotificationChannel(channelId);
            if (notificationChannel == null)
            {
                notificationChannel = new NotificationChannel(channelId, name, level)
                {
                    Description = description,
                };
                CreateNotificationChannel(notificationChannelType, notificationChannel);
                manager.CreateNotificationChannel(notificationChannel);
            }
            return notificationChannel;
        }

        /// <summary>
        /// 获取所属的通知渠道
        /// </summary>
        /// <param name="notificationType"></param>
        /// <returns></returns>
        protected abstract TNotificationChannelType GetChannel(TNotificationType notificationType);

        /// <summary>
        /// 获取渠道的重要性级别
        /// </summary>
        /// <param name="notificationChannelType"></param>
        /// <returns></returns>
        protected abstract NotificationImportanceLevel GetImportanceLevel(TNotificationChannelType notificationChannelType);

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

        protected NotificationCompat.Builder BuildNotify(
            Context context,
            NotificationManagerCompat manager,
            string text,
            TNotificationType notificationType,
            bool? autoCancel = null,
            string? title = null,
            JClass? entrance = null,
            IReadOnlyCollection<NotificationCompat.Action>? actions = null)
        {
            var channelType = GetChannel(notificationType);
            var channel = CreateNotificationChannel(manager, channelType);
            var builder = new NotificationCompat.Builder(context, channel.Id);
            var level = GetImportanceLevel(channelType);
            builder.SetPriority(GetNotificationPriority(level));
            var status_bar_icon = R.drawable.ic_stat_notify_msg;
            if (status_bar_icon.HasValue) builder.SetSmallIcon(status_bar_icon.Value);
            title ??= R.@string.app_name;
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
                var pendingIntent = PendingIntent.GetActivity(context, 0, intent, 0);
                builder.SetContentIntent(pendingIntent);
            }
            return builder;
        }

        public void Notify(string text,
           TNotificationType notificationType,
           bool autoCancel,
           string? title,
           TEntrance entrance)
        {
            var notificationEntrance = entrance != null ?
                GetActivityType(entrance).GetJClass() :
                R.activities.entrance;
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            var builder = BuildNotify(context, manager, text, notificationType,
                autoCancel, title, entrance: notificationEntrance);
            var notifyId = GetNotifyId(notificationType);
            manager.Notify(notifyId, builder.Build());
        }

        public Progress<float> NotifyDownload(
            string text,
            TNotificationType notificationType,
            string? title)
        {
            var context = AndroidApplication.Context;
            var manager = NotificationManagerCompat.From(context);
            var builder = BuildNotify(context, manager,
                text: text.Format(0),
                notificationType,
                title: title);
            // 进度单位说明
            // 通用层采用 float 浮点数作为进度值，范围从0~100，保留两位小数
            // 平台层采用 int 整型作为进度值，范围从0~10000，转换需要 乘 100
            const int unit_convert_multiple = 100; // 从float到int转换单位倍数
            const int PROGRESS_MAX = 100 * unit_convert_multiple; // 进度条满值
            // 发出零进度的初始通知
            // Issue the initial notification with zero progress
            builder.SetProgress(PROGRESS_MAX, 0, false);
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
                if (currentInt32 >= PROGRESS_MAX)
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
                    builder.SetProgress(PROGRESS_MAX, currentInt32, false);
                    builder.SetContentText(text.Format(currentInt32));
                    manager.Notify(notifyId, builder.Build());
                }
            }
            return new Progress<float>(Handler);
        }
    }
}