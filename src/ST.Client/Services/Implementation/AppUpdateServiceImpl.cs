using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.Properties;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static System.Application.Services.IAppUpdateService;

namespace System.Application.Services.Implementation
{
    public abstract class AppUpdateServiceImpl : IAppUpdateService
    {
        protected readonly ICloudServiceClient client;
        protected readonly AppSettings settings;
        protected readonly IToast toast;

        public AppUpdateServiceImpl(
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options)
        {
            this.toast = toast;
            this.client = client;
            settings = options.Value;
            SupportedAbis = GetSupportedAbis();
        }

        protected virtual ArchitectureFlags GetSupportedAbis()
        {
            var architecture = RuntimeInformation.OSArchitecture;
            return architecture.Convert(hasFlags: true);
        }

        protected ArchitectureFlags SupportedAbis { get; }

        protected abstract Version OSVersion { get; }

        public bool IsSupportedServerDistribution
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

        public AppVersionDTO? NewVersionInfo { get; protected set; }

        /// <summary>
        /// 当存在新的版本时，重写此方法实现弹窗提示用户
        /// </summary>
        protected abstract void OnExistNewVersion();

        static bool isCheckUpdateing;

        public async void CheckUpdate()
        {
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
                    toast.Show(SR.IsExistUpdateFalse);
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

        const string FileExDownloadCache = ".download_cache";

        /// <summary>
        /// 根据新版本信息获取升级包路径名
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static string GetPackName(AppVersionDTO m, bool isDirOrFile) => $"{m.Version}@{Hashs.String.Crc32(m.Id.ToStringN())}{(isDirOrFile ? "" : $".{FileEx.TAR_GZ}")}";

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
                var files = Directory.GetFiles("*" + FileExDownloadCache);
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

        public float CurrentProgressValue { get; protected set; }

        public float TotalProgressValue { get; protected set; }

        protected void ClearProgressValue(float value = 0f)
        {
            CurrentProgressValue = value;
            TotalProgressValue = value;
        }

        bool UpdatePackVerification(string filePath, string sha256, bool notChangeProgress = false)
        {
            var sha256_ = Hashs.String.SHA256(File.OpenRead(filePath));  // 改为带进度的哈希计算
            if (!notChangeProgress && TotalProgressValue < MaxProgressValue) TotalProgressValue += 10;
            return string.Equals(sha256_, sha256, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 下载更新包文件到本地缓存
        /// </summary>
        protected async void DownloadUpdate()
        {
            if (isDownloading) return;

            if (!IsSupportedServerDistribution) throw new PlatformNotSupportedException();

            isDownloading = true;

            ClearProgressValue();

            var newVersionInfo = NewVersionInfo;

            if (newVersionInfo.HasValue())
            {
                if (newVersionInfo.IncrementalUpdate.Any_Nullable())
                {
                    if (Path.DirectorySeparatorChar != '\\')
                    {
                        foreach (var item in newVersionInfo.IncrementalUpdate)
                        {
                            item.FileRelativePath =
                                item.FileRelativePath?.Replace('\\', Path.DirectorySeparatorChar);
                        }
                    }

                    // 增量更新
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

                    foreach (var item in incrementalUpdate)
                    {
                        var fileName = item.Value;
                        var cacheFileName = fileName + FileExDownloadCache;
                        var requestUri = AppVersionDTO.GetRequestUri(item.Key.FileId);

                        OnReport(0);
                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFileName,
                            new Progress<float>(OnReport));
                        if (rsp.IsSuccess)
                        {
                            File.Move(cacheFileName, fileName);

                            if (!UpdatePackVerification(fileName, item.Key.SHA256, notChangeProgress: true))
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

                        TotalProgressValue += incrementalUpdate.Count / 100f;
                    }

                    ClearProgressValue(MaxProgressValue);
                    OverwriteUpgrade(packDirPath, isIncrement: true);
                }
                else
                {
                    // 全量更新
                    var compressed = newVersionInfo.Downloads.FirstOrDefault(x => x.DownloadType == AppDownloadType.Compressed);
                    if (compressed.HasValue())
                    {
                        var packFileName = GetPackName(newVersionInfo, isDirOrFile: true);
                        var packFilePath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packFileName);
                        if (File.Exists(packFilePath)) // 存在安装包文件
                        {
                            if (UpdatePackVerification(packFilePath, compressed.SHA256)) // (已有文件)哈希验证成功，进行覆盖安装
                            {
                                OverwriteUpgrade(packFilePath, isIncrement: false);
                                goto end;
                            }
                            else // 验证文件失败，删除源文件，将会重新下载 --提示用户，然后结束本次逻辑
                            {
                                File.Delete(packFilePath);
                            }
                        }

                        var cacheFileName = packFileName + FileExDownloadCache;

                        var requestUri = AppVersionDTO.GetRequestUri(compressed.FileId);
                        var rsp = await client.Download(
                            isAnonymous: true,
                            requestUri: requestUri,
                            cacheFileName,
                            new Progress<float>(OnReport));

                        if (rsp.IsSuccess)
                        {
                            File.Move(cacheFileName, packFileName);

                            if (UpdatePackVerification(packFilePath, compressed.SHA256)) // (下载文件)哈希验证成功，进行覆盖安装
                            {
                                OverwriteUpgrade(packFileName, isIncrement: false);
                                OnReport(MaxProgressValue);
                                if (TotalProgressValue < 50) TotalProgressValue = 50;
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
                void OnReport(float value) => CurrentProgressValue = value;
            }

        end: isDownloading = false;
            void Fail(string error)
            {
                toast.Show(error);
                ClearProgressValue(MaxProgressValue);
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
        protected abstract void OpenInAppStore();
    }
}