using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        readonly IHttpProxyService httpProxyService = DI.Get<IHttpProxyService>();

        public override string Name
        {
            get => AppResources.CommunityFix;
            protected set { throw new NotImplementedException(); }
        }

        public ReactiveCommand<Unit, Unit> SetupCertificateCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteCertificateCommand { get; }
        public ReactiveCommand<Unit, Unit> EditHostsFileCommand { get; }
        public ReactiveCommand<Unit, Unit> AutoRunProxyCommand { get; }
        public ReactiveCommand<Unit, Unit> OnlySteamBrowserCommand { get; }

        public MenuItemViewModel AutoRunProxy { get; }
        public MenuItemViewModel OnlySteamBrowser { get; }


        public CommunityProxyPageViewModel()
        {
            SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
            DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
            EditHostsFileCommand = ReactiveCommand.Create(EditHostsFile_OnClick);

            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                //new MenuItemViewModel(nameof(AppResources.CommunityFix_MenuName))
                //{
                //    Items = new[]
                //    {
                        (this.AutoRunProxy = new MenuItemViewModel (nameof(AppResources.CommunityFix_AutoRunProxy)){ Command=AutoRunProxyCommand }),
                        (this.OnlySteamBrowser = new MenuItemViewModel (nameof(AppResources.CommunityFix_OnlySteamBrowser)){ Command=OnlySteamBrowserCommand}),
                        new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)){ Command=EditHostsFileCommand ,IconKey="CheckmarkDrawing" },
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_ScriptManage)){ Command=EditHostsFileCommand ,IconKey="JavaScriptDrawing" },
                        new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_CertificateSettings))
                        {
                            Items = new[]
                            {
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_SetupCertificate)){ Command=SetupCertificateCommand },
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_DeleteCertificate)){ Command=DeleteCertificateCommand },
                            }
                        },
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_EditHostsFile)){ Command=EditHostsFileCommand,IconKey="DocumentEditDrawing" },
                //    }
                //},
            };

            AutoRunProxyCommand = ReactiveCommand.Create(() =>
            {
                ProxySettings.ProgramStartupRunProxy.Value = !ProxySettings.ProgramStartupRunProxy.Value;
                if (ProxySettings.ProgramStartupRunProxy.Value)
                    AutoRunProxy.IconKey = "CheckmarkDrawing";
                else
                    AutoRunProxy.IconKey = "";
            });
            OnlySteamBrowserCommand = ReactiveCommand.Create(() =>
            {
                ProxySettings.IsOnlyWorkSteamBrowser.Value = !ProxySettings.IsOnlyWorkSteamBrowser.Value;
                if (ProxySettings.IsOnlyWorkSteamBrowser.Value)
                    OnlySteamBrowser.IconKey = "CheckmarkDrawing";
                else
                    OnlySteamBrowser.IconKey = "";
            });


        }


        //internal async override Task Initialize()
        //{

        //}


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

        public void EditHostsFile_OnClick()
        {
            IHostsFileService.Instance.OpenFile();
        }
    }
}
