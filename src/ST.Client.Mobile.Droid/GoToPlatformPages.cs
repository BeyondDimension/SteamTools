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

        public static void StartActivity<TActivity>(this Activity activity) where TActivity : Activity
        {
            var intent = new Intent(activity, typeof(TActivity));
            activity.StartActivity(intent);
        }

        public static void StartActivity<TActivity>(this Fragment fragment) where TActivity : Activity
        {
            var intent = new Intent(fragment.Activity, typeof(TActivity));
            fragment.StartActivity(intent);
        }
    }
}