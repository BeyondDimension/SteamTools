using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;
using System.Application.Security;
using System.Application.UI.Activities;
using System.Properties;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 页面 - 启动屏幕
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(SplashActivity))]
    [Activity(Theme = "@style/MainTheme.Splash", MainLauncher = true, NoHistory = true)]
    public sealed class SplashActivity : AppCompatActivity
    {
        internal static bool AllowStart { get; private set; }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AllowStart = DeviceSecurityCheckUtil.IsSupported(ThisAssembly.Debuggable);
            if (!this.IsAllowStart()) return;

            StartActivity(new Intent(this, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
        }
    }
}

namespace System
{
    public static partial class ActivityExtensions
    {
        public static bool IsAllowStart(this Activity activity)
        {
            if (!SplashActivity.AllowStart)
            {
                activity.Finish();
                Java.Lang.JavaSystem.Exit(0);
                return false;
            }
            return true;
        }
    }
}