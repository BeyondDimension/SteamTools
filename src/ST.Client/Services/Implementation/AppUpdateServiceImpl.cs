using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Application.Models;
using System.Application.Properties;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Input;
using static System.Application.Services.CloudService.Constants;
using static System.Application.Services.IAppUpdateService;

namespace System.Application.Services.Implementation
{
    public abstract class AppUpdateServiceImpl : ReactiveObject, IAppUpdateService
    {
        protected readonly ICloudServiceClient client;
        protected readonly AppSettings settings;
        protected readonly IToast toast;

        public ICommand StartUpdateCommand { get; }

        public AppUpdateServiceImpl(
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options)
        {
            this.toast = toast;
            this.client = client;
            settings = options.Value;
            SupportedAbis = GetSupportedAbis();
            StartUpdateCommand = ReactiveCommand.Create(StartUpdate);
        }

        protected virtual ArchitectureFlags GetSupportedAbis()
        {
            var architecture = RuntimeInformation.OSArchitecture;
            return architecture.Convert(hasFlags: true);
        }

        protected ArchitectureFlags SupportedAbis { get; }

        protected abstract Version OSVersion { get; }

        public virtual bool IsSupportedServerDistribution
        {
            get
            {
                if (DI.Platform == Platform.Apple && DI.DeviceIdiom != DeviceIdiom.Desktop)
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
            protected set
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
                var lines = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
                return string.Join(Environment.NewLine, lines);
            }
        }

        public string NewVersionInfoTitle => SR.NewVersionUpdateTitle_.Format(NewVersionInfo?.Version);

        /// <summary>
        /// 当存在新的版本时，重写此方法实现弹窗提示用户
        /// </summary>
        protected abstract void OnExistNewVersion();

        static bool isCheckUpdateing;

