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
    public sealed class FileSystemDesktop : IOPath.FileSystemBase
    {
        private FileSystemDesktop() => throw new NotSupportedException();

        static string BaseDirectory
        {
            get
            {
                var appDataRootPath =
#if NET35
                WinFormsApplication.StartupPath;
#else
                IOPath.BaseDirectory;
#endif
                return appDataRootPath;
            }
        }

        public static string AppDataDirectory
        {
            get
            {
                var appDataPath = Path.Combine(BaseDirectory, IOPath.DirName_AppData);
                return appDataPath;
            }
        }

        public static string CacheDirectory
        {
            get
            {
                var cachePath = Path.Combine(BaseDirectory, IOPath.DirName_Cache);
                return cachePath;
            }
        }

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        public static void InitFileSystem()
        {
            var appDataPath = AppDataDirectory;
            var cachePath = CacheDirectory;
            IOPath.DirCreateByNotExists(appDataPath);
            IOPath.DirCreateByNotExists(cachePath);
            InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            string GetAppDataDirectory() => appDataPath;
            string GetCacheDirectory() => cachePath;
        }
    }
}