using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class CloudArchiveWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_CloudArchiveManager;

        private int AppId { get; }

        #region 云存档列表
        private readonly ReadOnlyObservableCollection<SteamRemoteFile>? _CloudArchivews;
        private readonly SourceList<SteamRemoteFile> _CloudArchivewSourceList;

        public ReadOnlyObservableCollection<SteamRemoteFile>? CloudArchivews => _CloudArchivews;

        public string CloudArchivewCountStr => string.Format(AppResources.FileNumber, CloudArchivews?.Count ?? 0);

        private bool _IsLoading;

        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }
        #endregion

        //private bool? _IsCheckAll;

        //public bool? IsCheckAll
        //{
        //    get => _IsCheckAll;
        //    set => this.RaiseAndSetIfChanged(ref _IsCheckAll, value);
        //}

        //public bool IsCloudEnabledForAccount { get; }

        //private bool _IsCloudEnabledForApp;

        //public bool IsCloudEnabledForApp
        //{
        //    get => _IsCloudEnabledForApp;
        //    set => this.RaiseAndSetIfChanged(ref _IsCloudEnabledForApp, value);
        //}

        private int _UsedQutoa;

        public int UsedQutoa
        {
            get => _UsedQutoa;
            set => this.RaiseAndSetIfChanged(ref _UsedQutoa, value);
        }

        private int _TotalQutoa;

        public int TotalQutoa
        {
            get => _TotalQutoa;
            set => this.RaiseAndSetIfChanged(ref _TotalQutoa, value);
        }

        public CloudArchiveWindowViewModel(int appid)
        {
            Title = GetTitleByDisplayName(DisplayName);

            SteamConnectService.Current.Initialize(appid);

            if (SteamConnectService.Current.IsConnectToSteam == false)
            {
                MessageBox.ShowAsync(AppResources.Achievement_Warning_1, Title, MessageBox.Button.OK).ContinueWith(s =>
                {
                    EnforceClose();
                }).Wait();
            }

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
            ToastService.Current.Set(AppResources.Achievement_LoadData);

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
            var result = await MessageBox.ShowAsync(AppResources.GameList_CloudArchiveDeleteAllTip, Title, MessageBox.Button.OKCancel);
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
                        Toast.Show(AppResources.UploadSuccess);
                    }
                    catch (IOException)
                    {
                        Toast.Show(AppResources.UploadFailed);
                    }
                }
            }

            RefreshList();
        }

        public async void DownloadFile(SteamRemoteFile remoteFile)
        {
            var result = await FilePicker2.SaveAsync(new SaveOptions
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

                    Toast.Show(string.Format(AppResources.Download_Success_FileNameTip, remoteFile.Name));
                }
                catch
                {
                    Toast.Show(string.Format(AppResources.Download_Failed_FileNameTip, remoteFile.Name));
                }
            }
        }

        public async void DeleteFile(SteamRemoteFile remoteFile)
        {
            var result = await MessageBox.ShowAsync(AppResources.GameList_CloudArchiveDeleteTip, Title, MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                if (remoteFile.Delete())
                {
                    RefreshList();
                }
            }
        }

        void EnforceClose()
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}