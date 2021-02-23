using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO;
using System.Text;

namespace System.Application
{
    public static class TarGZipHelper
    {
        /// <summary>
        /// 创建压缩包文件
        /// </summary>
        /// <param name="filePath">要创建的文件路径</param>
        /// <param name="dirPath">要打包的目录</param>
        /// <param name="level">压缩等级，取值范围在 <see cref="Deflater.NO_COMPRESSION"/> ~ <see cref="Deflater.BEST_COMPRESSION"/></param>
        /// <returns></returns>
        public static bool Create(string filePath, string dirPath, int level = Deflater.BEST_COMPRESSION, ProgressMessageHandler? progress = null)
        {
            if (!filePath.EndsWith(FileEx.TAR_GZ)) filePath += FileEx.TAR_GZ;
            if (File.Exists(filePath)) return false;
            if (!Directory.Exists(dirPath)) return false;

            using var fs = File.Create(filePath);
            using var s = new GZipOutputStream(fs);
            s.SetLevel(level);
            using var archive = TarArchive.CreateOutputTarArchive(s, TarBuffer.DefaultBlockFactor, Encoding.UTF8);
            if (progress != null) archive.ProgressMessageEvent += progress;

            HandleFiles(dirPath);
            HandleDirs(dirPath);

            void HandleFiles(string dirPath_)
            {
                foreach (var file in Directory.GetFiles(dirPath_))
                {
                    var entry = TarEntry.CreateEntryFromFile(file);
                    entry.Name = file.TrimStart(dirPath).Trim(Path.DirectorySeparatorChar);
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
        /// <param name="filePath"></param>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool Unpack(string filePath, string dirPath, ProgressMessageHandler? progress = null)
        {
            if (!File.Exists(filePath)) return false;
            if (Directory.Exists(dirPath)) return false;

            using var fs = File.OpenRead(filePath);
            Directory.CreateDirectory(dirPath);
            using var s = new GZipInputStream(fs);
            using var archive = TarArchive.CreateInputTarArchive(s, Encoding.UTF8);
            if (progress != null) archive.ProgressMessageEvent += progress;
            archive.ExtractContents(dirPath);
            return true;
        }
    }

#if DEBUG

    [Obsolete("use TarGZipHelper", true)]
    public static class ZipHelper
    {
        [Obsolete("use TarGZipHelper.Create", true)]
        public static void PackFiles(string filename, string directory)
        {
        }

        [Obsolete("use TarGZipHelper.Unpack", true)]
        public static bool UnpackFiles(string file, string dir)
        {
            return default;
        }
    }

    [Obsolete("use TarGZipHelper", true)]
    public static class ClassZip
    {
        [Obsolete("use TarGZipHelper.Create", true)]
        public static bool Zip(string FileToZip, string ZipedFile, int level)
        {
            return default;
        }

        [Obsolete("use TarGZipHelper.Unpack", true)]
        public static void UnZip(string FileToUpZip, string ZipedFolder)
        {
        }
    }

#endif
}