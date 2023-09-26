#if ANDROID
using MauiStorageFS = Microsoft.Maui.Storage.FileSystem;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

sealed class EssentialsFileSystem : IOPath.FileSystemBase
{
    private EssentialsFileSystem() => throw new NotSupportedException();

    /// <inheritdoc cref="FileSystem2.InitFileSystem"/>
    public static void InitFileSystem()
    {
        InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
        string GetAppDataDirectory() => MauiStorageFS.AppDataDirectory;
        string GetCacheDirectory() => MauiStorageFS.CacheDirectory;
    }
}
#endif