using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class IdleAppWindowViewModel : WindowViewModel
    {
        public IdleAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_EditAppInfo;
            Refresh_Click();
        }

        private bool _RunState;
        public bool RunState
        {
            get => _RunState;
            set => this.RaiseAndSetIfChanged(ref _RunState, value);
        }

        public ObservableCollection<SteamApp> _IdleGameList = new();
        public ObservableCollection<SteamApp> IdleGameList
        {
            get => _IdleGameList;
            set => this.RaiseAndSetIfChanged(ref _IdleGameList, value);
        }
        public void StopOrRunItem(SteamApp item)
        {
            SteamConnectService.Current.RuningSteamApps.FirstOrDefault(x => x.AppId == item.AppId);


        }
        public void Refresh_Click()
        {
            IdleGameList = new ObservableCollection<SteamApp>(SteamConnectService.Current.RuningSteamApps.Where(x => GameLibrarySettings.AFKAppList.Value?.ContainsKey(x.AppId) ?? false));
        }

    }
}