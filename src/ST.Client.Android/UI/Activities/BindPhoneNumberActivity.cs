using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(BindPhoneNumberActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class BindPhoneNumberActivity : BaseActivity
    {
        protected override int? LayoutResource => throw new NotImplementedException();
    }
}
