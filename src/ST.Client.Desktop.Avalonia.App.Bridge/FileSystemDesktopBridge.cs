using System.Runtime.Versioning;
using Windows.Storage;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    [SupportedOSPlatform("Windows10.0.10240.0")]
    sealed class FileSystemDesktopBridge : IOPath.FileSystemBase
    {
        private FileSystemDesktopBridge() => throw new NotSupportedException();

        internal static void InitFileSystem()
        {
            // https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/FileSystem/FileSystem.uwp.cs#L12
            // https://docs.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-behind-the-scenes
            // 不允许在 C:\Program Files\WindowsApps\package_name 下写入。

            var destAppDataPath = ApplicationData.Current.LocalFolder.Path;
            var destCachePath = ApplicationData.Current.LocalCacheFolder.Path;

            // %HOMEPATH%\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalState
            // %HOMEPATH%\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalCache
            var sourceAppDataPath = ApplicationData.Current.LocalFolder.Path.Replace(DEST_PACKAGE_ID, SOURCE_PACKAGE_ID);
            var sourceCachePath = ApplicationData.Current.LocalCacheFolder.Path.Replace(DEST_PACKAGE_ID, SOURCE_PACKAGE_ID);

            InitFileSystemWithMigrations(destAppDataPath, destCachePath, sourceAppDataPath, sourceCachePath);
        }

        const string SOURCE_PACKAGE_ID = "4651ED44255E.47979655102CE_k6txddmbb6c52";
        const string DEST_PACKAGE_ID = "31F8A90E.SteamforWindows";
    }
}