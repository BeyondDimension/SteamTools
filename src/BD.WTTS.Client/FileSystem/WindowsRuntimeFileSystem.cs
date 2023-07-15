#if WINDOWS
using Windows.ApplicationModel;
using Windows.Storage;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// <list type="bullet">
/// <item>AppData: %USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalState</item>
/// <item>Cache: %USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalCache</item>
/// <item>Logs: %USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalCache\Logs</item>
/// </list>
/// </summary>
sealed class WindowsRuntimeFileSystem : IOPath.FileSystemBase
{
    private WindowsRuntimeFileSystem() => throw new NotSupportedException();

    /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
    public static void InitFileSystem()
    {
        // https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/FileSystem/FileSystem.uwp.cs#L12
        // https://docs.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-behind-the-scenes
        // 不允许在 C:\Program Files\WindowsApps\package_name 下写入。

        var destAppDataPath = ApplicationData.Current.LocalFolder.Path;
        var destCachePath = ApplicationData.Current.LocalCacheFolder.Path;

        // %HOMEPATH%\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalState
        // %HOMEPATH%\AppData\Local\Packages\4651ED44255E.47979655102CE_gvn1zxq6vcwjj\LocalCache

        var packageId = Package.Current.Id.FamilyName;
        //switch (packageId)
        //{
        //    //case DEST_PACKAGE_ID:
        //    //    var sourceAppDataPath = ApplicationData.Current.LocalFolder.Path.Replace(DEST_PACKAGE_ID, SOURCE_PACKAGE_ID);
        //    //    var sourceCachePath = ApplicationData.Current.LocalCacheFolder.Path.Replace(DEST_PACKAGE_ID, SOURCE_PACKAGE_ID);

        //    //    InitFileSystemWithMigrations(destAppDataPath, destCachePath, sourceAppDataPath, sourceCachePath);
        //    //    break;
        //    //case SOURCE_PACKAGE_ID:
        //    default:
        InitFileSystem(() => destAppDataPath, () => destCachePath);
        //        break;
        //}
    }
}
#endif