using ReactiveUI;
using System.Application.Models;
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

        public CommunityProxyPageViewModel()
        {
            SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
            DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
            EditHostsFileCommand = ReactiveCommand.Create(EditHostsFile_OnClick);

            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                new MenuItemViewModel(nameof(AppResources.CommunityFix_MenuName))
                {
                    Items = new[]
                    {
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_EditHostsFile)){ Command=EditHostsFileCommand,IconKey="EditDrawing" },
                        new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.CommunityFix_CertificateSettings))
                        {
                            Items=new[]
                            {
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_SetupCertificate)){ Command=SetupCertificateCommand },
                                new MenuItemViewModel(nameof(AppResources.CommunityFix_DeleteCertificate)){ Command=DeleteCertificateCommand },
                            }
                        },

                    }
                },
            };
        }


        //internal async override Task Initialize()
        //{

        //}


        public void StartProxyButton_Click()
        {
            ProxyService.Current.ProxyStatus = true;
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
