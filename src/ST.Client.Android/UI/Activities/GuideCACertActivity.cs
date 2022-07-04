using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Fragments;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static AndroidX.Activity.Result.ActivityResultTask;
using _ThisAssembly = System.Properties.ThisAssembly;
using ASettings = Android.Provider.Settings;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 引导导入证书页面
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(GuideCACertActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
      LaunchMode = LaunchMode.SingleTask,
      ConfigurationChanges = ManifestConstants.ConfigurationChangesWithOutOrientationLocale)]
    internal sealed class GuideCACertActivity : BaseActivity<activity_material_toolbar_base>
    {
        protected override int? LayoutResource => Resource.Layout.activity_guide_ca_cert;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            Title = string.Empty;

            var appBarConfiguration = new AppBarConfiguration.Builder(Resource.Id.navigation_guide_export_ca_cert).Build();
            navController = ((NavHostFragment)SupportFragmentManager.FindFragmentById(Resource.Id.nav_host_fragment)!).NavController;
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);

            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);
        }

        [Register(JavaPackageConstants.Fragments + nameof(GuideExportCACertFragment))]
        internal sealed class GuideExportCACertFragment : BaseFragment<fragment_guide_export_ca_cert, CommunityProxyPageViewModel>
        {
            protected override int? LayoutResource => Resource.Layout.fragment_guide_export_ca_cert;

            protected override CommunityProxyPageViewModel? OnCreateViewModel()
            {
                return IViewModelManager.Instance.GetMainPageViewModel<CommunityProxyPageViewModel>();
            }

            bool canInstalledCACert;

            public override void OnCreateView(View view)
            {
                base.OnCreateView(view);

                binding!.tvTitle.Text = GetString(Resource.String.ca_cert_title).Format(_ThisAssembly.AssemblyTrademark);

                var sdkInt = (int)Build.VERSION.SdkInt;
                canInstalledCACert = !(sdkInt > 29 || (sdkInt == 29 && Build.VERSION.PreviewSdkInt > 0));
                // com.adguard.kit.compatibility.h()
                // com.adguard.android.ui.fragments.https_ca_installation.b()
                // return com.adguard.kit.compatibility.a.h() ? super.b() : R.l.https_ca_installation_guide_install_ca;
                // Android 10+ 只能通过系统设置从存储区导入 CA 证书

                if (canInstalledCACert)
                {
                    binding!.tvDesc3.Visibility = ViewStates.Gone;
                    binding!.btnDone.Text = GetString(Resource.String.guide_export_ca_cert_btn_install);
                }

                SetOnClickListener(binding!.btnDone);
            }

            public override void OnResume()
            {
                base.OnResume();

                if (IReverseProxyService.Instance.CertificateManager.IsRootCertificateInstalled)
                {
                    var intent = IntermediateActivity.SetResult(new(), Result.Ok);
                    var a = RequireActivity();
                    a.SetResult(Result.Ok, intent);
                    a.Finish();
                }
            }

            protected override bool OnClick(View view)
            {
                if (view.Id == Resource.Id.btnDone)
                {
                    if (canInstalledCACert)
                    {
                        CommunityFixFragment.InstallCertificate(RequireContext(), ViewModel!);
                    }
                    else
                    {
                        ExportCertificateFile();
                    }
                    return true;
                }
                return base.OnClick(view);
            }

            async void ExportCertificateFile()
            {
                var isOK = await ViewModel!.ExportCertificateFileAsync();
                if (isOK)
                {
                    await GoToGuideHowInstallCACertPageAsync();
                }
            }

            async Task GoToGuideHowInstallCACertPageAsync()
            {
                int count = 0;
                while (XEPlatform.CurrentActivity != RequireActivity() && count < 10)
                {
                    await Task.Delay(200);
                    count++;
                }
                var navController = this.GetNavController();
                navController?.Navigate(Resource.Id.action_navigation_guide_export_ca_cert_to_navigation_guide_how_install_ca_cert);
            }
        }

        [Register(JavaPackageConstants.Fragments + nameof(GuideHowInstallCACertFragment))]
        internal sealed class GuideHowInstallCACertFragment : BaseFragment<fragment_guide_how_install_ca_cert>
        {
            protected override int? LayoutResource => Resource.Layout.fragment_guide_how_install_ca_cert;

            public override void OnCreateView(View view)
            {
                base.OnCreateView(view);

                SetOnClickListener(binding!.btnDone);
            }

            public override void OnResume()
            {
                base.OnResume();

                if (IReverseProxyService.Instance.CertificateManager.IsRootCertificateInstalled)
                {
                    var intent = IntermediateActivity.SetResult(new(), Result.Ok);
                    var a = RequireActivity();
                    a.SetResult(Result.Ok, intent);
                    a.Finish();
                }
            }

            protected override bool OnClick(View view)
            {
                if (view.Id == Resource.Id.btnDone)
                {
                    GoToPlatformPages.SystemSettingsSecurity(RequireActivity());
                    return true;
                }
                return base.OnClick(view);
            }
        }
    }
}
