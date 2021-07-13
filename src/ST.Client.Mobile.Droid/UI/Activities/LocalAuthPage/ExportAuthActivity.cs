using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.IO;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.ExportAuthWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ExportAuthActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ExportAuthActivity : BaseActivity<activity_export_auth, ExportAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_export_auth;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var externalPath = GetExternalFilesDir(null)?.CanonicalPath;
            if (string.IsNullOrWhiteSpace(externalPath) || !Directory.Exists(externalPath))
            {
                Toast.Show("error: externalPath is null or not exists.");
                Finish();
                return;
            }

            ViewModel = new();
            ViewModel.AddTo(this);

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

            ViewModel!.WhenAnyValue(x => x.IsPasswordEncrypt).Subscribe(value =>
            {
                if (binding == null) return;
                binding.layoutPassword.Enabled = value;
                binding.layoutPassword2.Enabled = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.Path).Subscribe(value =>
            {
                if (binding == null) return;
                binding.tvExportFilePathValue.Text = value;
            }).AddTo(this);

            binding!.swExportEncryption.CheckedChange += (_, e) =>
            {
                ViewModel!.IsPasswordEncrypt = e.IsChecked;
            };
            binding.tbPassword.TextChanged += (_, _) =>
            {
                ViewModel!.Password = binding.tbPassword.Text;
            };
            binding.tbPassword2.TextChanged += (_, _) =>
            {
                ViewModel!.VerifyPassword = binding.tbPassword2.Text;
            };

            SetOnClickListener(binding.btnExport);

            ViewModel!.Path = Path.Combine(externalPath, DefaultExportAuthDirName, DefaultExportAuthFileName);
        }

        protected override void OnClick(View view)
        {
            if (view.Id == Resource.Id.btnExport)
            {
                ViewModel!.ExportAuth();
                return;
            }
            base.OnClick(view);
        }
    }
}