using CC = System.Common.Constants;

namespace System.Application.Services
{
    public interface ISevenZipHelper
    {
        static ISevenZipHelper Instance => DI.Get_Nullable<ISevenZipHelper>() ?? throw new PlatformNotSupportedException();

        /// <summary>
        /// 解压压缩包文件
        /// </summary>
        /// <param name="filePath">要解压的压缩包文件路径</param>
        /// <param name="dirPath">要解压的文件夹路径，文件夹必须不存在</param>
        /// <param name="progress">进度值监听</param>
        /// <param name="maxProgress"></param>
        /// <returns></returns>
        bool Unpack(string filePath, string dirPath,
          IProgress<float>? progress = null,
          float maxProgress = CC.MaxProgress);
    }
}
