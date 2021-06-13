using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Microsoft.Identity.Client;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(LoginOrRegisterActivity))]
    [Activity(Theme = "@style/MainTheme",
          LaunchMode = LaunchMode.SingleTask,
          ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class LoginOrRegisterActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Return control to MSAL
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }
    }
}