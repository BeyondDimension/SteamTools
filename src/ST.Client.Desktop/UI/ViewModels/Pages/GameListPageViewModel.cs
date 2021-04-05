using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }

        private IReadOnlyCollection<SteamApp>? _SteamApps;
        public IReadOnlyCollection<SteamApp>? SteamApps
        {
            get => _SteamApps;
            set
            {
                if (_SteamApps != value)
                {
                    _SteamApps = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        internal async override Task Initialize()
        {
            SteamApps = ISteamService.Instance.GetAppInfos();
            await Task.CompletedTask;
        }
    }
}
