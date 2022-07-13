using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SteamShutdownWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_SteamShutdown;

        private readonly ReadOnlyObservableCollection<SteamApp>? _DownloadingApps;

        public ReadOnlyObservableCollection<SteamApp>? DownloadingApps => _DownloadingApps;

        private bool? _IsAllCheck = false;

        public bool? IsAllCheck
        {
            get => _IsAllCheck;
            set => this.RaiseAndSetIfChanged(ref _IsAllCheck, value);
        }

        public IReadOnlyCollection<SystemEndMode>? SystemEndModes { get; }

        public SteamShutdownWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            var modes = new List<SystemEndMode>();
            modes.Add(SystemEndMode.Sleep);
            if (!OperatingSystem2.IsMacOS())
                modes.Add(SystemEndMode.Hibernate);
            modes.Add(SystemEndMode.Shutdown);

            SystemEndModes = modes;

            SteamConnectService.Current.DownloadApps
               .Connect()
               .Filter(p => p.IsDownloading)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Sort(SortExpressionComparer<SteamApp>.Ascending(x => x.AppId).ThenBy(x => x.DisplayName))
               .Bind(out _DownloadingApps)
               .Subscribe();

            this.WhenAnyValue(x => x.DownloadingApps)
                .Subscribe(items => items?
                        .ToObservableChangeSet()
                        //.DistinctUntilChanged()
                        .AutoRefresh(x => x.IsWatchDownloading)
                        .WhenValueChanged(x => x.IsWatchDownloading)
                        .Subscribe(_ =>
                        {
                            bool? b = null;
                            var ids = items.Where(s => s.IsWatchDownloading).Select(s => s.AppId).ToArray();
                            if (!items.Any_Nullable() || ids.Length == 0)
                                b = false;
                            else if (ids.Length == items.Count)
                                b = true;

                            if (this.IsAllCheck != b)
                            {
                                this.IsAllCheck = b;
                            }

                            SteamConnectService.Current.WatchDownloadingSteamAppIds.Clear();
                            foreach (var id in ids)
                                SteamConnectService.Current.WatchDownloadingSteamAppIds.Add(id);
                        }));

            this.WhenValueChanged(x => x.IsAllCheck, false)
                .Subscribe(x =>
                {
                    if (DownloadingApps != null)
                        if (x == true)
                        {
                            foreach (var item in DownloadingApps)
                                if (!item.IsWatchDownloading)
                                    item.IsWatchDownloading = true;
                        }
                        else if (x == false)
                        {
                            foreach (var item in DownloadingApps)
                                if (item.IsWatchDownloading)
                                    item.IsWatchDownloading = false;
                        }
                });

            Initialize();
        }

        public override void Initialize()
        {
            if (!SteamConnectService.Current.IsWatchSteamDownloading)
            {
                SteamConnectService.Current.InitializeDownloadGameList();
            }
        }
    }
}