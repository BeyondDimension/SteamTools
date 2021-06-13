using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.AppCompat.App;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 页面 - 启动屏幕
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(SplashActivity))]
    [Activity(Theme = "@style/MainTheme.Splash",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges,
        NoHistory = true)]
    public sealed class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (!MainApplication.IsAllowStart(this)) return;
            StartActivity(new Intent(this, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
            GoToPlatformPages.MockHomePressed(this);
            return;
        }
    }
}