using BD.SteamClient.Constants;
using BD.SteamClient.Enums.SteamGridDB;
using BD.SteamClient.Models.SteamGridDB;

namespace BD.WTTS.UI.ViewModels;

public sealed class EditAppInfoPageViewModel : WindowViewModel
{
    readonly SourceCache<SteamGridItem, long> _SteamGridItemSourceList;
    readonly ReadOnlyObservableCollection<SteamGridItem>? _SteamGridItems;

    public static string DisplayName => Strings.GameList_EditAppInfo;

    public ReadOnlyObservableCollection<SteamGridItem>? SteamGridItems => _SteamGridItems;

    public SteamApp App { get; }

    [Reactive]
    public SteamGridItem? SelectGrid { get; set; }

    [Reactive]
    public bool IsLoadingSteamGrid { get; set; }

    public bool IsSteamGridEmpty => !IsLoadingSteamGrid && !_SteamGridItemSourceList.Items.Any_Nullable();

    public ICommand UpLaunchItemCommand { get; }

    public ICommand DownLaunchItemCommand { get; }

    public ICommand DeleteLaunchItemCommand { get; }

    public ICommand OpenSteamGridDBImageUrlCommand { get; }

    public ICommand OpenSteamGridDBAppUrlCommand { get; }

    public ICommand OpenSteamGridDBAuthorUrlCommand { get; }

    public EditAppInfoPageViewModel(ref SteamApp app)
    {
        App = app;

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

        UpLaunchItemCommand = ReactiveCommand.Create<SteamAppLaunchItem>(UpLaunchItem);
        DownLaunchItemCommand = ReactiveCommand.Create<SteamAppLaunchItem>(DownLaunchItem);
        DeleteLaunchItemCommand = ReactiveCommand.Create<SteamAppLaunchItem>(DeleteLaunchItem);
        OpenSteamGridDBImageUrlCommand = ReactiveCommand.Create<SteamGridItem>(OpenSteamGridDBImageUrl);
        OpenSteamGridDBAppUrlCommand = ReactiveCommand.Create<SteamGridItem>(OpenSteamGridDBAppUrl);
        OpenSteamGridDBAuthorUrlCommand = ReactiveCommand.Create<SteamGridItem>(OpenSteamGridDBAuthorUrl);
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

    void MoveLaunchItem(in SteamAppLaunchItem item, bool isUp)
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

    public void UpLaunchItem(SteamAppLaunchItem item)
    {
        MoveLaunchItem(item, true);
    }

    public void DownLaunchItem(SteamAppLaunchItem item)
    {
        MoveLaunchItem(item, false);
    }

    public void DeleteLaunchItem(SteamAppLaunchItem item)
    {
        if (App.LaunchItems != null)
        {
            App.LaunchItems.Remove(item);
        }
    }

    static bool CheckCurrentSteamUserStats(in SteamUser? user)
    {
        if (user == null)
        {
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SteamUserNullTip);
            return false;
        }
        return true;
    }

