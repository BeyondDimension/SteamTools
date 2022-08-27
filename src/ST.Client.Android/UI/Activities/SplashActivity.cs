#if !NET6_0_MAUI_LIB
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using System.Application.Security;

namespace System.Application.UI.Activities;

/// <summary>
/// 页面 - 启动屏幕
/// </summary>
[Register(JavaPackageConstants.Activities + nameof(SplashActivity))]
[Activity(Theme = ManifestConstants.MainTheme2_Splash,
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTask,
    ConfigurationChanges = ManifestConstants.ConfigurationChanges,
    NoHistory = true)]
internal sealed class SplashActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        //if (!DeviceSecurityCheckUtil.IsAllowStart(this)) return;
        StartActivity(IAndroidApplication.Instance.MainActivityType);
        OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
    }

    public override void OnBackPressed()
    {
        GoToPlatformPages.MockHomePressed(this);
        return;
    }
}
#endif