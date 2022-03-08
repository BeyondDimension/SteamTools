using System.IO;
using System.Text;
using System.Application.Properties;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;
using ArchiSteamFarm;
using System.Collections.Generic;

namespace System.Application.Services.Implementation
{
    partial class ArchiSteamFarmServiceImpl
    {
        const string _Version_ASFUI = "5.2.3.5";
        const string Version_FileName = "VERSION.txt";

        static string Version_ASFUI =>
            //_Version_ASFUI == "5.1.5.3" ?
            //(Path.DirectorySeparatorChar != WinDirectorySeparatorChar ?
            //    _Version_ASFUI + "-unix_unpack_fix" :
            //    _Version_ASFUI) :
            _Version_ASFUI;

        /// <summary>
        /// 尝试解压 ASF-UI 资源包
        /// </summary>
        static void TryUnPackASFUI()
        {
            var dirPath = IArchiSteamFarmService.GetFolderPath(ASFPathFolder.WWW);
            var versionFilePath = Path.Combine(dirPath, Version_FileName);
            if (File.Exists(versionFilePath))
            {
                var version = File.ReadAllText(versionFilePath);
                if (version != Version_ASFUI) // 版本号文件不匹配，重新解压
                {
                    UnPackASFUI(true);
                }
            }
            else
            {
                UnPackASFUI(false); // 版本号文件不存在，重新解压
            }

            void UnPackASFUI(bool dirPathExists)
            {
                if (dirPathExists || Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }

                using var stream = new MemoryStream(SR.asf_ui);
                using var decompress = new BrotliStream(stream, CompressionMode.Decompress);
                using var archive = TarArchive.CreateInputTarArchive(decompress, EncodingCache.UTF8NoBOM);
                archive.ExtractContents(dirPath);

                //if (Path.DirectorySeparatorChar != WinDirectorySeparatorChar) // 修正解压文件名中带有文件夹分隔符反斜杠的问题
                //{
                //    var files = Directory.GetFiles(dirPath);
                //    List<string> existsDirPaths = new();
                //    foreach (var item in files)
                //    {
                //        var fileName = Path.GetFileName(item);
                //        var fileRelativePaths = fileName.Split(WinDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                //        if (fileRelativePaths.Length < 2) continue;
                //        var paths = new string[fileRelativePaths.Length];
                //        paths[0] = dirPath;
                //        for (int i = 1; i < paths.Length; i++)
                //        {
                //            paths[i] = fileRelativePaths[i - 1];
                //        }
                //        var destDirPath = Path.Combine(paths);
                //        if (!existsDirPaths.Contains(destDirPath) && !Directory.Exists(destDirPath))
                //        {
                //            Directory.CreateDirectory(destDirPath);
                //            existsDirPaths.Add(destDirPath);
                //        }
                //        var destFilePath = Path.Combine(destDirPath, fileRelativePaths[^1]);
                //        File.Move(item, destFilePath);
                //    }
                //}

                File.WriteAllText(versionFilePath, Version_ASFUI);
            }
        }
    }
}
