using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.CommunityFix;
            protected set { throw new NotImplementedException(); }
        }

        public ReactiveCommand<Unit, Unit> SetupCertificateCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCertificateCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenCertificateDirCommand { get; }
        public ReactiveCommand<Unit, Unit> EditHostsFileCommand { get; }
        public ReactiveCommand<Unit, Unit> NetworkFixCommand { get; }
        public ReactiveCommand<Unit, Unit> AutoRunProxyCommand { get; }
        //public ReactiveCommand<Unit, Unit> EnableProxyScriptCommand { get; }

        public MenuItemViewModel AutoRunProxy { get; }
        //public MenuItemViewModel EnableProxyScript { get; }

        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }
        public ReactiveCommand<Unit, Unit> TrustCerCommand { get; }

        public CommunityProxyPageViewModel()
        {
            IconKey = nameof(CommunityProxyPageViewModel).Replace("ViewModel", "Svg");

            SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
            DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
            EditHostsFileCommand = ReactiveCommand.Create(EditHostsFile_OnClick);
            NetworkFixCommand = ReactiveCommand.Create(ProxyService.Current.FixNetwork);
            TrustCerCommand = ReactiveCommand.Create(TrustCer_OnClick);
            AutoRunProxyCommand = ReactiveCommand.Create(() =>
            {
                AutoRunProxy?.CheckmarkChange(ProxySettings.ProgramStartupRunProxy.Value = !ProxySettings.ProgramStartupRunProxy.Value);
            });
            OpenCertificateDirCommand = ReactiveCommand.Create(() =>
            {
                DI.Get<IDesktopPlatformService>().OpenFolder(IOPath.AppDataDirectory + @$"\{ThisAssembly.AssemblyProduct}.Certificate.cer");
            });
            RefreshCommand = ReactiveCommand.Create(RefreshButton_Click);
            //EnableProxyScriptCommand = ReactiveCommand.Create(() =>
            //{
            //    EnableProxyScript?.CheckmarkChange(ProxySettings.IsEnableScript.Value = !ProxySettings.IsEnableScript.Value);
            //});
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                //new MenuItemViewModel(nameof(AppResources.CommunityFix_MenuName))
                //{
                //    Items = new[]
                //    {
                        (AutoRunProxy = new MenuItemViewModel (nameof(AppResources.CommunityFix_AutoRunProxy)){
                            Command=AutoRunProxyCommand}),

                        //(EnableProxyScript = new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)){ Command=EnableProxyScriptCommand }),
                        //new MenuItemViewModel (nameof(AppResources.CommunityFix_ScriptManage)){ Command=EditHostsFileCommand ,IconKey="JavaScriptDrawing" },
                        new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.Refresh))
                            { IconKey="RefreshDrawing" , Command = RefreshCommand},
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_CertificateSettings))
                        {
                            IconKey="MoreDrawing",
                            Items = new[]
                            {
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_SetupCertificate)){ IconKey="CertificateDrawing", Command=SetupCertificateCommand },
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_DeleteCertificate)){ IconKey="CertificateDrawing", Command=DeleteCertificateCommand },
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_OpenCertificateDir)){ IconKey="FolderOpenDrawing", Command=OpenCertificateDirCommand },
                            }
                        },
#if MAC
                         new MenuItemViewModel (nameof(AppResources.Refresh))
                            { IconKey="RefreshDrawing" , Command = TrustCerCommand},
#endif
                new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_EditHostsFile)){ Command=EditHostsFileCommand,IconKey="DocumentEditDrawing" },
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_NetworkFix)){ Command=NetworkFixCommand },
                //    }
                //},
            };

            AutoRunProxy?.CheckmarkChange(ProxySettings.ProgramStartupRunProxy.Value);
        }
        public void TrustCer_OnClick() {
            DI.Get<IHttpProxyService>().TrustCer();
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

        public async override void Initialize()
        {
            await Task.CompletedTask;
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
            DI.Get<IHttpProxyService>().SetupCertificate();
        }

        public void DeleteCertificate_OnClick()
        {
            DI.Get<IHttpProxyService>().DeleteCertificate();
        }

        public void EditHostsFile_OnClick()
        {
            IHostsFileService.Instance.OpenFile();
        }
    }
}
