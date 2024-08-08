#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public const string xdg = "xdg-open";

    [Mobius(
"""
FolderHelper.XDGOpen
""")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void XDGOpen(string path) => Process2.StartPath(xdg, path);

    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenByDirectoryAsync
""")]
    public void OpenFolderByDirectoryPath(DirectoryInfo info) => XDGOpen(info.FullName);

    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenSelectFileAsync
""")]
    public void OpenFolderSelectFilePath(FileInfo info)
    {
        if (info.DirectoryName == null) return;
        XDGOpen(info.DirectoryName);
    }
}
#endif