using System.IO;
using System.Text;
using System.Application.Properties;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;
using ArchiSteamFarm;

namespace System.Application.Services.Implementation
{
    partial class ArchiSteamFarmServiceImpl
    {
        const string Version_ASFUI = "5.1.5.3";
        const string Version_FileName = "VERSION.txt";

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
                using var archive = TarArchive.CreateInputTarArchive(decompress, Encoding.UTF8);
                archive.ExtractContents(dirPath);

                File.WriteAllText(versionFilePath, Version_ASFUI);
            }
        }
    }
}
