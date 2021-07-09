using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(EncryptionAuthActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class EncryptionAuthActivity : BaseActivity<activity_encryption_auth>
    {
        protected override int? LayoutResource => Resource.Layout.activity_encryption_auth;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding != null)
                {
                    binding.tvProtectionAuthInfo.Text = LocalAuth_ProtectionAuth_Info;
                    binding.swEncryption.Text = LocalAuth_ProtectionAuth_EnablePassword;
                    binding.tvEncryptionDesc.Text = LocalAuth_ProtectionAuth_EnablePasswordTip;
                    binding.layoutPassword.Hint = LocalAuth_ProtectionAuth_Password;
                    binding.layoutPassword2.Hint = LocalAuth_ProtectionAuth_VerifyPassword;
                    binding.btnSave.Text = LocalAuth_ProtectionAuth_SaveApply;
                }
            }).AddTo(this);
        }
    }
}