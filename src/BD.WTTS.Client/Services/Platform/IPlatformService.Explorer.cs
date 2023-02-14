// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 使用资源管理器打开某个路径
    /// </summary>
    /// <param name="path"></param>
    void OpenFolder(string path)
    {
        sbyte isDirectory = -1;
        FileInfo? fileInfo;
        DirectoryInfo directoryInfo = new(path);
        if (directoryInfo.Exists) // 路径为文件夹，则打开文件夹
        {
            fileInfo = null;
            isDirectory = 1;
        }
        else
        {
            fileInfo = new(path);
            if (fileInfo.Exists) // 路径为文件，则打开文件
            {
                isDirectory = 0;
            }
            else if (fileInfo.DirectoryName != null)
            {
                directoryInfo = new(fileInfo.DirectoryName);
                fileInfo = null;
                if (directoryInfo.Exists) // 路径为文件但文件不存在，文件夹存在，则打开文件夹
                {
                    fileInfo = null;
                    isDirectory = 1;
                }
            }
        }

        switch (isDirectory)
        {
            case 1:
                if (directoryInfo != null) OpenFolderByDirectoryPath(directoryInfo);
                break;
            case 0:
                if (fileInfo != null) OpenFolderSelectFilePath(fileInfo);
                break;
        }
    }

    /// <summary>
    /// 使用资源管理器打开文件夹路径
    /// </summary>
    /// <param name="info"></param>
    void OpenFolderByDirectoryPath(DirectoryInfo info)
    {

    }

    /// <summary>
    /// 使用资源管理器选中文件路径
    /// </summary>
    /// <param name="info"></param>
    void OpenFolderSelectFilePath(FileInfo info)
    {

    }
}