namespace BD.WTTS.Enums;

/// <summary>
/// 指定用于检索应用程序所使用的文件夹目录路径的枚举常数
/// </summary>
public enum AppFolder : byte
{
    /// <summary>
    /// 图片
    /// </summary>
    Images,

    /// <summary>
    /// 数据库
    /// </summary>
    Database,

    /// <summary>
    /// 缓存、临时文件夹
    /// </summary>
    Cache,
}

/// <summary>
/// Enum 扩展 <see cref="AppFolder"/>
/// </summary>
public static class AppFolderEnumExtensions
{
    /// <summary>
    /// 获取文件夹路径，返回的路径必定存在
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static string GetPath(this AppFolder folder)
    {
        var path1 = folder switch
        {
            AppFolder.Images or AppFolder.Cache => IOPath.CacheDirectory,
            AppFolder.Database => IOPath.AppDataDirectory,
            _ => throw new ArgumentOutOfRangeException(nameof(folder), folder, null),
        };
        var path = Path.Combine(path1, folder.ToString());
        IOPath.DirCreateByNotExists(path);
        return path;
    }
}