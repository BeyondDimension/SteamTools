#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenByDirectoryAsync
""")]
    public void OpenFolderByDirectoryPath(DirectoryInfo info)
        => StartProcessExplorer($"\"{info.FullName}\"");

    [Mobius(
"""
Mobius.Helpers.FolderHelper.OpenSelectFileAsync
""")]
    public void OpenFolderSelectFilePath(FileInfo info)
        => StartProcessExplorer($"/select,\"{info.FullName}\"");
}
#endif