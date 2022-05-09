using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using System.Application.UI.ViewModels;
using System.Application.UI.Fragments;
using Android.Views;
using System.Application.Services;
using System.Security.Cryptography;
using System.Application.Services.Implementation;
using _ThisAssembly = System.Properties.ThisAssembly;
using RESX = System.Application.UI.Resx.AppResources;
using RS = System.Application.Resource.String;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// CA 证书状态
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(CACertStatusActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
      LaunchMode = LaunchMode.SingleTask,
      ConfigurationChanges = ManifestConstants.ConfigurationChangesWithOutOrientationLocale)]
    internal sealed class CACertStatusActivity : BaseActivity<activity_ca_cert_status, CommunityProxyPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_ca_cert_status;

        protected override CommunityProxyPageViewModel? OnCreateViewModel()
        {
            return IViewModelManager.Instance.GetMainPageViewModel<CommunityProxyPageViewModel>();
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            _ = ViewModel!.CerFilePath; // 当证书文件不存在时将生成

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            // com.adguard.android.ui.CertificateStatusActivity
            Title = RESX.CommunityFix_CertificateStatus;

            binding.tvTitle.Text = GetString(RS.ca_cert_title).Format(_ThisAssembly.AssemblyTrademark);

            binding.tvSubject.Text = ViewModel!.GetSubject();
            binding.tvSerialNumber.Text = ViewModel.GetSerialNumber();
            binding.tvPeriodValidity.Text = ViewModel.GetPeriodValidity();
            binding.tvSHA256.Text = ViewModel.GetCertHashString(HashAlgorithmName.SHA256);
            binding.tvSHA1.Text = ViewModel.GetCertHashString(HashAlgorithmName.SHA1);

            SetOnClickListener(binding.btnExport, binding.btnDelete, binding.btnOpenSettingsSecurity);
        }

        bool isInstallOrExport;

        protected override void OnResume()
        {
            base.OnResume();
            if (binding == null) return;

            var cert = ViewModel?.RootCertificate;
            if (cert == null) return;

            var value = AndroidPlatformServiceImpl.IsCertificateInstalled(cert);

            if (value.HasValue)
            {
                binding.tvCaState.Text = GetString(value.Value ? RS.ca_cert_status_installed_system : RS.ca_cert_status_installed_user);
                binding.tvCaState.Visibility = ViewStates.Visible;

                if (!value.Value)
                    binding.tvTip.Text = GetString(RS.ca_cert_status_installed_user_tip);
                binding.tvTip.Visibility = value.Value ? ViewStates.Gone : ViewStates.Visible;

                binding.btnExport.Text = RESX.CommunityFix_CertificateExport;
                binding.btnExport.Visibility = ViewStates.Visible;

                binding.btnOpenSettingsSecurity.Text = RESX.CommunityFix_GoToSystemSecuritySettings;
                binding.btnOpenSettingsSecurity.Visibility = ViewStates.Visible;

                binding.btnDelete.Text = RESX.Delete;
                binding.btnDelete.Visibility = ViewStates.Visible;

                isInstallOrExport = false;
            }
            else
            {
                binding.btnExport.Text = RESX.CommunityFix_SetupCertificate;
                binding.btnExport.Visibility = ViewStates.Visible;

                binding.btnOpenSettingsSecurity.Visibility = ViewStates.Gone;
                binding.tvCaState.Visibility = ViewStates.Gone;
                binding.tvTip.Visibility = ViewStates.Gone;
                binding.btnDelete.Visibility = ViewStates.Gone;

                isInstallOrExport = true;
            }
        }

        protected override void OnClick(View view)
        {
            base.OnClick(view);
            if (view.Id == Resource.Id.btnExport)
            {
                if (isInstallOrExport)
                {
                    GoToPlatformPages.StartActivity<GuideCACertActivity>(this);
                }
                else
                {
                    ViewModel!.ExportCertificateFile();
                }
            }
            else if (view.Id == Resource.Id.btnDelete)
            {
                CommunityFixFragment.UninstallCertificateShowTips(this);
            }
            else if (view.Id == Resource.Id.btnOpenSettingsSecurity)
            {
                GoToPlatformPages.SystemSettingsSecurity(this);
            }
        }
    }
}
