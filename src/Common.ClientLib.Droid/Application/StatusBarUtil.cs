using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using AndroidApplication = Android.App.Application;

namespace System.Application
{
    /// <summary>
    /// 状态栏工具类
    /// </summary>
    public static class StatusBarUtil
    {
        static int GetStatusBarHeight(Context context)
        {
            var resources = context.Resources;
            if (resources == null)
                throw new NullReferenceException($"Context.Resources is null, Context: {context}");
            int resourceId = resources.GetIdentifier("status_bar_height", "dimen", "android");
            return resources.GetDimensionPixelSize(resourceId);
        }

        /// <summary>
        /// 获取状态栏高度
        /// </summary>
        public static int StatusBarHeight => GetStatusBarHeight(AndroidApplication.Context);

        #region 亮色状态栏 WindowLightStatusBar

        const int SYSTEM_UI_FLAG_LIGHT_STATUS_BAR = (int)SystemUiFlags.LightStatusBar;

        /// <summary>
        /// 设置状态栏是否为亮色风格(图标背景为黑灰色)
        /// 仅 6.0 (M) 以上生效
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="isLightStatusBar"></param>
        public static void SetWindowLightStatusBar(Activity activity, bool isLightStatusBar)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var decorView = activity.Window?.DecorView;
                if (decorView != null)
                {
                    var oldValue = (int)decorView.SystemUiVisibility;
                    var value = oldValue;
                    if (isLightStatusBar)
                    {
                        value |= SYSTEM_UI_FLAG_LIGHT_STATUS_BAR;
                    }
                    else
                    {
                        value &= ~SYSTEM_UI_FLAG_LIGHT_STATUS_BAR;
                    }
                    if (value != oldValue)
                    {
                        decorView.SystemUiVisibility = (StatusBarVisibility)value;
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 初始化用于沉浸式状态栏的 View
        /// </summary>
        /// <param name="fakeStatusBar"></param>
        public static void InitImmersiveStatusBar(View fakeStatusBar)
        {
            int status_bar_height = StatusBarHeight;
            var layoutParams = fakeStatusBar.LayoutParameters;
            if (layoutParams != null)
            {
                layoutParams.Height = status_bar_height;
                fakeStatusBar.LayoutParameters = layoutParams;
            }
        }

        static void ClearFitsSystemWindows(View view)
        {
            view.SetFitsSystemWindows(false);
            view.SetPadding(0, 0, 0, 0);
        }

        /// <summary>
        /// 初始化用于沉浸式状态栏的 Activity
        /// </summary>
        /// <param name="activity"></param>
        public static void InitImmersiveStatusBar(Activity activity)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var window = activity.Window;
                if (window != null)
                {
                    var view = ((ViewGroup)window.DecorView).GetChildAt(0);
                    if (view == null) return;
                    ClearFitsSystemWindows(view);
                    view = view.FindViewById(Android.Resource.Id.Content);
                    if (view == null) return;
                    var viewParent = view.Parent;
                    if (viewParent == null) return;
                    var viewGroup = (ViewGroup)viewParent;
                    if (viewGroup == null) return;
                    ClearFitsSystemWindows(viewGroup);
                    view = ((ViewGroup)view).GetChildAt(0);
                    if (view == null) return;
                    ClearFitsSystemWindows(view);
                }
            }
        }
    }
}