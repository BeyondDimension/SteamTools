using System.Application.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using static System.Application.Utils;

namespace System.Application.Steps
{
    internal static class Step8_tar_gz // tgz
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_GZip,
                "tgz",
                FileEx.TAR_GZ,
                CreatePack);
    }

    internal static class Step8_7z // 7z(Lzma2) 64,809,214 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_7z,
                "7z",
                FileEx._7Z,
                CreateSevenZipPack);
    }

    internal static class Step8_tar_br // tbr 70,278,650 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_Br,
                "tbr",
                FileEx.TAR_BR_LONG,
                CreateBrotliPack);
    }

    internal static class Step8_tar_xz // tar.xz XZOutputStream.Flush NotSupportedException
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_XZ,
                "xz",
                FileEx.TAR_XZ,
                CreateXZPack);
    }

    internal static class Step8_tar_zst // tar.zst 71,854,499 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_Zstd,
                "zst",
                FileEx.TAR_ZST,
                CreateZstdPack);
    }
}

namespace System.Application
{
    partial class Utils
    {
        public static class Step8
        {
            public static void Add(RootCommand command, AppDownloadType type, string name, string fileEx, Action<string, IEnumerable<PublishFileInfo>> createPack)
            {
                var comm = new Command(name, "8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json")
                {
                    Handler = CommandHandler.Create(() =>
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
                            var packPath = GetPackPath(item, fileEx);
                            Console.WriteLine($"正在生成压缩包：{packPath}");
                            IOPath.FileIfExistsItDelete(packPath);

                            createPack(packPath, item.Files);

                            using var fileStream = File.OpenRead(packPath);
                            var sha256 = Hashs.String.SHA256(fileStream);

                            if (item.BuildDownloads.ContainsKey(type))
                            {
                                item.BuildDownloads[type] = new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length };
                            }
                            else
                            {
                                item.BuildDownloads.Add(type, new PublishFileInfo { SHA256 = sha256, Length = fileStream.Length });
                            }
                        }

                        SavePublishJson(dirNames, removeFiles: false);

                        Console.WriteLine("完成");
                    })
                };
                command.AddCommand(comm);
            }
        }
    }
}

// Steam++_linux_x64_v2.6.1.7z 56,743,466 字节
// Steam++_linux_x64_v2.6.1.tar.br 61,530,820 字节
// Steam++_linux_x64_v2.6.1.tar.xz 57,970,140 字节
// Steam++_linux_x64_v2.6.1.tar.zst 62,902,361 字节 (Zstandard v1.5.0 level 22)