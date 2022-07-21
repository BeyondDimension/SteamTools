using System.Application.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using static System.Application.Utils;
using System.Threading.Tasks;
using System.Linq;
using System.Application.Steps;

namespace System.Application.Steps
{
    internal static class Step8_tar_gz // tgz
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_GZip,
                "tgz");
    }

    internal static class Step8_7z // 7z(Lzma2) 64,809,214 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_7z,
                "7z");
    }

    internal static class Step8_tar_br // tbr 70,278,650 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_Br,
                "tbr");
    }

    internal static class Step8_tar_xz // tar.xz XZOutputStream.Flush NotSupportedException
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_XZ,
                "xz");
    }

    internal static class Step8_tar_zst // tar.zst 71,854,499 字节
    {
        public static void Add(RootCommand command)
            => Step8.Add(command,
                AppDownloadType.Compressed_Zstd,
                "zst");
    }
}

namespace System.Application
{
    partial class Utils
    {
        public static class Step8
        {
            public static void Add(RootCommand command, AppDownloadType type, string name)
            {
                var comm = new Command(name, "8. (本地)读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json")
                {
                    Handler = CommandHandler.Create(async (bool dev, string buildpackage) =>
                    {
                        if (!string.IsNullOrEmpty(buildpackage))
                        {
                            buildpackage = buildpackage.Trim();
                            buildpackage = buildpackage.Length > 7 ? buildpackage[..7] : buildpackage;
                            Version = buildpackage;
                        }

                        var publish_json_path = PublishJsonFilePath;
                        var publish_json_str = File.ReadAllText(publish_json_path);
                        var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

                        if (!dirNames.Any_Nullable())
                        {
                            Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
                            return;
                        }

                        dirNames = dirNames.ThrowIsNull(nameof(dirNames));

                        var parallelTasks = dirNames.Select(x => Task.Run(() => Step_cd.GenerateCompressedPackage(dev, x, type))).ToArray();

                        foreach (var item in parallelTasks)
                        {
                            await item;
                        }
                        //var processorCount = Environment.ProcessorCount;
                        //if (processorCount < 2) processorCount = 2;
                        //var parallelCount = processorCount;

                        //if (parallelTasks.Length > parallelCount)
                        //{
                        //    var count = 0;
                        //    while (true)
                        //    {
                        //        var parallelTasksSplit = parallelTasks.Skip(count++ * parallelCount).Take(parallelCount).ToArray();
                        //        if (!parallelTasksSplit.Any()) break;
                        //        await Task.WhenAll(parallelTasksSplit);
                        //    }
                        //}
                        //else
                        //{
                        //    await Task.WhenAll(parallelTasks);
                        //}

                        SavePublishJson(dirNames, removeFiles: false);

                        Console.WriteLine("完成");
                    })
                };
                comm.AddOption(new Option<bool>("-dev", DevDesc));
                comm.AddOption(new Option<string>("-buildpackage", "指定生成包版本号"));
                command.AddCommand(comm);
            }
        }
    }
}

// Steam++_linux_x64_v2.6.1.7z 56,743,466 字节
// Steam++_linux_x64_v2.6.1.tar.br 61,530,820 字节
// Steam++_linux_x64_v2.6.1.tar.xz 57,970,140 字节
// Steam++_linux_x64_v2.6.1.tar.zst 62,902,361 字节 (Zstandard v1.5.0 level 22)