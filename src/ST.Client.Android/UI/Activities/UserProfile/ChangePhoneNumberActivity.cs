using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace System.Application.UI.Activities
{
    [Authorize]
    [Register(JavaPackageConstants.Activities + nameof(ChangePhoneNumberActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ChangePhoneNumberActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);
        }
    }
}
