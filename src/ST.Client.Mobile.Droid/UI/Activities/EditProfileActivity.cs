using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(EditProfileActivity))]
    [Activity(Theme = "@style/MainTheme",
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class EditProfileActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();
    }
}
