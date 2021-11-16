using System.Application.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Security.Cryptography;
using static System.Application.Utils;

namespace System.Application.Steps
{
    internal static class Step8_tar_gz
    {
        public static void Add(RootCommand command)
        {
            // tgz
            var tgz = new Command("tgz", "8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json");
            tgz.Handler = CommandHandler.Create(() =>
            {
                var publish_json_path = PublishJsonFilePath;
                var publish_json_str = File.ReadAllText(publish_json_path);
                var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

                if (!dirNames.Any_Nullable())
                {
                    Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                    return;
                }

                dirNames = dirNames.ThrowIsNull(nameof(dirNames));

                foreach (var item in dirNames)
                {
                    var packPath = GetPackPath(item, FileEx.TAR_GZ);
                    Console.WriteLine($"正在生成压缩包：{packPath}");
                    IOPath.FileIfExistsItDelete(packPath);

                    CreatePack(packPath, item.Files);

                    using var fileStream = File.OpenRead(packPath);
                    var sha256 = Hashs.String.SHA256(fileStream);

                    if (item.BuildDownloads.ContainsKey(AppDownloadType.Compressed_GZip))
                    {
                        item.BuildDownloads[AppDownloadType.Compressed_GZip] = new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length };
                    }
                    else
                    {
                        item.BuildDownloads.Add(AppDownloadType.Compressed_GZip, new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length });
                    }
                }

                SavePublishJson(dirNames, removeFiles: true);

                Console.WriteLine("完成");
            });
            command.AddCommand(tgz);
        }
    }

    internal static class Step8_7z
    {
        public static void Add(RootCommand command)
        {
            // 7z
            var _7z = new Command("7z", "8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json");
            _7z.Handler = CommandHandler.Create(() =>
            {
                var publish_json_path = PublishJsonFilePath;
                var publish_json_str = File.ReadAllText(publish_json_path);
                var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

                if (!dirNames.Any_Nullable())
                {
                    Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                    return;
                }

                dirNames = dirNames.ThrowIsNull(nameof(dirNames));

                foreach (var item in dirNames)
                {
                    var packPath = GetPackPath(item, FileEx._7Z);
                    Console.WriteLine($"正在生成压缩包：{packPath}");
                    IOPath.FileIfExistsItDelete(packPath);

                    CreateSevenZipPack(packPath, item.Files);

                    using var fileStream = File.OpenRead(packPath);
                    var sha256 = Hashs.String.SHA256(fileStream);

                    if (item.BuildDownloads.ContainsKey(AppDownloadType.Compressed_7z))
                    {
                        item.BuildDownloads[AppDownloadType.Compressed_7z] = new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length };
                    }
                    else
                    {
                        item.BuildDownloads.Add(AppDownloadType.Compressed_7z, new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length });
                    }
                }

                SavePublishJson(dirNames, removeFiles: true);

                Console.WriteLine("完成");
            });
            command.AddCommand(_7z);
        }
    }

    internal static class Step8_tar_br
    {
        public static void Add(RootCommand command)
        {
            // tbr
            var tar_br = new Command("tbr", "8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json");
            tar_br.Handler = CommandHandler.Create(() =>
            {
                var publish_json_path = PublishJsonFilePath;
                var publish_json_str = File.ReadAllText(publish_json_path);
                var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

                if (!dirNames.Any_Nullable())
                {
                    Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                    return;
                }

                dirNames = dirNames.ThrowIsNull(nameof(dirNames));

                foreach (var item in dirNames)
                {
                    var packPath = GetPackPath(item, FileEx.TAR_BR);
                    Console.WriteLine($"正在生成压缩包：{packPath}");
                    IOPath.FileIfExistsItDelete(packPath);

                    CreateBrotliPack(packPath, item.Files);

                    using var fileStream = File.OpenRead(packPath);
                    var sha256 = Hashs.String.SHA256(fileStream);

                    if (item.BuildDownloads.ContainsKey(AppDownloadType.Compressed_Br))
                    {
                        item.BuildDownloads[AppDownloadType.Compressed_Br] = new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length };
                    }
                    else
                    {
                        item.BuildDownloads.Add(AppDownloadType.Compressed_Br, new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length });
                    }
                }

                SavePublishJson(dirNames, removeFiles: true);

                Console.WriteLine("完成");
            });
            command.AddCommand(tar_br);
        }
    }
}