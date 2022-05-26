#if NET35
using WinFormsApplication = System.Windows.Forms.Application;
#endif

// ReSharper disable once CheckNamespace
namespace System.Application;

/// <summary>
/// 提供了访问设备文件夹位置的简便方法。
/// </summary>
public sealed partial class FileSystem2 : IOPath.FileSystemBase
{
    private FileSystem2() => throw new NotSupportedException();

    public static class BaseDirectory
    {
        static string StartupPath
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
                var appDataPath = Path.Combine(StartupPath, IOPath.DirName_AppData);
                return appDataPath;
            }
        }

        public static string CacheDirectory
        {
            get
            {
                var cachePath = Path.Combine(StartupPath, IOPath.DirName_Cache);
                return cachePath;
            }
        }
    }

    public static void InitFileSystemByBaseDirectory()
    {
        var appDataPath = BaseDirectory.AppDataDirectory;
        var cachePath = BaseDirectory.CacheDirectory;
        IOPath.DirCreateByNotExists(appDataPath);
        IOPath.DirCreateByNotExists(cachePath);
        InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
        string GetAppDataDirectory() => appDataPath;
        string GetCacheDirectory() => cachePath;
    }
}
