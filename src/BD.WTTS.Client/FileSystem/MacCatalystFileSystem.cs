#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// <list type="bullet">
/// <item>AppData: ~/Library/Steam++</item>
/// <item>Cache: ~/Library/Caches/Steam++</item>
/// <item>Logs: ~/Library/Caches/Steam++/Logs</item>
/// </list>
/// </summary>
sealed class MacCatalystFileSystem : IOPath.FileSystemBase
{
    private MacCatalystFileSystem() => throw new NotSupportedException();

    /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
    public static void InitFileSystem()
    {
        // https://github.com/xamarin/Essentials/blob/main/Xamarin.Essentials/FileSystem/FileSystem.ios.tvos.watchos.macos.cs
        var appDataPath = Path.Combine(GetDirectory(NSSearchPathDirectory.LibraryDirectory), Constants.HARDCODED_APP_NAME);
        var cachePath = Path.Combine(GetDirectory(NSSearchPathDirectory.CachesDirectory), Constants.HARDCODED_APP_NAME);
        IOPath.DirCreateByNotExists(appDataPath);
        IOPath.DirCreateByNotExists(cachePath);
        InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
        string GetAppDataDirectory() => appDataPath;
        string GetCacheDirectory() => cachePath;
    }

    static string GetDirectory(NSSearchPathDirectory directory)
    {
        var dirs = NSSearchPath.GetDirectories(directory, NSSearchPathDomain.User);
        if (dirs == null || dirs.Length == 0)
        {
            // this should never happen...
            throw new NotSupportedException("this should never happen...");
            //return null;
        }
        return dirs[0];
    }
}
#endif