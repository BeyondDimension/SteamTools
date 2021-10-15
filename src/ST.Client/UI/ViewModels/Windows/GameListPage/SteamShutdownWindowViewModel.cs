using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SteamShutdownWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_SteamShutdown;

        public SteamShutdownWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            SteamConnectService.Current.DownloadApps
               .Connect()
               .Filter(p => p.IsDownloading)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId).ThenByDescending(x => x.SizeOnDisk))
               .Bind(out _DownloadingApps)
               .Subscribe();
        }

        private readonly ReadOnlyObservableCollection<SteamApp>? _DownloadingApps;
        public ReadOnlyObservableCollection<SteamApp>? DownloadingApps => _DownloadingApps;
    }
}