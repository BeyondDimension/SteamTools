using SevenZip;
using System.IO;
using CC = System.Common.Constants;

namespace System.Application
{
    public static class SevenZipHelper
    {
        static SevenZipHelper()
        {
            if (OperatingSystem2.IsWindows)
            {
                // ✅ AppContext.BaseDirectory
                var sevenZipLibraryPath = Path.Combine(AppContext.BaseDirectory, "7z.dll");
                SevenZipBase.SetLibraryPath(sevenZipLibraryPath);
            }
        }

        /// <summary>
        /// 解压压缩包文件
        /// </summary>
        /// <param name="filePath">要解压的压缩包文件路径</param>
        /// <param name="dirPath">要解压的文件夹路径，文件夹必须不存在</param>
        /// <param name="progress">进度值监听</param>
        /// <param name="maxProgress"></param>
        /// <returns></returns>
        public static bool Unpack(string filePath, string dirPath,
            IProgress<float>? progress = null,
            float maxProgress = CC.MaxProgress)
        {
            if (!File.Exists(filePath)) return false;
            if (Directory.Exists(dirPath)) return false;

            using var fs = File.OpenRead(filePath);
            float length = fs.Length;
            Directory.CreateDirectory(dirPath);
            using var archive = new SevenZipExtractor(fs);
            var hasProgress = progress != null;
            if (hasProgress)
            {
                archive.Extracting += (_, e) =>
                {
                    progress!.Report(e.PercentDone);
                };
            }
            archive.ExtractArchive(dirPath);
            if (hasProgress)
            {
                progress!.Report(maxProgress);
            }
            return true;
        }
    }
}