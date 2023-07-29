using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip.Compression;

// TODO: Use .NET 7 Tar API

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static class TarGZipHelper
{
    /// <summary>
    /// 创建压缩包文件
    /// </summary>
    /// <param name="filePath">要创建的文件路径</param>
    /// <param name="dirPath">要打包的目录</param>
    /// <param name="level">压缩等级，取值范围在 <see cref="Deflater.NO_COMPRESSION"/> ~ <see cref="Deflater.BEST_COMPRESSION"/></param>
    /// <param name="progress">进度值监听</param>
    /// <returns></returns>
    public static bool Create(string filePath, string dirPath, int level = Deflater.BEST_COMPRESSION, ProgressMessageHandler? progress = null)
    {
        if (!filePath.EndsWith(FileEx.TAR_GZ)) filePath += FileEx.TAR_GZ;
        if (File.Exists(filePath)) return false;
        if (!Directory.Exists(dirPath)) return false;

        using var fs = File.Create(filePath);
        using var s = new GZipOutputStream(fs);
        s.SetLevel(level);
        using var archive = TarArchive.CreateOutputTarArchive(s, TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
        if (progress != null) archive.ProgressMessageEvent += progress;

        HandleFiles(dirPath);
        HandleDirs(dirPath);

        void HandleFiles(string dirPath_)
        {
            foreach (var file in Directory.GetFiles(dirPath_))
            {
                var entry = TarEntry.CreateEntryFromFile(file);
                entry.Name = file.TrimStart(dirPath).Trim(Path.DirectorySeparatorChar);
                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                archive.WriteEntry(entry, false);
            }
        }

        void HandleDirs(string dirPath_)
        {
            foreach (var dir in Directory.GetDirectories(dirPath_))
            {
                HandleFiles(dir);
            }
        }

        return true;
    }

    /// <summary>
    /// 解压压缩包文件
    /// </summary>
    /// <param name="filePath">要解压的压缩包文件路径</param>
    /// <param name="dirPath">要解压的文件夹路径，文件夹必须不存在</param>
    /// <param name="progressMessage">进度消息监听</param>
    /// <param name="progress">进度值监听</param>
    /// <param name="maxProgress"></param>
    /// <returns></returns>
    public static bool Unpack(string filePath, string dirPath,
        ProgressMessageHandler? progressMessage = null,
        IProgress<float>? progress = null,
        float maxProgress = 100f)
    {
        if (!File.Exists(filePath)) return false;
        if (Directory.Exists(dirPath)) return false;

        using var fs = File.OpenRead(filePath);
        // 只能用 FileStream 的长度，GZipInputStream.Length 会引发异常
        // https://github.com/icsharpcode/SharpZipLib/blob/v1.3.1/src/ICSharpCode.SharpZipLib/Zip/Compression/Streams/InflaterInputStream.cs#L542
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
        using var s = new GZipInputStream(fs);
        using var archive = TarArchive.CreateInputTarArchive(s, EncodingCache.UTF8NoBOM);
        if (progressMessage != null) archive.ProgressMessageEvent += progressMessage;
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
        archive.ExtractContents(dirPath);
        isFinish = true;
        if (hasProgress)
        {
            cts!.Cancel();
            progress!.Report(maxProgress);
        }
        return true;
    }
}