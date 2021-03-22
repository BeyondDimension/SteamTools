using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.Properties;
using System.IO;
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
            return architecture switch
            {
                Architecture.Arm => ArchitectureFlags.Arm,
                Architecture.Arm64 => ArchitectureFlags.Arm64 | ArchitectureFlags.Arm,
                Architecture.X64 => ArchitectureFlags.X64 | ArchitectureFlags.X86,
                Architecture.X86 => ArchitectureFlags.X86,
                _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, null),
            };
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
            var rsp = await client.Version.CheckUpdate(id,
                platform, deviceIdiom,
                supportedAbis,
                osVersion);
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
        const string PackDirName = "-Pack";

        const string FileExDownloadCache = ".download_cache";

        /// <summary>
        /// 根据新版本信息获取升级包文件名
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        static string GetPackFileName(AppVersionDTO m)
            => $"{m.Version}@{Hashs.String.Crc32(m.Id.ToStringN())}.{FileEx.TAR_GZ}";

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

        bool UpdatePackVerification(string filePath, string sha256)
        {
            var sha256_ = Hashs.String.SHA256(File.OpenRead(filePath));  // 改为带进度的哈希计算
            if (TotalProgressValue < MaxProgressValue) TotalProgressValue += 10;
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
            if (newVersionInfo.HasValue() && newVersionInfo?.DownloadLink != null && newVersionInfo.SHA256 != null)
            {
                var packFileName = GetPackFileName(newVersionInfo);
                var packFilePath = Path.Combine(GetPackCacheDirPath(!isSupportedResume), packFileName);
                if (File.Exists(packFilePath)) // 存在安装包文件
                {
                    if (UpdatePackVerification(packFilePath, newVersionInfo.SHA256)) // (已有文件)哈希验证成功，进行覆盖安装
                    {
                        OverwriteUpgrade(packFilePath);
                        goto end;
                    }
                    else // 验证文件失败，删除源文件，将会重新下载 --提示用户，然后结束本次逻辑
                    {
                        File.Delete(packFilePath);
                    }
                }

                var cacheFileName = packFileName + FileExDownloadCache;

                CurrentProgressValue = 0;

                var rsp = await client.Download(isAnonymous: true,
                    newVersionInfo.DownloadLink, cacheFileName, new Progress<float>(OnReport));

                void OnReport(float value) => CurrentProgressValue = value;

                if (rsp.IsSuccess)
                {
                    File.Move(cacheFileName, packFileName);

                    if (UpdatePackVerification(packFilePath, newVersionInfo.SHA256)) // (下载文件)哈希验证成功，进行覆盖安装
                    {
                        OverwriteUpgrade(packFileName);
                        OnReport(MaxProgressValue);
                        if (TotalProgressValue < 50) TotalProgressValue = 50;
                    }
                    else
                    {
                        toast.Show(SR.UpdatePackVerificationFail);
                        ClearProgressValue(MaxProgressValue);
                    }
                }
                else // 下载失败，进度条填满，可能服务器崩了
                {
                    toast.Show(SR.DownloadUpdateFail);
                    ClearProgressValue(MaxProgressValue);
                }
            }

        end: isDownloading = false;
        }

        /// <summary>
        /// 覆盖更新，根据更新包解压覆盖文件
        /// <para>Android：打开apk包即可</para>
        /// </summary>
        protected abstract void OverwriteUpgrade(string fileName);

        /// <summary>
        /// 在应用商店中打开
        /// </summary>
        protected abstract void OpenInAppStore();
    }
}