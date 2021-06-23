using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(UserProfileActivity))]
    [Activity(Theme = ManifestConstants.MainTheme,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class UserProfileActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();
    }
}
