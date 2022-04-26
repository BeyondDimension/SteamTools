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
using System.Linq;

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

        private void MoveLaunchItem(in SteamAppLaunchItem item, bool isUp)
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

        bool CheckCurrentSteamUserStats(in SteamUser user)
        {
            if (user == null)
            {
                Toast.Show(AppResources.SaveEditedAppInfo_SteamUserNullTip);
                return false;
            }
            return true;
        }

        public async void SaveEditAppInfo()
        {
            #region 自定义图片保存
            var mostRecentUser = ISteamService.Instance.GetRememberUserList().Where(s => s.MostRecent).FirstOrDefault();
            if (!(App.EditHeaderLogoStream is FileStream fs3 && fs3.Name == await App.HeaderLogoStream))
            {
                if (!CheckCurrentSteamUserStats(mostRecentUser))
                    return;

                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditHeaderLogoStream,
                 mostRecentUser, App.AppId, SteamGridItemType.Header) == false)
                {
                    Toast.Show(string.Format(AppResources.SaveImageFileFailed, nameof(SteamGridItemType.Logo)));
                }
            }
            if (!(App.EditLibraryLogoStream is FileStream fs && fs.Name == await App.LibraryLogoStream))
            {
                if (!CheckCurrentSteamUserStats(mostRecentUser))
                    return;

                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryLogoStream,
                 mostRecentUser, App.AppId, SteamGridItemType.Logo) == false)
                {
                    Toast.Show(string.Format(AppResources.SaveImageFileFailed, nameof(SteamGridItemType.Logo)));
                }
            }
            if (!(App.EditLibraryHeroStream is FileStream fs1 && fs1.Name == await App.LibraryHeroStream))
            {
                if (!CheckCurrentSteamUserStats(mostRecentUser))
                    return;

                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryHeroStream,
                 mostRecentUser, App.AppId, SteamGridItemType.Hero) == false)
                {
                    Toast.Show(string.Format(AppResources.SaveImageFileFailed, nameof(SteamGridItemType.Hero)));
                }
            }
            if (!(App.EditLibraryGridStream is FileStream fs2 && fs2.Name == await App.LibraryGridStream))
            {
                if (!CheckCurrentSteamUserStats(mostRecentUser))
                    return;

                if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryGridStream,
                    mostRecentUser, App.AppId, SteamGridItemType.Grid) == false)
                {
                    Toast.Show(string.Format(AppResources.SaveImageFileFailed, nameof(SteamGridItemType.Grid)));
                }
            }
            #endregion

            App.IsEdited = true;

            SteamConnectService.Current.SteamApps.AddOrUpdate(App);

            //await MessageBox.ShowAsync("保存成功但还不会直接写入Steam文件, 请打开[保存Steam游戏自定义信息窗口]保存所有更改信息到Steam文件中。",
            //    ThisAssembly.AssemblyTrademark, MessageBox.Button.OK, MessageBox.Image.None, MessageBox.DontPromptType.SaveEditAppInfo);

            this.Close();
        }

        public void CancelEditAppInfo()
        {
            App.RefreshEditImage();
            this.Close();
        }

        public void ResetEditAppInfo()
        {
            //if (await MessageBox.ShowAsync("确定要重置当前App所有更改吗？(不会重置自定义图片)", ThisAssembly.AssemblyTrademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
            //{
            App.RefreshEditImage();
            if (App.OriginalData != null)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(App.OriginalData));
                reader.BaseStream.Seek(40L, SeekOrigin.Current);
                var table = reader.ReadPropertyTable();

                App.ExtractReaderProperty(table);

                App.IsEdited = true;
                SteamConnectService.Current.SteamApps.AddOrUpdate(App);
            }
            //}
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
            if (SelectGrid == null)
            {
                Toast.Show(AppResources.SaveEditedAppInfo_SelectImageFailed);
                return;
            }

            var stream = await IHttpService.Instance.GetImageStreamAsync(SelectGrid.Url, default);
            switch (type)
            {
                case SteamGridItemType.Header:
                    App.EditHeaderLogoStream = stream;
                    break;
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
                App.EditHeaderLogoStream = null;
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
                SteamGridItemType.Header => string.Format(SteamGridDBApiUrls.SteamGridDBUrl_Grid, item.Id),
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