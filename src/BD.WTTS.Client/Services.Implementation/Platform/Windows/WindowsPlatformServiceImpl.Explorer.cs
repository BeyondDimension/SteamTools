#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public void OpenFolderByDirectoryPath(DirectoryInfo info)
        => StartProcessExplorer($"\"{info.FullName}\"");

    public void OpenFolderSelectFilePath(FileInfo info)
        => StartProcessExplorer($"/select,\"{info.FullName}\"");
}
#endif