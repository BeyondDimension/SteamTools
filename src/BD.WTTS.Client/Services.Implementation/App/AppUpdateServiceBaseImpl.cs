using static BD.WTTS.Services.IAppUpdateService;
using AppResources = BD.WTTS.Client.Resources.Strings;
using FailCode = BD.WTTS.Enums.AppUpdateFailCode;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

public abstract class AppUpdateServiceBaseImpl : ReactiveObject, IAppUpdateService
{
    protected const string TAG = "AppUpdateS";
    protected readonly IMicroServiceClient client;
    protected readonly AppSettings settings;
    protected readonly IToast toast;
    protected readonly IApplication application;
    protected readonly INotificationService notification;

    public ICommand StartUpdateCommand { get; }

    public bool ShowNewVersionWindowOnMainOpen { get; protected set; }

    protected virtual async Task ShowNewVersionWindowAsync()
    {
        await MainThread2.InvokeOnMainThreadAsync(async () =>
        {
            var result = await MessageBox.ShowAsync(NewVersionInfoDesc,
                NewVersionInfoTitle,
                MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                StartUpdateCommand.Invoke();
            }
        });
    }

    protected virtual void ShowNewVersionNotifyAsync()
    {
        notification.Notify(
            AppResources.NewVersionUpdateNotifyText_.Format(NewVersionInfo?.Version),
            NotificationType.NewVersion);
    }

    public virtual async void OnMainOpenTryShowNewVersionWindow()
    {
        if (ShowNewVersionWindowOnMainOpen)
        {
            ShowNewVersionWindowOnMainOpen = false;
            await ShowNewVersionWindowAsync();
        }
    }

    protected abstract bool HasActiveWindow();

    public AppUpdateServiceBaseImpl(
        IApplication application,
        INotificationService notification,
        IToast toast,
        IMicroServiceClient client,
        IOptions<AppSettings> options)
    {
        this.toast = toast;
        this.client = client;
        this.application = application;
        this.notification = notification;
        settings = options.Value;
        StartUpdateCommand = ReactiveCommand.Create(StartUpdate);
    }

    protected virtual Version OSVersion => Environment.OSVersion.Version;

    public virtual bool IsSupportedServerDistribution
    {
        get
        {
#if WINDOWS
            if (DesktopBridge.IsRunningAsUwp) return false;
            return true;
#else
            return false;
#endif
        }
    }

    public bool IsExistUpdate { get; protected set; }

    AppVersionDTO? _NewVersionInfo;

    public AppVersionDTO? NewVersionInfo
    {
        get => _NewVersionInfo;
#if DEBUG
        set
#else
        protected set
#endif
        {
            _NewVersionInfo = value;
            this.RaisePropertyChanged(nameof(NewVersionInfoDesc));
            this.RaisePropertyChanged(nameof(NewVersionInfoTitle));
        }
    }

    public string NewVersionInfoDesc
    {
        get
        {
            var value = NewVersionInfo?.ReleaseNote;
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            var lines = value.Split(';');
            return string.Join(Environment.NewLine, lines);
        }
    }

    public string NewVersionInfoTitle => AppResources.NewVersionUpdateTitle__.Format(NewVersionInfo?.Version, AssemblyInfo.Trademark);

    /// <summary>
    /// 当存在新的版本时，重写此方法实现弹窗提示用户 
    /// </summary>
    protected virtual async void OnExistNewVersion()
    {
        var hasActiveWindow = HasActiveWindow();
        if (hasActiveWindow)
        {
            await ShowNewVersionWindowAsync();
        }
        else
        {
            ShowNewVersionWindowOnMainOpen = true;
            ShowNewVersionNotifyAsync();
        }
    }

    static bool isCheckUpdateing;

