using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using AndroidX.Navigation.Fragment;
using AndroidX.Navigation.UI;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Application.UI.Fragments;
using System.Text;
using Android.Views;
using System.Application.Services;
using ASettings = Android.Provider.Settings;
using static AndroidX.Activity.Result.ActivityResultTask;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 引导导入证书页面
    /// </summary>
    [Register(JavaPackageConstants.Activities + nameof(GuideCACertActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
      LaunchMode = LaunchMode.SingleTask,
      ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class GuideCACertActivity : BaseActivity<activity_guide_ca_cert>
    {
        protected override int? LayoutResource => Resource.Layout.activity_guide_ca_cert;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            var appBarConfiguration = new AppBarConfiguration.Builder(Resource.Id.navigation_guide_export_ca_cert).Build();
            navController = ((NavHostFragment)SupportFragmentManager.FindFragmentById(Resource.Id.nav_host_fragment)).NavController;
            NavigationUI.SetupActionBarWithNavController(this, navController, appBarConfiguration);
        }

        [Register(JavaPackageConstants.Fragments + nameof(GuideExportCACertFragment))]
        internal sealed class GuideExportCACertFragment : BaseFragment<fragment_guide_export_ca_cert, CommunityProxyPageViewModel>
        {
            protected override int? LayoutResource => Resource.Layout.fragment_guide_export_ca_cert;

            protected override CommunityProxyPageViewModel? OnCreateViewModel()
            {
                return IViewModelManager.Instance.GetMainPageViewModel<CommunityProxyPageViewModel>();
            }

            public override void OnCreateView(View view)
            {
                base.OnCreateView(view);

                SetOnClickListener(binding!.btnDone);
            }

            protected override bool OnClick(View view)
            {
                if (view.Id == Resource.Id.btnDone)
                {
                    ExportCertificateFile();
                    return true;
                }
                return base.OnClick(view);
            }

            async void ExportCertificateFile()
            {
                var isOK = await ViewModel!.ExportCertificateFileAsync();
                if (isOK)
                {
                    GoToGuideHowInstallCACertPage();
                }
            }

            void GoToGuideHowInstallCACertPage()
            {
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

            protected override bool OnClick(View view)
            {
                if (view.Id == Resource.Id.btnDone)
                {
                    GoToSystemSettingsSecurity();
                    return true;
                }
                return base.OnClick(view);
            }

            async void GoToSystemSettingsSecurity()
            {
                var intent = new Intent(ASettings.ActionSecuritySettings);
                intent.AddFlags(ActivityFlags.NewTask);
                intent = await IntermediateActivity.StartAsync(intent, requestCodeVpnService);
                var a = RequireActivity();
                if (IHttpProxyService.Instance.IsCurrentCertificateInstalled)
                {
                    intent = IntermediateActivity.SetResult(intent, Result.Ok);
                    a.SetResult(Result.Ok, intent);
                }
                a.Finish();
            }
        }
    }
}
