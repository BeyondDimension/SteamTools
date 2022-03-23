using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.IO;
using static System.Application.UI.Resx.AppResources;
using static System.Application.UI.ViewModels.ExportAuthWindowViewModel;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ExportAuthActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ExportAuthActivity : BaseActivity<activity_export_auth, ExportAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_export_auth;

        protected override ExportAuthWindowViewModel? OnCreateViewModel()
        {
            var selectId = this.GetViewModel<ushort>();
            return new() { SelectId = selectId };
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            if (!GeneralSettings.CaptureScreen.Value)
            {
                this.SetWindowSecure(true);
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);


            R.Subscribe(() =>
            {
                Title = ViewModel!.Title;
                if (binding == null) return;
                binding.swExportEncryption.Text = LocalAuth_ExportAuth_EncryptionExport;
                binding.tvExportEncryptionDesc.Text = LocalAuth_ExportAuth_ExportPassword;
                binding.layoutPassword.Hint = LocalAuth_ProtectionAuth_Password;
                binding.layoutPassword2.Hint = LocalAuth_ProtectionAuth_VerifyPassword;
                binding.btnExport.Text = LocalAuth_ExportAuth_ConfirmExport;
                binding.swExportQRCode.Text = LocalAuth_ExportAuth_ToQRCode;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsPasswordEncrypt).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.layoutPassword.Enabled = value && !ViewModel!.IsExportQRCode;
                binding.layoutPassword2.Enabled = value && !ViewModel!.IsExportQRCode;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsExportQRCode).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.swExportEncryption.Enabled = !value;
                binding.layoutPassword.Enabled = ViewModel!.IsPasswordEncrypt && !value;
                binding.layoutPassword2.Enabled = ViewModel!.IsPasswordEncrypt && !value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.QRCode).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var hasValue = value != null;
                binding.ivQRCode.Visibility = !hasValue ? ViewStates.Gone : ViewStates.Visible;
                int imgSize;
                if (value != null)
                {
                    imgSize = Resources!.DisplayMetrics!.WidthPixels;
                    imgSize -= Resources.GetDimensionPixelSize(Resource.Dimension.activity_horizontal_margin) * 2;
                }
                else
                {
                    imgSize = default;
                }
#pragma warning disable CS0618 // 类型或成员已过时
                binding.ivQRCode.SetImageSource(value, targetW: imgSize, inPreferredConfig: Bitmap.Config.Argb4444);
#pragma warning restore CS0618 // 类型或成员已过时
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsExporting).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnExport.Enabled = !value;
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
            binding.swExportQRCode.CheckedChange += (_, e) =>
            {
                ViewModel!.IsExportQRCode = e.IsChecked;
            };

            SetOnClickListener(binding.btnExport);
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

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<ExportAuthActivity, ushort>(activity, authId);
        }
    }
}