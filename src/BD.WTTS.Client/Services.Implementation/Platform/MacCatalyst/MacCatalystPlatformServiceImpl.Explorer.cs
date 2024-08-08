#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
#if MACOS
    public static void OpenFile(string appName, string filePath)
    {
        NSWorkspace.SharedWorkspace.OpenFile(filePath, appName);
    }

    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenByDirectoryAsync
""")]
    public void OpenFolderByDirectoryPath(DirectoryInfo info)
    {
        //NSWorkspace.SharedWorkspace.SelectFile(string.Empty, info.FullName);
        NSWorkspace.SharedWorkspace.ActivateFileViewer(new[] {
            new NSUrl(info.FullName, isDir: true),
        });
    }

    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenSelectFileAsync
""")]
    public void OpenFolderSelectFilePath(FileInfo info)
    {
        NSWorkspace.SharedWorkspace.ActivateFileViewer(new[] {
            new NSUrl(info.FullName, isDir: false),
        });
    }
#endif
}
#endif