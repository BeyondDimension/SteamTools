#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="INotificationService"/>
sealed class Windows10NotificationServiceImpl : INotificationService, INotificationService.ILifeCycle
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

    //void INotificationService.Notify(NotificationBuilder.IInterface b)
    //{
    //    var builder = new ToastContentBuilder()
    //        .AddToastActivationInfo(null, ToastActivationType.Foreground)
    //        .AddText(b.Title, hintStyle: AdaptiveTextStyle.Header)
    //        .AddText(b.Content, hintStyle: AdaptiveTextStyle.Body);

    //    if (String2.IsHttpUrl(b.ImageUri))
    //    {
    //        switch (b.ImageDisplayType)
    //        {
    //            case ImageDisplayType.HeroImage:
    //                builder.AddHeroImage(new(b.ImageUri));
    //                break;
    //            case ImageDisplayType.InlineImage:
    //                builder.AddInlineImage(new(b.ImageUri));
    //                break;
    //        }
    //    }

    //    if (!string.IsNullOrWhiteSpace(b.AttributionText))
    //    {
    //        builder.AddAttributionText(b.AttributionText);
    //    }

    //    if (b.CustomTimeStamp != default)
    //    {
    //        builder.Content.DisplayTimestamp = b.CustomTimeStamp;
    //    }

    //    builder.Show(t =>
    //    {
    //        (var tag, var group) = GetTagAndGroup(b.Type);
    //        t.Tag = tag;
    //        t.Group = group;
    //        t.Activated += (_, _) =>
    //        {
    //            if (b.Click != null)
    //            {
    //                switch (b.Click.Entrance)
    //                {
    //                    case Entrance.Main:
    //                        break;
    //                    case Entrance.Browser:
    //                        Browser2.Open(b.Click.RequestUri);
    //                        break;
    //                    case Entrance.Delegate:
    //                        b.Click.Action?.Invoke();
    //                        break;
    //                }
    //            }

    //            if (b.AutoCancel)
    //            {
    //                ToastNotificationManagerCompat.History.Remove(tag, group);
    //            }
    //        };
    //    });
    //}

    void INotificationService.Notify(string text, NotificationType notificationType, bool autoCancel, string? title, Entrance entrance, string? requestUri)
    {
        try
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
                t.Activated += (_, _) =>
                {
                    if (autoCancel)
                    {
                        ToastNotificationManagerCompat.History.Remove(tag, group);
                    }
                };
            });
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
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
            var progressValueString = array.LastOrDefault() ?? string.Empty;
            var progressStatus = array.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(progressStatus) || progressStatus == progressValueString)
                progressStatus = "Downloading...";
            data.Values["progressValueString"] = progressValueString;
            data.Values["progressStatus"] = progressStatus;
        }

        // Show the toast notification to the user
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);

        var updateResult = NotificationUpdateResult.Succeeded;
        void Handler(float current)
        {
            if (updateResult != NotificationUpdateResult.Succeeded) return;
            if (current >= 100f)
            {
                ToastNotificationManagerCompat.History.Remove(tag, group);
            }
            else
            {
                // Create NotificationData and make sure the sequence number is incremented
                // since last update, or assign 0 for updating regardless of order
                var data = new NotificationData();
                data.Values["progressValue"] = (current / 100f).ToString("0.00");
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

    void INotificationService.ILifeCycle.OnStartup()
    {
        // 桌面(未打包) 应用当前正在运行，也不会调用此，可能与管理员权限相关

        // https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop#step-3-handling-activation
        // Listen to notification activation
        //ToastNotificationManagerCompat.OnActivated += toastArgs =>
        //{
        //    // Obtain the arguments from the notification
        //    ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

        //    // Obtain any user input (text boxes, menu selections) from the notification
        //    var userInput = toastArgs.UserInput;
        //};
    }

    void INotificationService.ILifeCycle.OnShutdown()
    {
        // 如果应用有卸载程序，应在卸载程序中调用 ToastNotificationManagerCompat.Uninstall();。
        // 如果应用是不带安装程序的“可移植应用”，请考虑在应用退出时调用此方法，除非有通知在应用关闭后保留。
        // 卸载方法将清理任何计划通知和当前通知，删除任何关联的注册表值，并删除库创建的任何关联的临时文件。
        if (DesktopBridge.IsRunningAsUwp)
        {
            ToastNotificationManagerCompat.Uninstall();
        }
    }
}
#endif