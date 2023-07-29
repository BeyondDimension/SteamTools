// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface ISevenZipHelper
{
    static ISevenZipHelper Instance => Ioc.Get_Nullable<ISevenZipHelper>() ?? throw new PlatformNotSupportedException();

    /// <summary>
    /// 解压压缩包文件
    /// </summary>
    /// <param name="filePath">要解压的压缩包文件路径</param>
    /// <param name="dirPath">要解压的文件夹路径，文件夹必须不存在</param>
    /// <param name="progress">进度值监听</param>
    /// <param name="maxProgress">最大进度值，100%</param>
    /// <returns></returns>
    bool Unpack(string filePath, string dirPath,
        IProgress<float>? progress = null,
        float maxProgress = 100f);
}