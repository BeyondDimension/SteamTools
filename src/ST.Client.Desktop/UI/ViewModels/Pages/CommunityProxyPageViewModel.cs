using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public CommunityProxyPageViewModel() 
        {
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                new MenuItemViewModel(nameof(AppResources.CommunityFix_MenuName))
                {
                    Items = new[]
                    {
                        new MenuItemViewModel(nameof(AppResources.CommunityFix_SetupCertificate)) {IconKey="SteamDrawing"},                  
                        new MenuItemViewModel(nameof(AppResources.CommunityFix_DeleteCertificate)) {IconKey="SteamDrawing"},
                        new MenuItemViewModel (),
                        new MenuItemViewModel (nameof(AppResources.Edit)),
                    }
                },
            };
        }

        private IReadOnlyCollection<AccelerateProjectGroupDTO> _ProxyDomains;
        public IReadOnlyCollection<AccelerateProjectGroupDTO> ProxyDomains
        {
            get => _ProxyDomains;
            set
            {
                if (_ProxyDomains != value)
                {
                    _ProxyDomains = value;
                    this.RaisePropertyChanged();
                }
            }
        }


        private AccelerateProjectGroupDTO _SelectGroup;
        public AccelerateProjectGroupDTO SelectGroup
        {
            get => _SelectGroup;
            set
            {
                if (_ProxyDomains != value)
                {
                    _SelectGroup = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        internal async override Task Initialize()
        {
            var client = ICloudServiceClient.Instance.Accelerate;
            var result = await client.All();
            if (!result.IsSuccess) 
            {
                return;
            }

            ProxyDomains = new List<AccelerateProjectGroupDTO>(result.Content);
            SelectGroup = ProxyDomains.First();
        }
    }
}
