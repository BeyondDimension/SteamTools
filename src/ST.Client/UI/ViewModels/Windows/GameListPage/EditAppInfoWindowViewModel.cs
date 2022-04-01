using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using ReactiveUI;
using DynamicData;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData.Binding;
using System.Application.Services;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditAppInfo;

        private SteamApp? _App;
        public SteamApp? App
        {
            get => _App;
            set => this.RaiseAndSetIfChanged(ref _App, value);
        }

        readonly SourceCache<SteamGridItem, long> _SteamGridItemSourceList;
        readonly ReadOnlyObservableCollection<SteamGridItem>? _SteamGridItems;
        public ReadOnlyObservableCollection<SteamGridItem>? SteamGridItems => _SteamGridItems;

        public EditAppInfoWindowViewModel(SteamApp app)
        {
            if (app == null)
            {
                this.Close();
                return;
            }
            App = app;
            Title = App.DisplayName;

            _SteamGridItemSourceList = new SourceCache<SteamGridItem, long>(t => t.Id);

            _SteamGridItemSourceList
              .Connect()
              .ObserveOn(RxApp.MainThreadScheduler)
              .Sort(SortExpressionComparer<SteamGridItem>.Ascending(x => x.Id))
              .Bind(out _SteamGridItems)
              .Subscribe();
        }

        public void AddLaunchItem()
        {
            if (App.LaunchItems == null)
            {
                App.LaunchItems = new List<SteamAppLaunchItem> { new() };
            }
            else
            {
                App.LaunchItems.Add(new());
            }

            App.RaisePropertyChanged(nameof(App.LaunchItems));
        }

        public void DeleteLaunchItem(SteamAppLaunchItem item)
        {
            if (App.LaunchItems != null)
            {
                App.LaunchItems.Remove(item);
            }

            App.RaisePropertyChanged(nameof(App.LaunchItems));
        }

        public void SaveEditAppInfo()
        {

        }

        public void CancelEditAppInfo()
        {
            this.Close();
        }

        public async void RefreshSteamGridItemList(SteamGridItemType type = SteamGridItemType.Grid)
        {
            _SteamGridItemSourceList.Clear();

            var grid = await ISteamGridDBWebApiServiceImpl.Instance.GetSteamGridAppBySteamAppId(App.AppId);

            if (grid != null)
            {
                var items = await ISteamGridDBWebApiServiceImpl.Instance.GetSteamGridItemsByGameId(grid.Id, type);

                if (items.Any_Nullable())
                {
                    _SteamGridItemSourceList.AddOrUpdate(items);
                }
            }
        }
    }
}