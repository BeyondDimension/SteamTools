using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.ViewPager.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(MainActivity2))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    internal sealed class MainActivity2 : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            XEPlatform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}