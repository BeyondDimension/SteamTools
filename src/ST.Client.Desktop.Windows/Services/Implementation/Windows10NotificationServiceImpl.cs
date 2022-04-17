using Microsoft.Toolkit.Uwp.Notifications;
using System.Runtime.Versioning;
using System.Application.Models;
using Windows.UI.Notifications;
using System.Linq;
using CC = System.Common.Constants;

// https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/scheduled-toast
// https://github.com/davidortinau/WeatherTwentyOne/blob/main/src/WeatherTwentyOne/Platforms/Windows/NotificationService.cs

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INotificationService"/>
    [SupportedOSPlatform("Windows10.0.17763.0")]
    internal sealed class Windows10NotificationServiceImpl : INotificationService
    {
        bool INotificationService.AreNotificationsEnabled()
        {
            var notifier = ToastNotificationManagerCompat.CreateToastNotifier();
            return notifier.Setting == NotificationSetting.Enabled;
        }

        void INotificationService.Cancel(NotificationType notificationType)
        {
            (var tag, var group) = GetTagAndGroup(notificationType);
            ToastNotificationManagerCompat.History.Remove(tag, group);
        }

        void INotificationService.CancelAll()
        {
            ToastNotificationManagerCompat.History.Clear();
        }

        static (string tag, string group) GetTagAndGroup(NotificationType notificationType)
        {
            var tag = notificationType.ToString();
            var group = notificationType.GetChannel().ToString();
            return (tag, group);
        }

        void INotificationService.Notify(NotificationBuilder.IInterface b)
        {
            var builder = new ToastContentBuilder()
                .AddToastActivationInfo(null, ToastActivationType.Foreground)
                .AddText(b.Title, hintStyle: AdaptiveTextStyle.Header)
                .AddText(b.Content, hintStyle: AdaptiveTextStyle.Body);

            if (Browser2.IsHttpUrl(b.ImageUri))
            {
                switch (b.ImageDisplayType)
                {
                    case NotificationBuilder.EImageDisplayType.HeroImage:
                        builder = builder.AddHeroImage(new(b.ImageUri));
                        break;
                    case NotificationBuilder.EImageDisplayType.InlineImage:
                        builder = builder.AddInlineImage(new(b.ImageUri));
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(b.AttributionText))
            {
                builder.AddAttributionText(b.AttributionText);
            }

            if (b.CustomTimeStamp != default)
            {
                builder.AddCustomTimeStamp(b.CustomTimeStamp);
            }

            builder.Show(t =>
            {
                (var tag, var group) = GetTagAndGroup(b.Type);
                t.Tag = tag;
                t.Group = group;
            });
        }

        void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
        {
            var builder = new ToastContentBuilder()
                .AddToastActivationInfo(null, ToastActivationType.Foreground)
                .AddText(title, hintStyle: AdaptiveTextStyle.Header)
                .AddText(text, hintStyle: AdaptiveTextStyle.Body);

            builder.Show(t =>
            {
                (var tag, var group) = GetTagAndGroup(notificationType);
                t.Tag = tag;
                t.Group = group;
            });
        }

        bool INotificationService.IsSupportNotifyDownload => true;

        Progress<float> INotificationService.NotifyDownload(
            Func<string> text,
            NotificationType notificationType,
            string? title)
        {
            title ??= INotificationService.DefaultTitle;

            // https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/toast-progress-bar?tabs=builder-syntax

            // Construct the toast content with data bound fields
            var content = new ToastContentBuilder()
                .AddText(title)
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus"),
                }).GetToastContent();

            (var tag, var group) = GetTagAndGroup(notificationType);

            // Generate the toast notification
            var toast = new ToastNotification(content.GetXml())
            {
                Tag = tag,
                Group = group,
            };

            // Assign initial NotificationData values
            // Values must be of type string
            toast.Data = new NotificationData();
            toast.Data.Values["progressValue"] = "0";
            BindingText(text(), toast.Data);

            static void BindingText(string text, NotificationData data)
            {
                var array = text.Split(new[] { ':', '：' }, StringSplitOptions.RemoveEmptyEntries);
                data.Values["progressValueString"] = array.LastOrDefault() ?? string.Empty;
                data.Values["progressStatus"] = array.FirstOrDefault() ?? "Downloading...";
            }

            // Show the toast notification to the user
            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

            var updateResult = NotificationUpdateResult.Succeeded;
            void Handler(float current)
            {
                if (updateResult != NotificationUpdateResult.Succeeded) return;
                if (current >= CC.MaxProgress)
                {
                    ToastNotificationManagerCompat.History.Remove(tag, group);
                }
                else
                {
                    // Create NotificationData and make sure the sequence number is incremented
                    // since last update, or assign 0 for updating regardless of order
                    var data = new NotificationData();
                    data.Values["progressValue"] = (current / CC.MaxProgress).ToString("0.00");
                    BindingText(text(), data);

                    // Update the existing notification's data by using tag/group
                    // Update 方法返回枚举 NotificationUpdateResult，用于告知是更新成功还是
                    // 找不到通知（这意味着用户可能已消除通知，应停止向其发送更新）。 在进度操作
                    // 完成之前（如在下载完成前），不建议弹出另一个 Toast。
                    updateResult = ToastNotificationManagerCompat.CreateToastNotifier().Update(data, tag, group);
                }
            }

            return new Progress<float>(Handler);
        }
    }
}
