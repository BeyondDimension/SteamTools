#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public const string xdg = "xdg-open";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void XDGOpen(string path) => Process2.StartPath(xdg, path);

    public void OpenFolderByDirectoryPath(DirectoryInfo info) => XDGOpen(info.FullName);

    public void OpenFolderSelectFilePath(FileInfo info)
    {
        if (info.DirectoryName == null) return;
        XDGOpen(info.DirectoryName);
    }
}
#endif