        public async void CheckUpdate(bool force, bool showIsExistUpdateFalse = true)
        {
            if (!force && IsExistUpdate)
            {
                if (NewVersionInfo.HasValue())
                {
                    OnExistNewVersion();
                }
                else
                {
                    IsExistUpdate = false;
                }
            }

            if (isCheckUpdateing) return;
            isCheckUpdateing = true;

            var id = settings.AppVersion;
            var platform = DI.Platform;
            var deviceIdiom = DI.DeviceIdiom;
            if (deviceIdiom == DeviceIdiom.Tablet && platform == Platform.Apple)
            {
                deviceIdiom = DeviceIdiom.Phone;
            }
            var supportedAbis = SupportedAbis;
            var osVersion = OSVersion;
            var abi = RuntimeInformation.ProcessArchitecture.Convert(hasFlags: false);
            var rsp = await client.Version.CheckUpdate(id,
                platform, deviceIdiom,
                supportedAbis,
                osVersion,
                abi);
            if (rsp.IsSuccess)
            {
                if (!rsp.Content.HasValue())
                {
                    IsExistUpdate = false;
                    if (showIsExistUpdateFalse) toast.Show(SR.IsExistUpdateFalse);
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

        /// <summary>
        /// 升级包存放文件夹名称
        /// </summary>
        const string PackDirName = "UpgradePackage";

        public const string FileExDownloadCache = ".download_cache";

        /// <summary>
        /// 根据新版本信息获取升级包路径名
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static string GetPackName(AppVersionDTO m, bool isDirOrFile) => $"{m.Version}@{Hashs.String.Crc32(m.Id.ToByteArray())}{(isDirOrFile ? "" : $".{FileEx.TAR_GZ}")}";

        /// <summary>
        /// 获取存放升级包缓存文件夹的目录
        /// </summary>
        /// <param name="clear">是否需要清理之前的缓存</param>
        /// <returns></returns>
        static string GetPackCacheDirPath(bool clear)
        {
            var dirPath = Path.Combine(IOPath.CacheDirectory, PackDirName);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            else if (clear)
            {
                var files = Directory.GetFiles(dirPath, "*" + FileExDownloadCache);
                foreach (var item in files)
                {
                    IOPath.FileTryDelete(item);
                }
            }
            return dirPath;
        }

        static bool isDownloading;

        /// <summary>
        /// 是否支持断点续传
        /// </summary>
        const bool isSupportedResume = false;

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

        protected void OnReportDownloading3(float value, int current, int count) => OnReport(value, SR.Downloading3_.Format(value, current, count));
        protected void OnReportCalcHashing3(float value, int current, int count) => OnReport(value, SR.CalcHashing3_.Format(value, current, count));
        protected void OnReportDownloading(float value) => OnReport(value, SR.Downloading_.Format(value));
        protected void OnReportCalcHashing(float value) => OnReport(value, SR.CalcHashing_.Format(value));
        protected void OnReportDecompressing(float value) => OnReport(value, SR.Decompressing_.Format(value));
        protected void OnReport(float value = 0f) => OnReport(value, string.Empty);
        protected void OnReport(float value, string str)
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
            var sha256_ = Hashs.String.SHA256(File.OpenRead(filePath));  // 改为带进度的哈希计算
            var value = string.Equals(sha256_, sha256, StringComparison.OrdinalIgnoreCase);
            onReportCalcHashing(MaxProgressValue);
            return value;
        }

        static string GetSingleFileUrl(string fileId) => $"{Prefix_HTTPS}steampp.net/uploads/publish/files/{fileId}{FileEx.BIN}";

        static string GetPackFileUrl(string fileName) => $"{Prefix_HTTPS}steampp.net/uploads/publish/{fileName}";

        /// <summary>
        /// 下载更新包文件到本地缓存
        /// </summary>
        protected async void DownloadUpdate()
        {
            if (isDownloading) return;

            if (!IsSupportedServerDistribution) throw new PlatformNotSupportedException();

            isDownloading = true;

            OnReport();

            var newVersionInfo = NewVersionInfo;

            if (newVersionInfo.HasValue())
            {
                if (newVersionInfo.IncrementalUpdate.Any_Nullable()) // 增量更新
                {
                    if (Path.DirectorySeparatorChar != '\\')
                    {
                        foreach (var item in newVersionInfo.IncrementalUpdate)
                        {
                            item.FileRelativePath =
                                item.FileRelativePath?.Replace('\\', Path.DirectorySeparatorChar);
                        }
                    }

                    var packDirName = GetPackName(newVersionInfo, isDirOrFile: false);
                    var packDirPath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packDirName);

                    var incrementalUpdate = newVersionInfo.IncrementalUpdate.ToDictionary(x => x, x => packDirPath + Path.DirectorySeparatorChar + x.FileRelativePath);

                    if (Directory.Exists(packDirPath))
                    {
                        foreach (var item in newVersionInfo.IncrementalUpdate)
                        {
                            var filePath = incrementalUpdate[item];
                            var fileInfo = new FileInfo(filePath);
                            if (fileInfo.Exists)
                            {
                                if (fileInfo.Length == item.Length)
                                {
                                    using var fileStream = fileInfo.OpenRead();
                                    var sha256 = Hashs.String.SHA256(fileStream);
                                    if (sha256 == item.SHA256)
                                    {
                                        incrementalUpdate.Remove(item);
                                    }
                                }
                                fileInfo.Delete();
                            }
                        }
                    }

                    int i = 1;
                    foreach (var item in incrementalUpdate)
                    {
                        void OnReportDownloading3_(float value) => OnReportDownloading3(value, i, incrementalUpdate.Count);

                        var fileName = item.Value;
                        var cacheFileName = fileName + FileExDownloadCache;
                        var requestUri = GetSingleFileUrl(item.Key.FileId);

                        OnReportDownloading3_(0f);
                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFileName,
                            new Progress<float>(OnReportDownloading3_));
                        if (rsp.IsSuccess)
                        {
                            File.Move(cacheFileName, fileName);

                            if (!UpdatePackVerification(fileName, item.Key.SHA256, i, incrementalUpdate.Count))
                            {
                                Fail(SR.UpdatePackVerificationFail);
                                break;
                            }
                        }
                        else
                        {
                            Fail(SR.DownloadUpdateFail);
                            break;
                        }
                        i++;
                    }

                    OnReport(MaxProgressValue);
                    OverwriteUpgrade(packDirPath, isIncrement: true);
                }
                else // 全量更新
                {
                    var downloadType = DI.Platform switch
                    {
                        Platform.Windows or Platform.Linux or Platform.Apple => AppDownloadType.Compressed,
                        Platform.Android => AppDownloadType.Install,
                        _ => throw new PlatformNotSupportedException(),
                    };
                    var download = newVersionInfo.Downloads.FirstOrDefault(x => x.DownloadType == downloadType);
                    if (download.HasValue()) // 压缩包格式是否正确
                    {
                        var packFileName = GetPackName(newVersionInfo, isDirOrFile: true);
                        var packFilePath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packFileName);
                        if (File.Exists(packFilePath)) // 存在压缩包文件
                        {
                            if (UpdatePackVerification(packFilePath, download.SHA256)) // (已有文件)哈希验证成功，进行覆盖安装
                            {
                                OverwriteUpgrade(packFilePath, isIncrement: false);
                                goto end;
                            }
                            else // 验证文件失败，删除源文件，将会重新下载
                            {
                                File.Delete(packFilePath);
                            }
                        }

                        string fileEx;
                        string filePlatform;
                        string fileArch;
                        switch (newVersionInfo.Platform)
                        {
                            case Platform.Android:
                                fileEx = FileEx.APK;
                                break;
                            case Platform.Windows:
                            case Platform.Linux:
                            case Platform.Apple:
                                fileEx = FileEx.TAR_GZ;
                                break;
                            default:
                                Fail(SR.UpdateEnumOutOfRange);
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
                                Fail(SR.UpdateEnumOutOfRange);
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
                            default:
                                fileArch = ((int)newVersionInfo.SupportedAbis).ToString();
                                break;
                            case 0:
                            case ArchitectureFlags.Arm:
                                Fail(SR.UpdateEnumOutOfRange);
                                goto end;
                        }
                        var downloadFileName = $"{filePlatform}-{fileArch}_{newVersionInfo.Version}{fileEx}";
                        var requestUri = GetPackFileUrl(downloadFileName);
                        var cacheFilePath = packFilePath + FileExDownloadCache;
                        OnReportDownloading(0f);
                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFilePath,
                            new Progress<float>(OnReportDownloading));

                        if (rsp.IsSuccess)
                        {
                            File.Move(cacheFilePath, packFilePath);

                            if (UpdatePackVerification(packFilePath, download.SHA256)) // (下载文件)哈希验证成功，进行覆盖安装
                            {
                                OverwriteUpgrade(packFileName, isIncrement: false);
                                OnReportDownloading(MaxProgressValue);
                            }
                            else
                            {
                                Fail(SR.UpdatePackVerificationFail);
                            }
                        }
                        else // 下载失败，进度条填满，可能服务器崩了
                        {
                            Fail(SR.DownloadUpdateFail);
                        }
                    }
                    else
                    {
                        Fail("The new version compressed package is missing, please contact the administrator.");
                    }
                }
            }

        end: isDownloading = false;
            void Fail(string error)
            {
                toast.Show(error);
                OnReport(MaxProgressValue);
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
        protected abstract void OverwriteUpgrade(string value, bool isIncrement);

        /// <summary>
        /// 在应用商店中打开
        /// </summary>
        protected virtual void OpenInAppStore() => BrowserOpen("https://steampp.net/");

        void StartUpdate()
        {
            if (IsSupportedServerDistribution)
            {
                DownloadUpdate();
            }
            else
            {
                OpenInAppStore();
            }
        }
    }
}