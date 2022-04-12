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
using System.ComponentModel;
using System.IO;

namespace System.Application.UI.ViewModels
{
    public class EditAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_EditAppInfo;

        private SteamApp _App;
        public SteamApp App
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

            App.RefreshEditImage();

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

        public async void SaveEditAppInfo()
        {
            #region 自定义图片保存
            if (App.EditLibraryLogoStream?.ToString() != await App.LibraryLogoStream)
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryLogoStream,
                 SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Grid) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            if (App.EditLibraryHeroStream?.ToString() != await App.LibraryHeroStream)
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryHeroStream,
                 SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Grid) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            if (App.EditLibraryGridStream?.ToString() != await App.LibraryGridStream)
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryGridStream,
                    SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Grid) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            #endregion

            App.IsEdited = true;
        }

        public void CancelEditAppInfo()
        {
            this.Close();
        }

        public void ResetEditAppInfo()
        {
            App.RefreshEditImage();
            App.IsEdited = false;
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

            var stream = await IHttpService.Instance.GetImageStreamAsync(SelectGrid.Url, default);
            switch (type)
            {
                case SteamGridItemType.Grid:
                    App.EditLibraryGridStream = stream;
                    break;
                case SteamGridItemType.Hero:
                    App.EditLibraryHeroStream = stream;
                    break;
                case SteamGridItemType.Logo:
                    App.EditLibraryLogoStream = stream;
                    break;
            }
        }

        public override void OnClosing(object? sender, CancelEventArgs e)
        {
            if (App != null)
            {
                if (App.EditLibraryGridStream is MemoryStream ms)
                {
                    ms.Close();
                    ms.Dispose();
                }
                if (App.EditLibraryHeroStream is MemoryStream ms1)
                {
                    ms1.Close();
                    ms1.Dispose();
                }
                if (App.EditLibraryLogoStream is MemoryStream ms2)
                {
                    ms2.Close();
                    ms2.Dispose();
                }
            }

            _SteamGridItemSourceList.Dispose();
        }

        public async void OpenSteamGridDBImageUrl(SteamGridItem item)
        {
            await Browser2.OpenAsync(item.Url);
        }

        public async void OpenSteamGridDBAppUrl(SteamGridItem item)
        {
            var url = item.GridType switch
            {
                SteamGridItemType.Grid => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Grid, item.Id),
                SteamGridItemType.Hero => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Hero, item.Id),
                SteamGridItemType.Icon => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Icon, item.Id),
                SteamGridItemType.Logo => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Logo, item.Id),
                _ => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Grid, item.Id),
            };

            await Browser2.OpenAsync(url);
        }

        public async void OpenSteamGridDBAuthorUrl(SteamGridItem item)
        {
            await Browser2.OpenAsync(string.Format(SteamGridDBApiUrls.SteamGridDB_Author_URL, item.Author.Steam64));
        }
    }
}