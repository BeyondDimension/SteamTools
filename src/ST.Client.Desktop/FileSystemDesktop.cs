using System.IO;
#if NET35
using WinFormsApplication = System.Windows.Forms.Application;
#endif

namespace System.Application
{
    /// <summary>
    /// 适用于桌面端的文件系统帮助类，参考 Xamarin.Essentials.FileSystem
    /// <para><see cref="https://docs.microsoft.com/zh-cn/xamarin/essentials/file-system-helpers?tabs=uwp"/></para>
    /// </summary>
    public static class FileSystemDesktop
    {
        public const string AppDataDirName = "AppData";
        public const string CacheDirName = "Cache";

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        public static void InitFileSystem()
        {
            //var appDataRootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //if (string.IsNullOrWhiteSpace(appDataRootPath))
            //{
            //    // 需要测试 macOS 和 linux 上 Environment.SpecialFolder.ApplicationData 返回的目录值
            //    // 之前在 android 上测试过 Environment.GetFolderPath 有些枚举值返回的是空字符串 比如StartMenu
            //    throw new ArgumentNullException(nameof(appDataRootPath));
            //}
            //appDataRootPath = Path.Combine(appDataRootPath, BuildConfig.APPLICATION_ID);

            var appDataRootPath =
#if NET35
                WinFormsApplication.StartupPath;
#else
                IOPath.BaseDirectory;
#endif

            var appDataPath = Path.Combine(appDataRootPath, AppDataDirName);
            var cachePath = Path.Combine(appDataRootPath, CacheDirName);
            IOPath.DirCreateByNotExists(appDataPath);
            IOPath.DirCreateByNotExists(cachePath);
            IOPath.InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            string GetAppDataDirectory() => appDataPath;
            string GetCacheDirectory() => cachePath;
        }

#if DEBUG
        /// <inheritdoc cref="Xamarin.Essentials.FileSystem.AppDataDirectory"/>
        [Obsolete("use IOPath.AppDataDirectory", true)]
        public static string AppDataDirectory => IOPath.AppDataDirectory;

        /// <inheritdoc cref="Xamarin.Essentials.FileSystem.CacheDirectory"/>
        [Obsolete("use IOPath.CacheDirectory", true)]
        public static string CacheDirectory => IOPath.CacheDirectory;
#endif
    }
}