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
    [Register(JavaPackageConstants.Activities + nameof(ExportAuthActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ExportAuthActivity : BaseActivity<activity_export_auth>
    {
        protected override int? LayoutResource => Resource.Layout.activity_export_auth;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding != null)
                {
                    binding.tvExportFilePathLabel.Text = LocalAuth_ExportAuth_ExportPath + "：";
                    binding.swExportEncryption.Text = LocalAuth_ExportAuth_EncryptionExport;
                    binding.tvExportEncryptionDesc.Text = LocalAuth_ExportAuth_ExportPassword;
                    binding.layoutPassword.Hint = LocalAuth_ProtectionAuth_Password;
                    binding.layoutPassword2.Hint = LocalAuth_ProtectionAuth_VerifyPassword;
                    binding.btnExport.Text = LocalAuth_ExportAuth_ConfirmExport;
                }
            }).AddTo(this);
        }
    }
}