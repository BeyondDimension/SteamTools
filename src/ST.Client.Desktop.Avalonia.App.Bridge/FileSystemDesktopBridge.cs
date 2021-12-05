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
            // C:\Users\{user}\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalState
            // C:\Users\{user}\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalCache
            InitFileSystem(() => ApplicationData.Current.LocalFolder.Path, () => ApplicationData.Current.LocalCacheFolder.Path);
        }
    }
}