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

            if (App.SaveFiles.Any_Nullable())
                foreach (var file in App.SaveFiles)
                {
                    file.FormatPathGenerate();
                }

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
            if (!(App.EditLibraryLogoStream is FileStream fs && fs.Name == await App.LibraryLogoStream))
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryLogoStream,
                 SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Logo) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            if (!(App.EditLibraryHeroStream is FileStream fs1 && fs1.Name == await App.LibraryHeroStream))
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryHeroStream,
                 SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Hero) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            if (!(App.EditLibraryGridStream is FileStream fs2 && fs2.Name == await App.LibraryGridStream))
            {
                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryGridStream,
                    SteamConnectService.Current.CurrentSteamUser, App.AppId, SteamGridItemType.Grid) == false)
                {
                    Toast.Show($"保存 {SteamGridItemType.Grid} 自定义图片失败");
                }
            }
            #endregion

            App.IsEdited = true;

            SteamConnectService.Current.SteamApps.AddOrUpdate(App);

            this.Close();
        }

        public void CancelEditAppInfo()
        {
            App.RefreshEditImage();
            this.Close();
        }

        public async void ResetEditAppInfo()
        {
            if (await MessageBox.ShowAsync("确定要重置当前App所有更改吗？", ThisAssembly.AssemblyTrademark, MessageBox.Button.OK) == MessageBox.Result.OK)
            {
                App.RefreshEditImage();
            }
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
            if (App != null && !App.IsEdited)
            {
                App.EditLibraryGridStream = null;
                App.EditLibraryHeroStream = null;
                App.EditLibraryLogoStream = null;
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

        public void OpenFolder(string path)
        {
            if (!string.IsNullOrEmpty(path))
                IPlatformService.Instance.OpenFolder(path);
        }
    }
}