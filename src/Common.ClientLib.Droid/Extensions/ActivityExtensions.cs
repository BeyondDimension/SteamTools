using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Fragment = AndroidX.Fragment.App.Fragment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="Activity"/> 扩展
    /// </summary>
    public static partial class ActivityExtensions
    {
        #region 显示隐藏软键盘 (Show/Hide)SoftInput

        /// <summary>
        /// 隐藏软键盘
        /// </summary>
        /// <param name="activity"></param>
        public static void HideSoftInput(this Activity activity)
        {
            var windowToken = (activity.CurrentFocus ?? activity.Window?.DecorView)?.WindowToken;
            if (windowToken != null)
            {
                var imm = activity.GetSystemService<InputMethodManager>();
                if (imm != null)
                {
                    imm.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.NotAlways);
                }
            }
        }

        /// <summary>
        /// 显示软键盘
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="editText"></param>
        /// <param name="inOnCreate">是否在<see cref="Activity.OnCreate(Android.OS.Bundle?)"/>中调用</param>
        public static async void ShowSoftInput(this Activity activity, EditText editText, bool inOnCreate = false)
        {
            var imm = activity.GetSystemService<InputMethodManager>();
            if (imm != null)
            {
                if (inOnCreate)
                {
                    const int millisecondsDelay = 99;
                    if (activity.Window != null)
                    {
                        activity.Window.DecorView.PostDelayed(ShowSoftInput, millisecondsDelay);
                    }
                    else
                    {
                        await Task.Delay(millisecondsDelay);
                        MainThread.BeginInvokeOnMainThread(ShowSoftInput);
                    }
                }
                else
                {
                    ShowSoftInput();
                }
                void ShowSoftInput()
                {
                    editText.Focusable = true;
                    editText.RequestFocus();
                    imm.ShowSoftInput(editText, 0);
                }
            }
        }

        #endregion

        public static bool HasValue(this Activity? activity)
        {
            return activity != null && !activity.IsFinishing && !activity.IsDestroyed;
        }

#if DEBUG

        [Obsolete("use HasValue", true)]
        public static bool IsAvailable(this Activity? activity) => activity.HasValue();

#endif

        public static void SetSupportActionBarWithNavigationClick(this AppCompatActivity activity, Toolbar toolbar, bool displayHomeAsUpEnabled = false)
        {
            activity.SetSupportActionBar(toolbar);
            toolbar.NavigationClick += (_, _) => activity.OnBackPressed();
            if (displayHomeAsUpEnabled) activity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        /// <summary>
        /// 设置是否禁止截屏
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="value"></param>
        public static void SetWindowSecure(this Activity activity, bool value)
        {
            if (activity.Window?.Attributes == null) return;
            if (value)
            {
                if (activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.Secure))
                {
                    return;
                }
                else
                {
                    activity.Window.AddFlags(WindowManagerFlags.Secure);
                }
            }
            else
            {
                if (activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.Secure))
                {
                    activity.Window.ClearFlags(WindowManagerFlags.Secure);
                }
                else
                {
                    return;
                }
            }
        }
    }
}