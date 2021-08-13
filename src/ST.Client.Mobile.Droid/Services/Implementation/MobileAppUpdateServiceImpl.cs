#if __ANDROID__
using Android.OS;
#endif
using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Windows;
using XEPlatform = Xamarin.Essentials.Platform;
using static System.Application.UI.Resx.AppResources;
using CC = System.Common.Constants;

namespace System.Application.Services.Implementation
{
    internal sealed partial class MobileAppUpdateServiceImpl : AppUpdateServiceImpl
    {
        readonly INotificationService notification;
        public MobileAppUpdateServiceImpl(INotificationService notification, IToast toast, ICloudServiceClient client, IOptions<AppSettings> options) : base(toast, client, options)
        {
            this.notification = notification;
        }

        protected override Version OSVersion =>
#if __ANDROID__
            new((int)Build.VERSION.SdkInt, 0);
#else
            Xamarin.Essentials.DeviceInfo.Version;
#endif

        protected override async void OnExistNewVersion()
        {
            var result = await MessageBoxCompat.ShowAsync(NewVersionInfoDesc, UpdateContent, MessageBoxButtonCompat.OKCancel);
            if (result == MessageBoxResultCompat.OK)
            {
                StartUpdateCommand.Invoke();
            }
        }

        protected override void OverwriteUpgrade(string value, bool isIncrement, AppDownloadType downloadType = AppDownloadType.Install)
        {
#if __ANDROID__
            if (isIncrement) // 增量更新
            {
                throw new NotImplementedException();
            }
            else // 全量更新
            {
                GoToPlatformPages.InstallApk(XEPlatform.CurrentActivity, value);
            }
#else
            throw new NotImplementedException();
#endif
        }

        IProgress<float>? progress;
        protected override void OnReport(float value, string str)
        {
            base.OnReport(value, str);
            if (value == 0)
            {
                progress?.Report(CC.MaxProgress);
                progress = notification.NotifyDownload(() => ProgressString,
                    NotificationType.NewVersionDownloadProgress);
            }
            else if (value == CC.MaxProgress)
            {
                progress?.Report(CC.MaxProgress);
                progress = null;
            }
            else
            {
                progress?.Report(value);
            }
        }
    }
}