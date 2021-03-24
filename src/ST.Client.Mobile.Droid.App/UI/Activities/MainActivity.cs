using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI.Activities
{
    [Register("net.steampp.app.ui.activities." + nameof(MainActivity))]
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            XEPlatform.Init(this, savedInstanceState);
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