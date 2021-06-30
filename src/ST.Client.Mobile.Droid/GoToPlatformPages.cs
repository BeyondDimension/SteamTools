using Android.App;
using Android.Content;
using Fragment = AndroidX.Fragment.App.Fragment;

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

        public static void StartActivity(this Activity activity, Type activityType)
        {
            var intent = new Intent(activity, activityType);
            activity.StartActivity(intent);
        }

        public static void StartActivity<TActivity>(this Activity activity) where TActivity : Activity
            => StartActivity(activity, typeof(TActivity));

        public static void StartActivity(this Fragment fragment, Type activityType)
        {
            var activity = fragment.Activity;
            if (activity == null) return;
            StartActivity(activity, activityType);
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
                    catch
                    {
                    }
                }
            }
            return default;
        }

        public static void StartActivity(Activity activity, Type activityType, object viewModel, Type? viewModelType = null)
        {
            viewModelType ??= viewModel.GetType();
            var viewModel_ = Serializable.SMP(viewModelType, viewModel);
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
    }
}