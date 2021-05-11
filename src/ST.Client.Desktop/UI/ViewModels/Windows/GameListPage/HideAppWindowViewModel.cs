using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class HideAppWindowViewModel : WindowViewModel
    {
        public HideAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_EditAppInfo;
        }



        private ObservableCollection<SteamHideApps>? _SteamHideApp;
        public ObservableCollection<SteamHideApps>? SteamHideApp
        {
            get => _SteamHideApp;
            set
            {
                if (_SteamHideApp != value)
                {
                    _SteamHideApp = value;
                    this.RaisePropertyChanged();
                }
            }
        }


    }
}