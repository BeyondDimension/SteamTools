#if __ANDROID__
using Android.OS;
#endif
using Microsoft.Extensions.Options;
using System.Application.Models;
using XEPlatform = Xamarin.Essentials.Platform;
using System.Application.UI;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IApplicationUpdateService"/>
    internal sealed partial class ApplicationUpdateServiceImpl : ApplicationUpdateServiceBaseImpl
    {
        public ApplicationUpdateServiceImpl(
            IApplication application,
            INotificationService notification,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options) : base(application, notification, toast, client, options)
        {

        }

#if __ANDROID__ && IS_STORE_PACKAGE // 渠道包不支持从服务器分发，仅通过应用商店分发
        public override bool IsSupportedServerDistribution => false;
#endif

        protected override Version OSVersion =>
#if __ANDROID__
            new((int)Build.VERSION.SdkInt, 0, 0);
#else
            Xamarin.Essentials.DeviceInfo.Version;
#endif

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
    }
}