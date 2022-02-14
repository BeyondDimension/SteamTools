using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class CommunityProxyPageViewModel
    {
        readonly IHostsFileService? hostsFileService;
        readonly IPlatformService platformService = IPlatformService.Instance;

        public ReactiveCommand<Unit, Unit>? SetupCertificateCommand { get; }

        public ReactiveCommand<Unit, Unit>? DeleteCertificateCommand { get; }

        public ReactiveCommand<Unit, Unit>? OpenCertificateDirCommand { get; }

        public ReactiveCommand<Unit, Unit>? EditHostsFileCommand { get; }

        public ReactiveCommand<Unit, Unit>? OpenHostsDirCommand { get; }

        public ReactiveCommand<Unit, Unit>? ResetHostsFileCommand { get; }

        public ReactiveCommand<Unit, Unit>? NetworkFixCommand { get; }

        //public ReactiveCommand<Unit, Unit> AutoRunProxyCommand { get; }

        public ReactiveCommand<Unit, Unit>? ProxySettingsCommand { get; }

        //public ReactiveCommand<Unit, Unit> EnableProxyScriptCommand { get; }

        //public MenuItemViewModel AutoRunProxy { get; }

        //public MenuItemViewModel EnableProxyScript { get; }

        public ReactiveCommand<Unit, Unit>? RefreshCommand { get; }

        public ReactiveCommand<Unit, Unit>? TrustCerCommand { get; }

        protected readonly IHttpProxyService httpProxyService = IHttpProxyService.Instance;

        public CommunityProxyPageViewModel()
        {
            IconKey = nameof(CommunityProxyPageViewModel);

            if (IApplication.IsDesktopPlatform)
            {
                hostsFileService = IHostsFileService.Instance;
                SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
                DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
                EditHostsFileCommand = ReactiveCommand.Create(hostsFileService.OpenFile);
                OpenHostsDirCommand = ReactiveCommand.Create(hostsFileService.OpenFileDir);
                ResetHostsFileCommand = ReactiveCommand.Create(hostsFileService.ResetFile);
                NetworkFixCommand = ReactiveCommand.Create(ProxyService.Current.FixNetwork);
                TrustCerCommand = ReactiveCommand.Create(TrustCer_OnClick);
                OpenCertificateDirCommand = ReactiveCommand.Create(() =>
                {
                    httpProxyService.GetCerFilePathGeneratedWhenNoFileExists();
                    platformService.OpenFolder(httpProxyService.PfxFilePath);
                });
                RefreshCommand = ReactiveCommand.Create(RefreshButton_Click);
            }

            //AutoRunProxyCommand = ReactiveCommand.Create(() =>
            //{
            //    AutoRunProxy?.CheckmarkChange(ProxySettings.ProgramStartupRunProxy.Value = !ProxySettings.ProgramStartupRunProxy.Value);
            //});
            ProxySettingsCommand = ReactiveCommand.Create(() =>
            {
                IWindowManager.Instance.Show(CustomWindow.ProxySettings, resizeMode: ResizeMode.CanResize);
            });
            //EnableProxyScriptCommand = ReactiveCommand.Create(() =>
            //{
            //    EnableProxyScript?.CheckmarkChange(ProxySettings.IsEnableScript.Value = !ProxySettings.IsEnableScript.Value);
            //});
            //MenuItems = new ObservableCollection<MenuItemViewModel>()
            //{
            //    //new MenuItemViewModel(nameof(AppResources.CommunityFix_MenuName))
            //    //{
            //    //    Items = new[]
            //    //    {
            //            (AutoRunProxy = new MenuItemViewModel (nameof(AppResources.CommunityFix_AutoRunProxy)){
            //                Command=AutoRunProxyCommand}),

            //            //(EnableProxyScript = new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)){ Command=EnableProxyScriptCommand }),
            //            //new MenuItemViewModel (nameof(AppResources.CommunityFix_ScriptManage)){ Command=EditHostsFileCommand ,IconKey="JavaScriptDrawing" },
            //            new MenuItemViewModel (),
            //            new MenuItemViewModel (nameof(AppResources.Refresh))
            //                { IconKey="RefreshDrawing" , Command = RefreshCommand},
            //            new MenuItemViewModel (nameof(AppResources.CommunityFix_CertificateSettings))
            //            {
            //                IconKey="CertificateDrawing",
            //                Items = new[]
            //                {
            //                    new MenuItemViewModel(nameof(AppResources.CommunityFix_SetupCertificate)){ IconKey="CertificateDrawing", Command=SetupCertificateCommand },
            //                    new MenuItemViewModel(nameof(AppResources.CommunityFix_DeleteCertificate)){ IconKey="CertificateDrawing", Command=DeleteCertificateCommand },
            //                    new MenuItemViewModel(nameof(AppResources.CommunityFix_OpenCertificateDir)){ IconKey="FolderOpenDrawing", Command=OpenCertificateDirCommand },
            //                }
            //            },
            //            new MenuItemViewModel (),
            //            new MenuItemViewModel (nameof(AppResources.CommunityFix_EditHostsFile)){ Command=EditHostsFileCommand,IconKey="DocumentEditDrawing" },
            //            //new MenuItemViewModel (nameof(AppResources.CommunityFix_NetworkFix)){ Command=NetworkFixCommand },
            //    //    }
            //    //},
            //};

            //if (OperatingSystem2.IsMacOS)
            //{
            //    MenuItems.Add(new MenuItemViewModel(nameof(AppResources.CommunityFix_CertificateTrust)) { IconKey = "RefreshDrawing", Command = TrustCerCommand });
            //}
            //AutoRunProxy?.CheckmarkChange(ProxySettings.ProgramStartupRunProxy.Value);
        }

        public void TrustCer_OnClick()
        {
            httpProxyService.TrustCer();
        }

        public override void Activation()
        {
            if (ProxyService.Current.ProxyStatus)
            {
                ProxyService.Current.StartTimer();
            }

            base.Activation();
        }

        public override void Deactivation()
        {
            if (ProxyService.Current.ProxyStatus)
            {
                ProxyService.Current.StopTimer();
            }

            base.Deactivation();
        }

        public async void RefreshButton_Click()
        {
            if (ProxyService.Current.ProxyStatus == false)
                await ProxyService.Current.InitializeAccelerate();
        }

        public void StartProxyButton_Click(bool start)
        {
            ProxyService.Current.ProxyStatus = start;
        }

        public void SetupCertificate_OnClick()
        {
            httpProxyService.SetupCertificate();
        }

        public void DeleteCertificate_OnClick()
        {
            httpProxyService.DeleteCertificate();
        }

    }
}
