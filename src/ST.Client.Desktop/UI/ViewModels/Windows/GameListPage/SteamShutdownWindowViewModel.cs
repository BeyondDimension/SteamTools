using DynamicData;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SteamShutdownWindowViewModel : WindowViewModel
    {
        public SteamShutdownWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_SteamShutdown;

            _DownloadingAppSourceList = new SourceList<SteamApp>();

            _DownloadingAppSourceList
               .Connect()
               .ObserveOn(RxApp.MainThreadScheduler)
               //.Sort(SortExpressionComparer<SteamApp>.Descending(x => x.))
               .Bind(out _DownloadingApps)
               .Subscribe();
        }

        private readonly ReadOnlyObservableCollection<SteamApp>? _DownloadingApps;
        public ReadOnlyObservableCollection<SteamApp>? DownloadingApps => _DownloadingApps;

        private readonly SourceList<SteamApp> _DownloadingAppSourceList;

    }
}