    public async void SaveEditAppInfo()
    {
        #region 自定义图片保存
        //var mostRecentUser = ISteamService.Instance.GetRememberUserList().Where(s => s.MostRecent).FirstOrDefault();
        var mostRecentUser = SteamConnectService.Current.SteamUsers.Items.Where(s => s.MostRecent).FirstOrDefault();
        if (!(App.EditHeaderLogoStream is FileStream fs3 && fs3.Name == (await App.HeaderLogoStream)?.Name))
        {
            if (!CheckCurrentSteamUserStats(mostRecentUser))
                return;

            if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditHeaderLogoStream,
             mostRecentUser!, App.AppId, SteamGridItemType.Header) == false)
            {
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, nameof(SteamGridItemType.Header)));
            }
            else
                this.RaisePropertyChanged(nameof(App.HeaderLogoStream));
        }
        if (!(App.EditLibraryLogoStream is FileStream fs && fs.Name == (await App.LibraryLogoStream)?.Name))
        {
            if (!CheckCurrentSteamUserStats(mostRecentUser))
                return;

            if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryLogoStream,
             mostRecentUser!, App.AppId, SteamGridItemType.Logo) == false)
            {
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, nameof(SteamGridItemType.Logo)));
            }
            else
                this.RaisePropertyChanged(nameof(App.LibraryLogoStream));
        }
        if (!(App.EditLibraryHeroStream is FileStream fs1 && fs1.Name == (await App.LibraryHeroStream)?.Name))
        {
            if (!CheckCurrentSteamUserStats(mostRecentUser))
                return;

            if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryHeroStream,
             mostRecentUser!, App.AppId, SteamGridItemType.Hero) == false)
            {
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, nameof(SteamGridItemType.Hero)));
            }
            else
                this.RaisePropertyChanged(nameof(App.LibraryHeroStream));
        }
        if (!(App.EditLibraryGridStream is FileStream fs2 && fs2.Name == (await App.LibraryGridStream)?.Name))
        {
            if (!CheckCurrentSteamUserStats(mostRecentUser))
                return;

            if (await ISteamService.Instance.SaveAppImageToSteamFile(App.EditLibraryGridStream,
                mostRecentUser!, App.AppId, SteamGridItemType.Grid) == false)
            {
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, nameof(SteamGridItemType.Grid)));
            }
            else
                this.RaisePropertyChanged(nameof(App.LibraryGridStream));
        }
        #endregion

        App.IsEdited = true;

        SteamConnectService.Current.SteamApps.AddOrUpdate(App);

        //await MessageBox.ShowAsync("保存成功但还不会直接写入Steam文件, 请打开[保存Steam游戏自定义信息窗口]保存所有更改信息到Steam文件中。",
        //    ThisAssembly.AssemblyTrademark, MessageBox.Button.OK, MessageBox.Image.None, MessageBox.DontPromptType.SaveEditAppInfo);

        Toast.Show(ToastIcon.Info, "保存成功但还不会直接写入 Steam 文件，请到右上角编辑列表中保存所有更改信息到 Steam 文件中，即可生效！");

        this.Close?.Invoke(false);
    }

    public void CancelEditAppInfo()
    {
        App.RefreshEditImage();
        this.Close?.Invoke(false);
    }

    public void ResetEditAppInfo()
    {
        //if (await MessageBox.ShowAsync("确定要重置当前App所有更改吗？(不会重置自定义图片)", ThisAssembly.AssemblyTrademark, MessageBox.Button.OKCancel) == MessageBox.Result.OK)
        //{
        App.RefreshEditImage();
        if (App.OriginalData != null)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(App.OriginalData), Encoding.UTF8, true);
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
            //Toast.Show(ToastIcon.Warning, Strings.SaveEditedAppInfo_SelectImageFailed);
            var mostRecentUser = SteamConnectService.Current.SteamUsers.Items.Where(s => s.MostRecent).FirstOrDefault();
            if (!CheckCurrentSteamUserStats(mostRecentUser))
                return;
            await ISteamService.Instance.SaveAppImageToSteamFile(null, mostRecentUser!, App.AppId, type);

            switch (type)
            {
                case SteamGridItemType.Header:
                    App.EditHeaderLogoStream = (await App.HeaderLogoStream)?.Stream;
                    break;
                case SteamGridItemType.Grid:
                    App.EditLibraryGridStream = (await App.LibraryGridStream)?.Stream;
                    break;
                case SteamGridItemType.Hero:
                    App.EditLibraryHeroStream = (await App.LibraryHeroStream)?.Stream;
                    break;
                case SteamGridItemType.Logo:
                    App.EditLibraryLogoStream = (await App.LibraryLogoStream)?.Stream;
                    break;
            }
            return;
        }
        Toast.Show(ToastIcon.Info, "图片下载需要一些时间，请稍后");
        var imageHttpClientService = Ioc.Get<IImageHttpClientService>();
        var stream = await imageHttpClientService.GetImageMemoryStreamAsync(SelectGrid.Url, default);
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

    public void OpenFolder(object? parm)
    {
        if (parm is string path && !string.IsNullOrEmpty(path))
            IPlatformService.Instance.OpenFolder(path);
    }

    public void ManageCloudArchive_Click()
    {
        GameListPageViewModel.ManageCloudArchive_Click(App);
    }
}
