using System.Xml.Linq;

namespace BD.WTTS.UI.ViewModels;

public sealed class CloudArchiveAppPageViewModel : WindowViewModel
{
    public static string DisplayName => Strings.GameList_CloudArchiveManager;

    int AppId { get; }

    #region 云存档列表

    private readonly ReadOnlyObservableCollection<SteamRemoteFile>? _CloudArchivews;
    private readonly SourceList<SteamRemoteFile> _CloudArchivewSourceList;

    public ReadOnlyObservableCollection<SteamRemoteFile>? CloudArchivews => _CloudArchivews;

    public string CloudArchivewCountStr => string.Format(Strings.FileNumber, CloudArchivews?.Count ?? 0);

    #endregion

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public int UsedQutoa { get; set; }

    [Reactive]
    public int TotalQutoa { get; set; }

    public ICommand DownloadFile { get; set; }

    public ICommand DeleteFile { get; set; }

    public CloudArchiveAppPageViewModel(int appid)
    {
        Title = DisplayName;

        SteamConnectService.Current.Initialize(appid);

        if (SteamConnectService.Current.IsConnectToSteam == false)
        {
            MessageBox.ShowAsync(Strings.Achievement_Warning_1, Title, MessageBox.Button.OK).ContinueWith(s =>
            {
                EnforceClose();
            }).Wait();
        }

        DownloadFile = ReactiveCommand.Create<SteamRemoteFile>(async remoteFile =>
        {
            var result = await FilePicker2.SaveAsync(new PickOptions
            {
                PickerTitle = "Download " + remoteFile.Name,
                InitialFileName = remoteFile.Name,
            });

            if (result != null)
            {
                try
                {
                    using var dest = result.OpenWrite();
                    var b = remoteFile.ReadAllBytes();
                    await dest.WriteAsync(b, 0, b.Length);
                    dest.Close();

                    Toast.Show(ToastIcon.Success, string.Format(Strings.Download_Success_FileNameTip, remoteFile.Name));
                }
                catch
                {
                    Toast.Show(ToastIcon.Error, string.Format(Strings.Download_Failed_FileNameTip, remoteFile.Name));
                }
            }
        });
        DeleteFile = ReactiveCommand.Create<SteamRemoteFile>(async remoteFile =>
        {
            var result = await MessageBox.ShowAsync(Strings.GameList_CloudArchiveDeleteTip, Title, MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                if (remoteFile.Delete())
                {
                    RefreshList();
                }
            }
        });
        _CloudArchivewSourceList = new SourceList<SteamRemoteFile>();

        _CloudArchivewSourceList
            .Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<SteamRemoteFile>.Descending(x => x.Timestamp).ThenByAscending(x => x.Name))
            .Bind(out _CloudArchivews)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(CloudArchivewCountStr)));

        AppId = appid;
        string name = ISteamworksLocalApiService.Instance.GetAppData((uint)appid, "name");
        name ??= appid.ToString();
        Title = Constants.HARDCODED_APP_NAME + " | " + name;

        //ToastService.Current.Set(Strings.Achievement_LoadData);

        RefreshList();
    }

    public void RefreshList()
    {
        if (!IsLoading)
        {
            IsLoading = true;
            var results = ISteamworksLocalApiService.Instance.GetCloudArchiveFiles();
            _CloudArchivewSourceList.Clear();
            if (results.Any_Nullable())
                _CloudArchivewSourceList.AddRange(results);
            IsLoading = false;
        }
        _CloudArchivewSourceList.Items.Sum(x => x.Size);
        ISteamworksLocalApiService.Instance.GetCloudArchiveQuota(out var totalBytes, out var availBytes);
        TotalQutoa = (int)(totalBytes / 1024 / 1024);
        UsedQutoa = (int)(_CloudArchivewSourceList.Items.Sum(x => x.Size) / 1024 / 1024);
    }

    public async void ClearAllFiles()
    {
        var result = await MessageBox.ShowAsync(Strings.GameList_CloudArchiveDeleteAllTip, Title, MessageBox.Button.OKCancel);
        if (result.IsOK())
        {
            foreach (var file in _CloudArchivewSourceList.Items)
            {
                file.Delete();
            }
            RefreshList();
        }
    }

    public async void UploadFile()
    {
        var result = await FilePicker2.PickMultipleAsync(new PickOptions
        {
            PickerTitle = "",
        });

        if (result != null)
        {
            foreach (var f in result)
            {
                var file = new SteamRemoteFile(f.FileName.ToLowerInvariant());

                try
                {
                    byte[] data = File.ReadAllBytes(f.FullPath);
                    if (!file.WriteAllBytes(data))
                    {
                        throw new IOException("Upload File Write Failed");
                    }
                    Toast.Show(ToastIcon.Success, Strings.UploadSuccess);
                }
                catch (IOException)
                {
                    Toast.Show(ToastIcon.Error, Strings.UploadFailed);
                }
            }
        }

        RefreshList();
    }

    void EnforceClose()
    {
        Process.GetCurrentProcess().Kill();
    }
}
