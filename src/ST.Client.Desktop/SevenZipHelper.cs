using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CC = System.Common.Constants;

namespace System.Application
{
    public static class SevenZipHelper
    {
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
            var isFinish = false;
            CancellationTokenSource? cts = null;
            async void ProgressMonitor()
            {
                try
                {
                    while (!isFinish)
                    {
                        var value = fs.Position / length * maxProgress;
                        progress!.Report(value);
                        await Task.Delay(200, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }
            Directory.CreateDirectory(dirPath);
            using var archive = SevenZipArchive.Open(fs);
            var hasProgress = progress != null;
            if (hasProgress)
            {
                cts = new();
                try
                {
                    Task.Factory.StartNew(ProgressMonitor, cts.Token);
                }
                catch (OperationCanceledException)
                {
                }
            }
            var files = archive.Entries.Where(entry => !entry.IsDirectory).ToArray();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                file.WriteToDirectory(dirPath, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true
                });
            }
            isFinish = true;
            if (hasProgress)
            {
                cts!.Cancel();
                progress!.Report(maxProgress);
            }
            return true;
        }
    }
}