    public async Task CheckUpdateAsync(bool force, bool showIsExistUpdateFalse = true)
    {
        if (!force && IsExistUpdate)
        {
            if (NewVersionInfo.HasValue())
            {
                OnExistNewVersion();
                return;
            }
            else
            {
                IsExistUpdate = false;
            }
        }

        if (isCheckUpdateing) return;
        isCheckUpdateing = true;

        var platform = DeviceInfo2.Platform();
        var deviceIdiom = DeviceInfo2.Idiom();
        if (deviceIdiom == DeviceIdiom.Tablet && platform == Platform.Apple)
        {
            deviceIdiom = DeviceIdiom.Phone;
        }
        var osVersion = OSVersion;
        var architecture = OperatingSystem2.IsWindows() ?
            RuntimeInformation.ProcessArchitecture :
            RuntimeInformation.OSArchitecture;
        var deploymentMode = application.DeploymentMode;
        var rsp = await client.Version.CheckUpdate(
            platform, deviceIdiom,
            osVersion,
            architecture,
            deploymentMode);
        if (rsp.IsSuccess)
        {
            static bool IsNewVersion(AppVersionDTO? value)
            {
                if (value.HasValue())
                {
                    if (value.Version != AssemblyInfo.FileVersion)
                    {
                        if (Version.TryParse(value.Version, out var newVersion) &&
                            Version.TryParse(AssemblyInfo.FileVersion, out var thisVersion))
                        {
                            if (newVersion <= thisVersion)
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            if (!IsNewVersion(rsp.Content))
            {
                IsExistUpdate = false;
                if (showIsExistUpdateFalse) toast.Show(ToastIcon.None, AppResources.IsExistUpdateFalse);
            }
            else
            {
                IsExistUpdate = true;
                NewVersionInfo = rsp.Content;
                OnExistNewVersion();
            }
        }
        else
        {
            // ServerApiClient 错误已由全局处理
        }

        isCheckUpdateing = false;
    }

    static bool isDownloading;

    /// <summary>
    /// 是否支持断点续传
    /// </summary>
    const bool isSupportedResume = false;

    bool _IsNotStartUpdateing = true;

    public bool IsNotStartUpdateing
    {
        get => _IsNotStartUpdateing;
        protected set => this.RaiseAndSetIfChanged(ref _IsNotStartUpdateing, value);
    }

    float _ProgressValue;

    public float ProgressValue
    {
        get => _ProgressValue;
        protected set => this.RaiseAndSetIfChanged(ref _ProgressValue, value);
    }

    string _ProgressString = string.Empty;

    public string ProgressString
    {
        get => _ProgressString;
        protected set => this.RaiseAndSetIfChanged(ref _ProgressString, value);
    }

    protected void OnReportDownloading3(float value, int current, int count) => OnReport(value, AppResources.Downloading3__.Format(MathF.Round(value, 2), current, count));

    protected void OnReportCalcHashing3(float value, int current, int count) => OnReport(value, AppResources.CalcHashing3__.Format(MathF.Round(value, 2), current, count));

    protected void OnReportDownloading(float value) => OnReport(value, AppResources.Downloading_.Format(MathF.Round(value, 2)));

    protected void OnReportCalcHashing(float value) => OnReport(value, AppResources.CalcHashing_.Format(MathF.Round(value, 2)));

    protected void OnReportDecompressing(float value) => OnReport(value, AppResources.Decompressing_.Format(MathF.Round(value, 2)));

    protected void OnReport(float value = 0f) => OnReport(value, string.Empty);

    protected IProgress<float>? progress;

    protected virtual void OnReport(float value, string str)
    {
        ProgressValue = value;
        ProgressString = str;

        if (notification.IsSupportNotifyDownload)
        {
            if (value == 0)
            {
                progress?.Report(100f);
                progress = notification.NotifyDownload(() => ProgressString,
                    NotificationType.NewVersion);
            }
            else if (value == 100f)
            {
                progress?.Report(100f);
                progress = null;
            }
            else
            {
                progress?.Report(value);
            }
        }
    }

    bool UpdatePackVerification(string filePath, string sha256, int current = 0, int count = 0)
    {
        void OnReportCalcHashing_(float value) => OnReportCalcHashing(value);
        void OnReportCalcHashing3_(float value) => OnReportCalcHashing3(value, current, count);

        Action<float> onReportCalcHashing = current > 0 && count > 0 ? OnReportCalcHashing3_ : OnReportCalcHashing_;
        onReportCalcHashing(0);
        try
        {
            using var fs = File.OpenRead(filePath);
            var sha256_ = Hashs.String.SHA256(fs);  // 改为带进度的哈希计算
            var value = string.Equals(sha256_, sha256, StringComparison.OrdinalIgnoreCase);
            onReportCalcHashing(100f);
            return value;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    static string GetSingleFileUrl(string fileId) =>
        string.Format(Constants.Urls.OfficialWebsite_UploadsPublishFiles, fileId);

    static string GetPackFileUrl(string fileName) =>
        string.Format(Constants.Urls.OfficialWebsite_UploadsPublish, fileName);

    /// <summary>
    /// 下载更新包文件到本地缓存
    /// </summary>
    protected async void DownloadUpdate()
    {
        var isCallOverwriteUpgrade = false;

        if (isDownloading)
            return;

        if (!IsSupportedServerDistribution)
            throw new PlatformNotSupportedException();

        isDownloading = true;

        var newVersionInfo = NewVersionInfo;

        if (newVersionInfo.HasValue())
        {
            if (newVersionInfo!.DisableAutomateUpdate)
            {
                OpenInAppStore();
                goto end;
            }

            var isAndroid = OperatingSystem.IsAndroid();
            var isDesktop = IApplication.IsDesktop();
            if (!isAndroid && !isDesktop)
            {
                OpenInAppStore();
                goto end;
            }

            AppVersionDTODownload? download = null;
            if (newVersionInfo.Downloads != null)
            {
                if (isAndroid)
                {
                    CloudFileType apk = (CloudFileType)265;
                    download = GetByDownloadChannelSettings(
                        newVersionInfo.Downloads.Where(x => x.DownloadType == apk));
                }
                else if (isDesktop)
                {
                    download = GetByDownloadChannelSettings(
                        newVersionInfo.Downloads.Where(x => x.DownloadType == CloudFileType.SevenZip));
                    download ??= GetByDownloadChannelSettings(
                        newVersionInfo.Downloads.Where(x => x.DownloadType == CloudFileType.TarGzip));
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
            if (download.HasValue()) // 压缩包格式是否正确
            {
                OnReport();

                var packFileName = GetPackName(newVersionInfo, isDirOrFile: true);
                var packFilePath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packFileName);
                if (File.Exists(packFilePath)) // 存在压缩包文件
                {
                    if (string.IsNullOrWhiteSpace(download.SHA256) || UpdatePackVerification(packFilePath, download.SHA256)) // (已有文件)哈希验证成功，进行覆盖安装
                    {
                        isCallOverwriteUpgrade = true;
                        OverwriteUpgrade(packFilePath, isIncrement: false, downloadType: download.DownloadType);
                        goto end;
                    }
                    else // 验证文件失败，删除源文件，将会重新下载
                    {
                        try
                        {
                            File.Delete(packFilePath);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(TAG, ex, "全量更新已有缓存文件验证失败，删除源文件时错误");
                            Fail(FailCode.UpdatePackCacheHashInvalidDeleteFileFail_, packFilePath);
                            goto end;
                        }
                        toast.Show(ToastIcon.Error, AppResources.UpdatePackCacheHashInvalidDeleteFileTrue);
                    }
                }

                string filePlatform;
                string fileArch;
                var fileEx = newVersionInfo.Platform switch
                {
                    Platform.Android => FileEx.APK,
                    Platform.Windows or Platform.Linux or Platform.Apple => download!.DownloadType switch
                    {
                        CloudFileType.TarGzip => FileEx.TAR_GZ,
                        CloudFileType.SevenZip => FileEx._7Z,
                        _ => string.Empty,
                    },
                    _ => string.Empty,
                };
                if (fileEx == string.Empty)
                {
                    Fail(FailCode.UpdateEnumOutOfRange);
                    goto end;
                }
                switch (newVersionInfo.Platform)
                {
                    case Platform.Windows:
                        filePlatform = "win";
                        break;
                    case Platform.Linux:
                        filePlatform = "linux";
                        break;
                    case Platform.Android:
                        filePlatform = "android";
                        break;
                    default:
                        Fail(FailCode.UpdateEnumOutOfRange);
                        goto end;
                }
                switch (newVersionInfo.SupportedAbis)
                {
                    case ArchitectureFlags.X86:
                        fileArch = "x86";
                        break;
                    case ArchitectureFlags.X64:
                        fileArch = "x64";
                        break;
                    case ArchitectureFlags.Arm64:
                        fileArch = "arm64";
                        break;
                    case ArchitectureFlags.Arm:
                        fileArch = "arm";
                        break;
                    default:
                        fileArch = ((int)newVersionInfo.SupportedAbis).ToString();
                        break;
                    case 0:
                        Fail(FailCode.UpdateEnumOutOfRange);
                        goto end;
                }

                string requestUri;
                if (string.IsNullOrWhiteSpace(download!.DownloadUrl))
                {
                    var downloadFileName = $"{filePlatform}-{fileArch}_{newVersionInfo.Version}{fileEx}";
                    requestUri = GetPackFileUrl(downloadFileName);
                }
                else if (String2.IsHttpUrl(download.DownloadUrl))
                {
                    requestUri = download.DownloadUrl;
                }
                else
                {
                    requestUri = GetSingleFileUrl(download.DownloadUrl!);
                }

                var cacheFilePath = packFilePath + FileEx.DownloadCache;
                OnReportDownloading(0f);
                var rsp = await client.Download(
                    isAnonymous: true,
                    requestUri: requestUri,
                    cacheFilePath,
                    new Progress<float>(OnReportDownloading));
                OnReportDownloading(100f);

                if (rsp.IsSuccess)
                {
                    File.Move(cacheFilePath, packFilePath);

                    if (string.IsNullOrWhiteSpace(download.SHA256) || UpdatePackVerification(packFilePath, download.SHA256)) // (下载文件)哈希验证成功，进行覆盖安装
                    {
                        isCallOverwriteUpgrade = true;
                        OverwriteUpgrade(packFilePath, isIncrement: false, downloadType: download.DownloadType);
                    }
                    else
                    {
                        Fail(FailCode.UpdatePackVerificationFail);
                    }
                }
                else // 下载失败，可能服务器崩了
                {
                    Fail(FailCode.DownloadUpdateFail);
                }
            }
            else
            {
                OpenInAppStore();
            }
        }

    end: isDownloading = false;
        if (!isCallOverwriteUpgrade)
        {
            IsNotStartUpdateing = true;
        }
        void Fail(FailCode failCode, params string[] args)
        {
            var error = failCode.ToString2(args);
            toast.Show(ToastIcon.Error, error);
            Browser2.Open(string.Format(Constants.Urls.OfficialWebsite_AppUpdateFailCode_, (byte)failCode));
            OnReport(100f);
        }
    }

    /// <summary>
    /// 覆盖更新，根据更新包解压覆盖文件
    /// <para>Android：打开apk包即可</para>
    /// <para>isIncrement==<see langword="true"/> 增量更新，value值为下载缓存所在文件夹路径</para>
    /// <para>isIncrement==<see langword="false"/> 全量更新，value值为压缩包文件名</para>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isIncrement"></param>
    /// <param name="downloadType"></param>
    protected virtual async void OverwriteUpgrade(string value, bool isIncrement, CloudFileType downloadType = default)
    {
        if (isIncrement) // 增量更新
        {
            OverwriteUpgradePrivate(value);
        }
        else // 全量更新
        {
            var dirPath = Path.Combine(IOPath.BaseDirectory, Path.GetFileNameWithoutExtension(value));

            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }

            var isOK = await Task.Run(() =>
            {
                try
                {
                    return downloadType switch
                    {
                        CloudFileType.TarGzip
                            => TarGZipHelper.Unpack(value, dirPath,
                                progress: new Progress<float>(OnReportDecompressing),
                                maxProgress: 100f),
                        CloudFileType.SevenZip
                            => ISevenZipHelper.Instance.Unpack(value, dirPath,
                                progress: new Progress<float>(OnReportDecompressing),
                                maxProgress: 100f),
                        _ => false,
                    };
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, ex, "Compressed unpack catch.");
                    return false;
                }
            });
            if (isOK)
            {
                OnReport(100f);
                IOPath.FileTryDelete(value);
                OverwriteUpgradePrivate(dirPath);
            }
            else
            {
                toast.Show(ToastIcon.Error, AppResources.UpdateUnpackFail);
                OnReport(100f);
                IsNotStartUpdateing = true;
            }
        }

        static async void OverwriteUpgradePrivate(string dirPath)
        {
            var path = dirPath.TrimEnd(Path.DirectorySeparatorChar);

            byte[]? sha384Data = null;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                sha384Data = SHA384.HashData(stream);
            }

            var platformService = await IPlatformService.IPCRoot.Instance;
            platformService.SetUpdatePackageInfo(path, sha384Data);
        }
    }

    /// <summary>
    /// 在应用商店中打开
    /// </summary>
    protected virtual async void OpenInAppStore()
    {
#if WINDOWS
        if (DesktopBridge.IsRunningAsUwp)
        {
            await Browser2.OpenAsync(Constants.Urls.MicrosoftStoreProtocolLink);
            return;
        }
#endif
        await Browser2.OpenAsync(Constants.Urls.OfficialWebsite);
    }

#if DEBUG
    public
#endif
    void StartUpdate()
    {
        if (IsSupportedServerDistribution &&
            NewVersionInfo.HasValue() &&
            !NewVersionInfo!.DisableAutomateUpdate)
        {
            IsNotStartUpdateing = false;
            DownloadUpdate();
        }
        else
        {
            OpenInAppStore();
        }
    }

    /// <summary>
    /// 当前应用设置的更新渠道
    /// </summary>
    [Obsolete]
    public static UpdateChannelType UpdateChannelType
    {
        get
        {
            var channel = GeneralSettings.UpdateChannel.Value;
            //switch (channel)
            //{
            //    case UpdateChannelType.GitHub:
            //    case UpdateChannelType.Gitee:
            //    case UpdateChannelType.Official:
            //        break;
            //    default:
            //        channel = ResourceService.IsChineseSimplified ?
            //            UpdateChannelType.Gitee : UpdateChannelType.GitHub;
            //        break;
            //}
            return channel;
        }
    }

    [Obsolete]
    protected virtual AppVersionDTODownload? GetByDownloadChannelSettings(IEnumerable<AppVersionDTODownload> downloads)
    {
        // 应该将首选渠道传入 API 请求，返回中取首个
        var channel = UpdateChannelType;
        return downloads.FirstOrDefault(x => x.HasValue() && x.DownloadChannelType == channel)
            ?? downloads.FirstOrDefault(x => x.HasValue());
    }
}
