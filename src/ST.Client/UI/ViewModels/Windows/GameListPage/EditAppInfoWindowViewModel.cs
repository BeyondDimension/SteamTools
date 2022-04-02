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

        private SteamGridItem? _SelectGrid;
        public SteamGridItem? SelectGrid
        {
            get => _SelectGrid;
            set => this.RaiseAndSetIfChanged(ref _SelectGrid, value);
        }

        private bool _IsLoadingSteamGrid;
        public bool IsLoadingSteamGrid
        {
            get => _IsLoadingSteamGrid;
            set => this.RaiseAndSetIfChanged(ref _IsLoadingSteamGrid, value);
        }

        public bool IsSteamGridEmpty => !IsLoadingSteamGrid && !SteamGridItems.Any_Nullable();

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
              .Subscribe(_ => this.RaisePropertyChanged(nameof(IsSteamGridEmpty)));
        }

        public void AddLaunchItem()
        {
            if (App.LaunchItems == null)
            {
                App.LaunchItems = new ObservableCollection<SteamAppLaunchItem> { new() };
            }
            else
            {
                App.LaunchItems.Add(new());
            }
        }

        public void UpLaunchItem(SteamAppLaunchItem item)
        {
            MoveLaunchItem(item, true);
        }

        public void DownLaunchItem(SteamAppLaunchItem item)
        {
            MoveLaunchItem(item, false);
        }

        private void MoveLaunchItem(SteamAppLaunchItem item, bool isUp)
        {
            if (App.LaunchItems != null)
            {
                var oldindex = App.LaunchItems.IndexOf(item);
                if (isUp ? oldindex == 0 : oldindex == App.LaunchItems.Count - 1)
                {
                    return;
                }
                App.LaunchItems.Move(oldindex, isUp ? oldindex - 1 : oldindex + 1);
            }
        }

        public void DeleteLaunchItem(SteamAppLaunchItem item)
        {
            if (App.LaunchItems != null)
            {
                App.LaunchItems.Remove(item);
            }
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
            IsLoadingSteamGrid = true;
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

            IsLoadingSteamGrid = false;
        }

        public async void ApplyCustomImageToApp(SteamGridItemType type)
        {
            if (SteamConnectService.Current.CurrentSteamUser == null)
            {
                Toast.Show("因为修改自定义封面是根据账号生效的，所以必须要先运行Steam");
                return;
            }

            if (SelectGrid == null)
            {
                Toast.Show("请选择一张要应用的图片");
                return;
            }

            App.EditLibraryGridStream = IHttpService.Instance.GetImageStreamAsync(SelectGrid.Url, default);

            //if (await ISteamService.Instance.SaveAppImageToSteamFile(stream, SteamConnectService.Current.CurrentSteamUser, App.AppId, type) == false)
            //{
            //    Toast.Show("下载图片失败");
            //}
        }
    }
}