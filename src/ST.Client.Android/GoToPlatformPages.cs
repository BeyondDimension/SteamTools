using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using AndroidUri = Android.Net.Uri;
using Fragment = AndroidX.Fragment.App.Fragment;
using JFile = Java.IO.File;
using ASettings = Android.Provider.Settings;
using Android.Security;
using System.IO;

namespace System.Application
{
    /// <summary>
    /// 页面跳转到平台特定操作系统中的某些页面，例如当前APP设置页
    /// </summary>
    public static class GoToPlatformPages
    {
        /// <summary>
        /// 模拟 Home 键按下效果，回到主页
        /// </summary>
        /// <param name="context"></param>
        public static void MockHomePressed(Context context)
        {
            try
            {
                var intent = new Intent(Intent.ActionMain)
                    .SetFlags(ActivityFlags.NewTask)
                    .AddCategory(Intent.CategoryHome);
                context.StartActivity(intent);
            }
            catch
            {
            }
        }

        public static string GetAuthority(Context context)
        {
            var packageName = context.ApplicationContext!.PackageName;
            var value = packageName + ".fileProvider";
            return value;
        }

        public static void StartActivity<TActivity>(this Activity activity) where TActivity : Activity
            => activity.StartActivity(typeof(TActivity));

        public static void StartActivity(this Fragment fragment, Type activityType)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            activity.StartActivity(activityType);
        }

        public static void StartActivity<TActivity>(this Fragment fragment) where TActivity : Activity
            => StartActivity(fragment, typeof(TActivity));

        const string KEY_ViewModel = "ByteArrayViewModel";
        static void StartActivity(this Activity activity, Type activityType, byte[] viewModel)
        {
            var intent = new Intent(activity, activityType);
            intent.PutExtra(KEY_ViewModel, viewModel);
            activity.StartActivity(intent);
        }

        public static TViewModel? GetViewModel<TViewModel>(this Activity activity)
        {
            var intent = activity.Intent;
            if (intent != null)
            {
                var byteArray = intent.GetByteArrayExtra(KEY_ViewModel);
                if (byteArray != null)
                {
                    try
                    {
                        var value = Serializable.DMP<TViewModel>(byteArray);
                        return value;
                    }
                    catch (Exception e)
                    {
                        Log.Warn(nameof(GoToPlatformPages), e, "GetViewModel fail, type: {0}, activity: {1}", typeof(TViewModel), activity.GetType());
                    }
                }
            }
            return default;
        }

        public static void StartActivity(Activity activity, Type activityType, object viewModel, Type? viewModelType = null)
        {
            viewModelType ??= viewModel.GetType();
            var viewModel_ = Serializable.SMP(viewModelType, viewModel);
#if DEBUG
            var t = Serializable.DMP(viewModelType, viewModel_);
#endif
            StartActivity(activity, activityType, viewModel_);
        }

