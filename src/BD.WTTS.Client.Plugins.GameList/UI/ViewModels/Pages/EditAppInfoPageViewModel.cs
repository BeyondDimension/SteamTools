using BD.SteamClient.Constants;
using BD.SteamClient.Enums.SteamGridDB;
using BD.SteamClient.Models.SteamGridDB;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class EditAppInfoPageViewModel : WindowViewModel
{
    readonly SourceCache<SteamGridItem, long> _SteamGridItemSourceList;
    readonly ReadOnlyObservableCollection<SteamGridItem>? _SteamGridItems;

    public static string DisplayName => Strings.GameList_EditAppInfo;

    public ReadOnlyObservableCollection<SteamGridItem>? SteamGridItems => _SteamGridItems;

    public SteamApp App { get; }

    public SteamApp Draft { get; }
    readonly SteamAppEdit originals;
    readonly SteamAppEdit openingValues;
    readonly CancellationTokenSource mediaLifetime = new();
    readonly HashSet<SteamGridItemType> changedImages = [];
    readonly Dictionary<SteamGridItemType, Stream> images = [];
    bool closed;

    [Reactive] public partial int DraftTabIndex { get; set; }
    [Reactive] public partial string? DraftName { get; set; }
    [Reactive] public partial bool IsChangingImage { get; set; }

    public ICommand ResetEditAppInfoCommand { get; }
    public ICommand ResetImageCommand { get; }
    public ICommand AddLaunchItemCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand ManageCloudArchiveCommand { get; }

    public Stream? EditHeaderLogoStream
    {
        get => images.GetValueOrDefault(SteamGridItemType.Header);
        set => SetImage(SteamGridItemType.Header, value);
    }

    public Stream? EditLibraryLogoStream
    {
        get => images.GetValueOrDefault(SteamGridItemType.Logo);
        set => SetImage(SteamGridItemType.Logo, value);
    }

    public Stream? EditLibraryHeroStream
    {
        get => images.GetValueOrDefault(SteamGridItemType.Hero);
        set => SetImage(SteamGridItemType.Hero, value);
    }

    public Stream? EditLibraryGridStream
    {
        get => images.GetValueOrDefault(SteamGridItemType.Grid);
        set => SetImage(SteamGridItemType.Grid, value);
    }
    static readonly SteamGridItemType[] ImageTypes =
        [SteamGridItemType.Header, SteamGridItemType.Logo, SteamGridItemType.Hero, SteamGridItemType.Grid];

    static SteamApp.LibCacheType CacheType(SteamGridItemType type) => type switch
    {
        SteamGridItemType.Header => SteamApp.LibCacheType.Header,
        SteamGridItemType.Logo => SteamApp.LibCacheType.Logo,
        SteamGridItemType.Hero => SteamApp.LibCacheType.Library_Hero,
        _ => SteamApp.LibCacheType.Library_Grid,
    };

    void SetImage(SteamGridItemType type, Stream? stream, bool changed = true)
    {
        var previous = images.GetValueOrDefault(type);
        if (ReferenceEquals(previous, stream) && stream != null)
            return;
        if (closed)
        {
            stream?.Dispose();
            return;
        }

        if (changed)
            changedImages.Add(type);
        if (stream != null)
            images[type] = stream;
        else
            images.Remove(type);
        this.RaisePropertyChanged(type switch
        {
            SteamGridItemType.Header => nameof(EditHeaderLogoStream),
            SteamGridItemType.Logo => nameof(EditLibraryLogoStream),
            SteamGridItemType.Hero => nameof(EditLibraryHeroStream),
            _ => nameof(EditLibraryGridStream),
        });
        previous?.Dispose();
    }

    Task LoadMediaAsync() => Task.WhenAll(ImageTypes.Select(LoadImageAsync));

    async Task LoadImageAsync(SteamGridItemType type)
    {
        try
        {
            var stream = await ISteamService.Instance.GetAppImageAsync(App, CacheType(type), mediaLifetime.Token);
            if (closed || changedImages.Contains(type))
                stream?.Dispose();
            else
                SetImage(type, stream, changed: false);
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception ex)
        {
            Log.Error(nameof(EditAppInfoPageViewModel), ex, "Unable to load the game cover.");
        }
    }

    static SteamUser? GetCurrentSteamUser()
    {
        var users = SteamConnectService.Current.SteamUsers.Items;
        return users.FirstOrDefault(s => s.MostRecent) ?? users.FirstOrDefault(s => s.AutoLogin);
    }

    string? GetCustomImagePath(SteamUser user, SteamGridItemType type)
    {
        var steamPath = ISteamService.Instance.SteamDirPath;
        if (string.IsNullOrEmpty(steamPath))
            return null;

        var suffix = type switch
        {
            SteamGridItemType.Logo => "_logo",
            SteamGridItemType.Hero => "_hero",
            SteamGridItemType.Grid => "p",
            _ => "",
        };
        return Path.Combine(steamPath, "userdata", user.SteamId32.ToString(), "config", "grid", $"{App.AppId}{suffix}.png");
    }

    void ResetImage(SteamGridItemType type)
    {
        if (GetCurrentSteamUser() is { } user && File.Exists(GetCustomImagePath(user, type)))
            SetImage(type, null);
        else if (changedImages.Remove(type))
        {
            SetImage(type, null, changed: false);
            _ = LoadImageAsync(type);
        }
    }

    public async Task SaveEditAppInfoAsync()
    {
        try
        {
            var edited = SteamAppEdit.Capture(Draft) with { Name = DraftName ?? string.Empty };
            if (!edited.Diff(openingValues).IsEmpty)
            {
                await SteamConnectService.Current.SaveGameAppEditAsync(App, edited.Diff(originals));
                Toast.Show(ToastIcon.Info, Strings.L10nGames_SaveEditNotice);
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(EditAppInfoPageViewModel), ex, "Unable to save the local game edit.");
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SaveFailed);
        }

        if (changedImages.Count == 0)
            return;

        var user = GetCurrentSteamUser();
        if (user == null)
        {
            Toast.Show(ToastIcon.Error, Strings.SaveEditedAppInfo_SteamUserNullTip);
            return;
        }

        foreach (var type in changedImages)
        {
            try
            {
                var stream = images.GetValueOrDefault(type);
                var target = GetCustomImagePath(user, type);
                if (stream == null && !File.Exists(target))
                    continue;
                if (target != null && stream is FileStream file && string.Equals(Path.GetFullPath(file.Name), Path.GetFullPath(target),
                    OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    continue;
                if (await ISteamService.Instance.SaveAppImageToSteamFile(stream, user, App.AppId, type))
                {
                    App.RaisePropertyChanged(type switch
                    {
                        SteamGridItemType.Header => nameof(App.HeaderLogoStream),
                        SteamGridItemType.Logo => nameof(App.LibraryLogoStream),
                        SteamGridItemType.Hero => nameof(App.LibraryHeroStream),
                        _ => nameof(App.LibraryGridStream),
                    });
                    continue;
                }
            }
            catch (Exception ex)
            {
                Log.Error(nameof(EditAppInfoPageViewModel), ex, "Unable to save the game cover.");
            }
            Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, type));
        }
    }

    void ResetEditAppInfo()
    {
        originals.ApplyTo(Draft);
        DraftName = Draft.Name;
        Draft.RaisePropertyChanged(nameof(Draft.Developer));
        Draft.RaisePropertyChanged(nameof(Draft.Publisher));
    }

    public async void RefreshSteamGridItemList(SteamGridItemType type)
    {
        IsLoadingSteamGrid = true;
        _SteamGridItemSourceList.Clear();
        try
        {
            var grid = await ISteamGridDBWebApiServiceImpl.Instance.GetSteamGridAppBySteamAppId(App.AppId);
            if (grid == null || closed)
                return;
            var items = await ISteamGridDBWebApiServiceImpl.Instance.GetSteamGridItemsByGameId(grid.Id, type);
            if (!closed && items.Any_Nullable())
                _SteamGridItemSourceList.AddOrUpdate(items);
        }
        catch (Exception ex)
        {
            Log.Error(nameof(EditAppInfoPageViewModel), ex, "Unable to load SteamGridDB results.");
        }
        finally
        {
            if (!closed)
                IsLoadingSteamGrid = false;
        }
    }

    public async Task ApplyCustomImageToApp(SteamGridItemType type, SteamGridItem item)
    {
        IsChangingImage = true;
        try
        {
            Toast.Show(ToastIcon.Info, Strings.L10nGames_ImageDownloadWait);
            var stream = await Ioc.Get<IImageHttpClientService>().GetImageMemoryStreamAsync(item.Url, cancellationToken: mediaLifetime.Token);
            if (stream != null)
                SetImage(type, stream);
            else if (!closed)
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, type));
        }
        catch (OperationCanceledException) when (closed) { }
        catch (Exception ex)
        {
            Log.Error(nameof(EditAppInfoPageViewModel), ex, "Unable to download the game cover.");
            if (!closed)
                Toast.Show(ToastIcon.Error, string.Format(Strings.SaveImageFileFailed, type));
        }
        finally
        {
            IsChangingImage = false;
        }
    }

    public override void OnClosing(object? sender, CancelEventArgs e)
    {
        if (!e.Cancel)
            StopMedia();
    }

    void StopMedia()
    {
        if (closed)
            return;
        closed = true;
        mediaLifetime.Cancel();
        _SteamGridItemSourceList.Dispose();
    }

    public void DisposeEditor()
    {
        StopMedia();
        mediaLifetime.Dispose();
        foreach (var stream in images.Values)
            stream.Dispose();
        images.Clear();
    }

    [Reactive]
    public partial bool IsLoadingSteamGrid { get; set; }

    public bool IsSteamGridEmpty => !IsLoadingSteamGrid && !_SteamGridItemSourceList.Items.Any_Nullable();

    public ICommand UpLaunchItemCommand { get; }

    public ICommand DownLaunchItemCommand { get; }

    public ICommand DeleteLaunchItemCommand { get; }

    public ICommand OpenSteamGridDBImageUrlCommand { get; }

    public ICommand OpenSteamGridDBAppUrlCommand { get; }

    public ICommand OpenSteamGridDBAuthorUrlCommand { get; }

    public EditAppInfoPageViewModel(SteamApp app)
    {
        App = app;
        Draft = new SteamApp(app.AppId);
        originals = app.GetOriginalEditValues();
        openingValues = SteamAppEdit.Capture(app);
        openingValues.ApplyTo(Draft);
        DraftName = Draft.Name;
        ResetEditAppInfoCommand = ReactiveCommand.Create(ResetEditAppInfo);
        ResetImageCommand = ReactiveCommand.Create<SteamGridItemType>(ResetImage);
        AddLaunchItemCommand = ReactiveCommand.Create(AddLaunchItem);
        OpenFolderCommand = ReactiveCommand.Create<object?>(OpenFolder);
        ManageCloudArchiveCommand = ReactiveCommand.Create(ManageCloudArchive_Click);
        this.ObserveValue(nameof(DraftTabIndex), static x => x.DraftTabIndex).Where(index => index == 1)
            .Take(1).Subscribe(index => { _ = LoadMediaAsync(); });

        if (App.SaveFiles.Any_Nullable())
            foreach (var file in App.SaveFiles)
            {
                file.FormatPathGenerate();
            }

        _SteamGridItemSourceList = new SourceCache<SteamGridItem, long>(t => t.Id);

        _SteamGridItemSourceList
          .Connect()
          .ObserveOn(RxSchedulers.MainThreadScheduler)
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
        if (Draft.LaunchItems == null)
            Draft.LaunchItems = new ObservableCollection<SteamAppLaunchItem> { new() };
        else
            Draft.LaunchItems.Add(new());
    }

    void MoveLaunchItem(in SteamAppLaunchItem item, bool isUp)
    {
        if (Draft.LaunchItems is { } items)
        {
            var oldIndex = items.IndexOf(item);
            var newIndex = isUp ? oldIndex - 1 : oldIndex + 1;
            if (oldIndex >= 0 && newIndex >= 0 && newIndex < items.Count)
                items.Move(oldIndex, newIndex);
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
        Draft.LaunchItems?.Remove(item);
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
