using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Settings;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Properties;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Application.Browser2;
using static System.Application.Services.IApplicationUpdateService;
using CC = System.Common.Constants;
using FailCode = System.Application.Services.ApplicationUpdateFailCode;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IApplicationUpdateService"/>
    public abstract class ApplicationUpdateServiceBaseImpl : ReactiveObject, IApplicationUpdateService
    {
        protected const string TAG = "AppUpdateS";
        protected readonly ICloudServiceClient client;
        protected readonly AppSettings settings;
        protected readonly IToast toast;
        protected readonly IApplication application;

        public ICommand StartUpdateCommand { get; }

        public ApplicationUpdateServiceBaseImpl(
            IApplication application,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options)
        {
            this.toast = toast;
            this.client = client;
            this.application = application;
            settings = options.Value;
            StartUpdateCommand = ReactiveCommand.Create(StartUpdate);
        }

        protected virtual Version OSVersion => Environment.OSVersion.Version;

        /// <summary>
        /// 当更新下载完毕并校验完成时，即将退出程序之前
        /// </summary>
        protected virtual void OnExit()
        {
        }

        public virtual bool IsSupportedServerDistribution
        {
            get
            {
                if (DesktopBridge.IsRunningAsUwp ||
                    OperatingSystem2.IsOnlySupportedStore ||
                    OperatingSystem2.IsLinux ||
                    OperatingSystem2.IsMacOS)
                {
                    return false;
                }
                return true;
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
                var value = NewVersionInfo?.Description;
                if (string.IsNullOrWhiteSpace(value)) return string.Empty;
                var lines = value.Split(';');
                return string.Join(Environment.NewLine, lines);
            }
        }

        public string NewVersionInfoTitle => AppResources.NewVersionUpdateTitle_.Format(NewVersionInfo?.Version);

        /// <summary>
        /// 当存在新的版本时，重写此方法实现弹窗提示用户
        /// </summary>
        protected virtual async void OnExistNewVersion()
        {
            var result = await MessageBox.ShowAsync(NewVersionInfoDesc, AppResources.UpdateContent, MessageBox.Button.OKCancel);
            if (result.IsOK())
            {
                StartUpdateCommand.Invoke();
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

            //var id = settings.AppVersion;
            Guid id = default;
            var platform = DeviceInfo2.Platform;
            var deviceIdiom = DeviceInfo2.Idiom;
            if (deviceIdiom == DeviceIdiom.Tablet && platform == Platform.Apple)
            {
                deviceIdiom = DeviceIdiom.Phone;
            }
            var osVersion = OSVersion;
            var architecture = OperatingSystem2.IsWindows ?
                RuntimeInformation.ProcessArchitecture :
                RuntimeInformation.OSArchitecture;
            var deploymentMode = application.DeploymentMode;
            var rsp = await client.Version.CheckUpdate2(id,
                platform, deviceIdiom,
                osVersion,
                architecture,
                deploymentMode);
            if (rsp.IsSuccess)
            {
                if (!rsp.Content.HasValue() || rsp.Content?.Version == ThisAssembly.Version)
                {
                    IsExistUpdate = false;
                    if (showIsExistUpdateFalse) toast.Show(AppResources.IsExistUpdateFalse);
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

        protected void OnReportDownloading3(float value, int current, int count) => OnReport(value, AppResources.Downloading3_.Format(MathF.Round(value, 2), current, count));
        protected void OnReportCalcHashing3(float value, int current, int count) => OnReport(value, AppResources.CalcHashing3_.Format(MathF.Round(value, 2), current, count));
        protected void OnReportDownloading(float value) => OnReport(value, AppResources.Downloading_.Format(MathF.Round(value, 2)));
        protected void OnReportCalcHashing(float value) => OnReport(value, AppResources.CalcHashing_.Format(MathF.Round(value, 2)));
        protected void OnReportDecompressing(float value) => OnReport(value, AppResources.Decompressing_.Format(MathF.Round(value, 2)));
        protected void OnReport(float value = 0f) => OnReport(value, string.Empty);
        protected virtual void OnReport(float value, string str)
        {
            ProgressValue = value;
            ProgressString = str;
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
                onReportCalcHashing(CC.MaxProgress);
                return value;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        static string GetSingleFileUrl(string fileId) => $"{Prefix_HTTPS}steampp.net/uploads/publish/files/{fileId}{FileEx.BIN}";

        static string GetPackFileUrl(string fileName) => $"{Prefix_HTTPS}steampp.net/uploads/publish/{fileName}";

        /// <summary>
        /// 下载更新包文件到本地缓存
        /// </summary>
        protected async void DownloadUpdate()
        {
            var isCallOverwriteUpgrade = false;

            if (isDownloading) return;

            if (!IsSupportedServerDistribution) throw new PlatformNotSupportedException();

            isDownloading = true;

            OnReport();

            var newVersionInfo = NewVersionInfo;

            if (newVersionInfo.HasValue())
            {
                if (newVersionInfo!.DisableAutomateUpdate)
                {
                    OpenInAppStore();
                    goto end;
                }

                var isAndroid = OperatingSystem2.IsAndroid;
                var isDesktop = OperatingSystem2.IsDesktop;
                if (!isAndroid && !isDesktop)
                {
                    OpenInAppStore();
                    goto end;
                }

                if (!isAndroid && newVersionInfo.CurrentAllFiles.Any_Nullable() &&
                    newVersionInfo.CurrentAllFiles.All(x => x.HasValue()) &&
                    newVersionInfo.AllFiles.Any_Nullable() &&
                    newVersionInfo.AllFiles.All(x => x.HasValue())) // 增量更新 v2
                {
                    if (Path.DirectorySeparatorChar != '\\') // 路径分隔符在客户端系统上与服务端纪录的值不同时，替换分隔符
                    {
                        void CorrectionDirectorySeparatorChar(IEnumerable<AppVersionDTO.IncrementalUpdateDownload> items)
                        {
                            foreach (var item in items)
                            {
                                item.FileRelativePath =
                                    item.FileRelativePath?.Replace('\\',
                                        Path.DirectorySeparatorChar);
                            }
                        }
                        CorrectionDirectorySeparatorChar(newVersionInfo.AllFiles!);
                        CorrectionDirectorySeparatorChar(newVersionInfo.CurrentAllFiles!);
                    }

                    var packDirName = GetPackName(newVersionInfo, isDirOrFile: false);
                    var packDirPath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packDirName);
                    var packDirPathExists = Directory.Exists(packDirPath);

                    string GetDownloadPath(string fileRelativePath)
                        => packDirPath + Path.DirectorySeparatorChar + fileRelativePath;
                    static string GetFilePath(string fileRelativePath)
                        => IOPath.AppDataDirectory + Path.DirectorySeparatorChar + fileRelativePath;
                    var allFiles = newVersionInfo.AllFiles.ToDictionary(x => x, x => (
                        downloadPath: GetDownloadPath(x.FileRelativePath),
                        filePath: GetFilePath(x.FileRelativePath)
                    ));
                    var currentAllFiles = newVersionInfo.CurrentAllFiles.ToDictionary(x => x,
                        x => GetFilePath(x.FileRelativePath));

                    var hashFiles = new Dictionary<(string sha256, long fileLen), string>();

                    int i = 1;
                    void OnReportDownloading3_(float value) => OnReportDownloading3(value, i, allFiles.Count);
                    OnReportDownloading3_(0f);

                    foreach (var item in allFiles.Keys)
                    {
                        var (downloadPath, filePath) = allFiles[item]; // (downloadPath)文件下载存放路径，(filePath)文件要覆盖的路径
                        var hashFileKey = (item.SHA256!, item.Length); // 使用 sha256 与 文件大小 作为唯一标识
                        var fileInfo = new FileInfo(filePath);
                        if (!fileInfo.Directory.Exists) fileInfo.Directory.Create(); // 文件所在目录必须存在

                        #region 当前下载更新文件缓存文件夹存在，检查是否已下载了文件
                        if (packDirPathExists)
                        {
                            if (fileInfo.Exists)
                            {
                                if (fileInfo.Length == item.Length)
                                {
                                    using var fileStream = fileInfo.OpenRead();
                                    var sha256 = Hashs.String.SHA256(fileStream);
                                    if (sha256 == item.SHA256)
                                    {
                                        hashFiles.Add(hashFileKey, filePath);
                                        goto for_end; // 文件已下载，校验通过
                                    }
                                }
                                fileInfo.Delete(); // 文件已下载，校验不通过，删除文件重新下载
                            }
                        }
                        #endregion

                        #region 根据当前版本文件清单匹配新版本文件清单查找相同项
                        var query_current_files = from m in currentAllFiles
                                                  where m.Key.SHA256 == item.SHA256 && m.Key.Length == item.Length
                                                  let local_file_info = new FileInfo(m.Value)
                                                  where local_file_info.Exists
                                                  select local_file_info;
                        if (query_current_files.Any(x => x.FullName == fileInfo.FullName)) // 如果路径相同，则忽略
                        {
                            goto for_end;
                        }
                        var query_current_file = query_current_files.FirstOrDefault();
                        if (query_current_file != null) // 如果路径不同，则复制
                        {
                            hashFiles.Add(hashFileKey, query_current_file.FullName);
                            File.Copy(query_current_file.FullName, fileInfo.FullName);
                            goto for_end;
                        }
                        #endregion

                        #region 根据哈希值匹配已有文件
                        if (hashFiles.ContainsKey(hashFileKey))
                        {
                            var hashFilePath = hashFiles[hashFileKey];
                            File.Copy(hashFilePath, fileInfo.FullName);
                            goto for_end;
                        }
                        #endregion

                        #region 下载文件，并加入 hashFiles 中
                        var cacheFileDownloadPath = downloadPath + FileEx.DownloadCache;
                        var requestUri = GetSingleFileUrl(item.FileId!);

                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFileDownloadPath,
                            null);
                        if (rsp.IsSuccess)
                        {
                            File.Move(cacheFileDownloadPath, downloadPath);

                            if (!UpdatePackVerification(downloadPath, item.SHA256!, i, allFiles.Count))
                            {
                                Fail(FailCode.UpdatePackVerificationFail);
                                break;
                            }

                            hashFiles.Add(hashFileKey, downloadPath);
                        }
                        else
                        {
                            Fail(FailCode.DownloadUpdateFail);
                            break;
                        }
                    #endregion

                    for_end: i++;
                        OnReportDownloading3_(i / (float)allFiles.Count * CC.MaxProgress);
                    }

                    OnReport(CC.MaxProgress);
                    OverwriteUpgrade(packDirPath, isIncrement: true);
                }
                else // 全量更新
                {
                    AppVersionDTO.Download? download = null;
                    if (newVersionInfo.Downloads != null)
                    {
                        if (isAndroid)
                        {
                            download = GetByDownloadChannelSettings(newVersionInfo.Downloads.Where(x => x.DownloadType == AppDownloadType.Install));
                        }
                        else if (isDesktop)
                        {
                            download = GetByDownloadChannelSettings(newVersionInfo.Downloads.Where(x => x.DownloadType == AppDownloadType.Compressed_7z));
                            if (download == null)
                            {
                                download = GetByDownloadChannelSettings(newVersionInfo.Downloads.Where(x => x.DownloadType == AppDownloadType.Compressed_GZip));
                            }
                        }
                        else
                        {
                            throw new PlatformNotSupportedException();
                        }
                    }
                    if (download.HasValue()) // 压缩包格式是否正确
                    {
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
                                toast.Show(AppResources.UpdatePackCacheHashInvalidDeleteFileTrue);
                            }
                        }

                        string filePlatform;
                        string fileArch;
                        var fileEx = newVersionInfo.Platform switch
                        {
                            Platform.Android => FileEx.APK,
                            Platform.Windows or Platform.Linux or Platform.Apple => download!.DownloadType switch
                            {
                                AppDownloadType.Compressed_GZip => FileEx.TAR_GZ,
                                AppDownloadType.Compressed_7z => FileEx._7Z,
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
                        if (string.IsNullOrWhiteSpace(download!.FileIdOrUrl))
                        {
                            var downloadFileName = $"{filePlatform}-{fileArch}_{newVersionInfo.Version}{fileEx}";
                            requestUri = GetPackFileUrl(downloadFileName);
                        }
                        else if (IsHttpUrl(download.FileIdOrUrl))
                        {
                            requestUri = download.FileIdOrUrl;
                        }
                        else
                        {
                            requestUri = GetSingleFileUrl(download.FileIdOrUrl!);
                        }

                        var cacheFilePath = packFilePath + FileEx.DownloadCache;
                        OnReportDownloading(0f);
                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFilePath,
                            new Progress<float>(OnReportDownloading));
                        OnReportDownloading(CC.MaxProgress);

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
            }

        end: isDownloading = false;
            if (!isCallOverwriteUpgrade)
            {
                IsNotStartUpdateing = true;
            }
            void Fail(FailCode failCode, params string[] args)
            {
                var error = failCode.ToString2(args);
                toast.Show(error);
                Open(string.Format(UrlConstants.OfficialWebsite_ApplicationUpdateFailCode_, (byte)failCode));
                OnReport(CC.MaxProgress);
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
        protected virtual async void OverwriteUpgrade(string value, bool isIncrement, AppDownloadType downloadType = default)
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
                            AppDownloadType.Compressed_GZip
                                => TarGZipHelper.Unpack(value, dirPath,
                                    progress: new Progress<float>(OnReportDecompressing),
                                    maxProgress: CC.MaxProgress),
                            AppDownloadType.Compressed_7z
                                => ISevenZipHelper.Instance.Unpack(value, dirPath,
                                    progress: new Progress<float>(OnReportDecompressing),
                                    maxProgress: CC.MaxProgress),
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
                    OnReport(CC.MaxProgress);
                    IOPath.FileTryDelete(value);
                    OverwriteUpgradePrivate(dirPath);
                }
                else
                {
                    toast.Show(AppResources.UpdateUnpackFail);
                    OnReport(CC.MaxProgress);
                    IsNotStartUpdateing = true;
                }
            }

            void OverwriteUpgradePrivate(string dirPath)
            {
                OnExit();

                var updateCommandPath = Path.Combine(IOPath.CacheDirectory, "update.cmd");
                IOPath.FileIfExistsItDelete(updateCommandPath);

                var updateCommand = string.Format(
                   SR.ProgramUpdateCmd_,
                   IApplication.ProgramName,
                   dirPath.TrimEnd(Path.DirectorySeparatorChar),
                   IOPath.BaseDirectory,
                   IApplication.ProgramPath);

                updateCommand = "chcp 65001" + Environment.NewLine + updateCommand;

                File.WriteAllText(updateCommandPath, updateCommand, Encoding.Default);

                using var p = new Process();
                p.StartInfo.FileName = updateCommandPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = !ThisAssembly.Debuggable; // 不显示程序窗口
                p.StartInfo.Verb = "runas"; // 管理员权限运行
                p.Start(); // 启动程序
            }
        }

        /// <summary>
        /// 在应用商店中打开
        /// </summary>
        protected virtual async void OpenInAppStore()
        {
            if (DesktopBridge.IsRunningAsUwp)
            {
                await OpenAsync(UrlConstants.MicrosoftStoreProtocolLink);
            }
            else
            {
                await OpenAsync(UrlConstants.OfficialWebsite);
            }
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
        public static UpdateChannelType UpdateChannelType
        {
            get
            {
                var channel = GeneralSettings.UpdateChannel.Value;
                switch (channel)
                {
                    case UpdateChannelType.GitHub:
                    case UpdateChannelType.Gitee:
                        break;
                    default:
                        channel = R.IsChineseSimplified ? UpdateChannelType.Gitee : UpdateChannelType.GitHub;
                        break;
                }
                return channel;
            }
        }

        protected virtual AppVersionDTO.Download? GetByDownloadChannelSettings(IEnumerable<AppVersionDTO.Download> downloads)
        {
            var channel = UpdateChannelType;
            return downloads.FirstOrDefault(x => x.DownloadChannelType == channel)
                ?? downloads.FirstOrDefault();
        }
    }
}
