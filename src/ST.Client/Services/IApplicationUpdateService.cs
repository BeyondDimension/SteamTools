using System.Application.Models;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.Services
{
    /// <summary>
    /// 应用程序更新服务
    /// </summary>
    public interface IApplicationUpdateService
    {
        static IApplicationUpdateService Instance => DI.Get<IApplicationUpdateService>();

        /// <summary>
        /// 升级包存放文件夹名称
        /// </summary>
        const string PackDirName = "UpgradePackages";

        bool IsNotStartUpdateing { get; }

        /// <summary>
        /// 进度值
        /// </summary>
        float ProgressValue { get; }

        /// <summary>
        /// 进度值描述
        /// </summary>
        string ProgressString { get; }

        /// <summary>
        /// 是否支持服务端分发，如果返回 <see langword="false"/> 将不能使用 DownloadAsync 与 OverwriteUpgradeAsync
        /// <para>对于 iOS 平台，必须使用 App Store 分发</para>
        /// </summary>
        bool IsSupportedServerDistribution { get; }

        /// <summary>
        /// 是否有更新
        /// </summary>
        bool IsExistUpdate { get; }

        /// <summary>
        /// 新版本信息
        /// </summary>
        AppVersionDTO? NewVersionInfo { get; }

        string NewVersionInfoDesc { get; }

        string NewVersionInfoTitle { get; }

        /// <inheritdoc cref="CheckUpdateAsync(bool, bool)"/>
        async void CheckUpdate(bool force = false, bool showIsExistUpdateFalse = true)
        {
            await CheckUpdateAsync(force, showIsExistUpdateFalse);
        }

        /// <summary>
        /// 检查更新，返回新版本信息
        /// </summary>
        /// <param name="force">是否强制检查，如果为 <see langword="false"/> 当有新版本内存中缓存时将跳过 api 请求</param>
        /// <param name="showIsExistUpdateFalse">是否显示已是最新版本吐司提示</param>
        Task CheckUpdateAsync(bool force = false, bool showIsExistUpdateFalse = true);

        ICommand StartUpdateCommand { get; }

        /// <summary>
        /// 根据新版本信息获取升级包路径名
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected static string GetPackName(AppVersionDTO m, bool isDirOrFile) => $"{m.Version}@{Hashs.String.Crc32(m.Id.ToByteArray())}{(isDirOrFile ? "" : $"{FileEx.TAR_GZ}")}";

        /// <summary>
        /// 获取存放升级包缓存文件夹的目录
        /// </summary>
        /// <param name="onNotExistsCreateDirectory">当目录不存在时是否创建目录</param>
        /// <param name="exists">目录是否存在</param>
        /// <returns></returns>
        protected static string GetPackCacheDirPath(bool onNotExistsCreateDirectory, out bool exists)
        {
            var dirPath = Path.Combine(IOPath.CacheDirectory, PackDirName);
            exists = Directory.Exists(dirPath);
            if (!exists && onNotExistsCreateDirectory)
            {
                Directory.CreateDirectory(dirPath);

            }
            return dirPath;
        }

        /// <summary>
        /// 获取存放升级包缓存文件夹的目录
        /// </summary>
        /// <param name="clear">是否需要清理之前的缓存</param>
        /// <returns></returns>
        protected static string GetPackCacheDirPath(bool clear)
        {
            var dirPath = GetPackCacheDirPath(true, out var exists);
            if (exists && clear)
            {
                var files = Directory.GetFiles(dirPath, "*" + FileEx.DownloadCache);
                foreach (var item in files)
                {
                    IOPath.FileTryDelete(item);
                }
            }
            return dirPath;
        }

        /// <summary>
        /// 清理存放升级包缓存文件夹的目录
        /// </summary>
        static void ClearAllPackCacheDir()
        {
            var dirPath = GetPackCacheDirPath(true, out var exists);
            if (exists)
            {
                var files = Directory.GetFiles(dirPath);
                foreach (var item in files)
                {
                    IOPath.FileTryDelete(item);
                }
            }
        }

        /// <summary>
        /// 是否需要在主窗口显示时显示新版本通知窗口
        /// </summary>
        bool ShowNewVersionWindowOnMainOpen { get; }

        /// <summary>
        /// 在主窗口显示时调用此函数检查是否需要显示新版本通知窗口
        /// </summary>
        void OnMainOpenTryShowNewVersionWindow();
    }
}