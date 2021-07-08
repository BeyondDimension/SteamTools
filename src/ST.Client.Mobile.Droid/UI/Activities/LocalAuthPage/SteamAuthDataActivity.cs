using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using System.Linq;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthDataActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthDataActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var vm = MainActivity.Instance?.LocalAuthPageViewModel.Authenticators.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            if (vm == null)
            {
                Finish();
                return;
            }
        }

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<SteamAuthDataActivity, ushort>(activity, authId);
        }
    }
}