        public static void StartActivity(Fragment fragment, Type activityType, object viewModel, Type? viewModelType = null)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            StartActivity(activity, activityType, viewModel, viewModelType);
        }

        public static void StartActivity<TActivity>(Activity activity, object viewModel, Type? viewModelType = null)
        {
            StartActivity(activity, typeof(TActivity), viewModel, viewModelType);
        }

        public static void StartActivity<TActivity>(Fragment fragment, object viewModel, Type? viewModelType = null)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            StartActivity<TActivity>(activity, viewModel, viewModelType);
        }

        public static void StartActivity<TViewModel>(Activity activity, Type activityType, TViewModel viewModel)
        {
            var viewModel_ = Serializable.SMP(viewModel);
#if DEBUG
            var t = Serializable.DMP(typeof(TViewModel), viewModel_);
#endif
            StartActivity(activity, activityType, viewModel_);
        }

        public static void StartActivity<TViewModel>(Fragment fragment, Type activityType, TViewModel viewModel)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            StartActivity(activity, activityType, viewModel);
        }

        public static void StartActivity<TActivity, TViewModel>(Activity activity, TViewModel viewModel)
        {
            StartActivity(activity, typeof(TActivity), viewModel);
        }

        public static void StartActivity<TActivity, TViewModel>(Fragment fragment, TViewModel viewModel)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            StartActivity<TActivity, TViewModel>(activity, viewModel);
        }

        public static bool OpenFile(Context context, JFile jFile, string mime, string? name = null)
        {
            try
            {
                var sdkInt = (int)Build.VERSION.SdkInt;
                var intent = new Intent(Intent.ActionView);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    intent.PutExtra("name", name);
                }
                AndroidUri apkUri;
                if (sdkInt >= (int)BuildVersionCodes.N) // 7.0 FileProvider
                {
                    apkUri = FileProvider.GetUriForFile(context, GetAuthority(context), jFile);
                    intent.AddFlags(ActivityFlags.GrantReadUriPermission); // FLAG_GRANT_READ_URI_PERMISSION 添加这一句表示对目标应用临时授权该Uri所代表的文件
                }
                else
                {
                    apkUri = AndroidUri.FromFile(jFile)!;
                }
                intent.SetDataAndType(apkUri, mime);
                intent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool InstallApk(Context context, JFile apkFile) => OpenFile(context, apkFile, MediaTypeNames.APK);

        public static bool InstallApk(Context context, string apkFilePath)
        {
            JFile apkFile = new(apkFilePath);
            return InstallApk(context, apkFile);
        }

        public static bool InstallCertificate(Context context, string filePath, string name)
        {
            try
            {
                // com.adguard.android.service.q(HttpsFilteringServiceImpl).f()
                var intent = KeyChain.CreateInstallIntent();
                intent.PutExtra("name", name);
                intent.PutExtra("CERT", File.ReadAllBytes(filePath));
                intent.AddFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
                return true;
            }
            catch
            {
                return false;
            }
        }

        static void StartNewTaskActivity(Context context, string action)
        {
            var intent = new Intent(action);
            intent.AddFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }

        public static void SystemSettingsSecurity(Context context)
            => StartNewTaskActivity(context, ASettings.ActionSecuritySettings);

        public static void SystemSettings(Context context)
            => StartNewTaskActivity(context, ASettings.ActionSettings);

        public static void AppDetailsSettings(Context context)
        {
            try
            {
                var intent = new Intent();
                intent.AddFlags(ActivityFlags.NewTask);
                AppDetailsSettings(context, intent);
                context.StartActivity(intent);
            }
            catch
            {
                SystemSettings(context);
            }
        }

        static void AppDetailsSettings(Context context, Intent intent, int? sdkInt_ = null)
        {
            var sdkInt = sdkInt_ ?? (int)Build.VERSION.SdkInt;
            if (sdkInt >= 9)
            {
                intent.SetAction(ASettings.ActionApplicationDetailsSettings);
                intent.AddCategory(Intent.CategoryDefault);
                intent.SetData(AndroidUri.FromParts("package", context.PackageName, null));
            }
            else if (sdkInt <= 8)
            {
                intent.SetAction(Intent.ActionView);
                intent.SetClassName("com.android.settings", "com.android.settings.InstalledAppDetails");
                intent.PutExtra("com.android.settings.ApplicationPkgName", context.PackageName);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public static void NotificationSettings(Context context)
        {
            // https://blog.csdn.net/LikeBoke/article/details/83929711
            // https://www.jianshu.com/p/1e27efb1dcac
            var isAppDetailsSettings = false;
            try
            {
                var intent = new Intent();
                var sdkInt = (int)Build.VERSION.SdkInt;
                if (sdkInt >= (int)BuildVersionCodes.O) // >=8.0
                {
                    // APP通知设置详情
                    intent.SetAction(ASettings.ActionAppNotificationSettings); // ACTION_APP_NOTIFICATION_SETTINGS
                    intent.PutExtra(ASettings.ExtraAppPackage, context.PackageName);
                    intent.PutExtra(ASettings.ExtraChannelId, context.ApplicationInfo!.Uid);
                }
                else if (sdkInt >= (int)BuildVersionCodes.Lollipop && sdkInt < (int)BuildVersionCodes.O) // >=5.0 && <8.0
                {
                    // APP通知设置详情
                    intent.SetAction(ASettings.ActionAppNotificationSettings); // ACTION_APP_NOTIFICATION_SETTINGS
                    intent.PutExtra("app_package", context.PackageName);
                    intent.PutExtra("app_uid", context.ApplicationInfo!.Uid);
                }
                else
                {
                    isAppDetailsSettings = true;
                    AppDetailsSettings(context, intent, sdkInt);
                }
                context.StartActivity(intent);
            }
            catch
            {
                if (isAppDetailsSettings)
                {
                    SystemSettings(context);
                }
                else
                {
                    AppDetailsSettings(context);
                }
            }
        }
    }
}