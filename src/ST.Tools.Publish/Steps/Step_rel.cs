using System;
using System.Application.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    internal static class StepRel
    {
        public static void Add(RootCommand command)
        {
            var rel = new Command("rel", "12. (本地)读取 **Publish.json** 中的 SHA256 值写入 release-template.md")
            {
                Handler = CommandHandler.Create(() => Handler2()),
            };
            command.AddCommand(rel);
        }

        //static void Handler()
        //{
        //    var publish_json_path = PublishJsonFilePath;
        //    var publish_json_str = File.ReadAllText(publish_json_path);
        //    var dirNames = Serializable.DJSON<PublishDirInfo[]>(publish_json_str);

        //    if (!dirNames.Any_Nullable())
        //    {
        //        Console.WriteLine($"错误：发布配置文件读取失败！{publish_json_path}");
        //        return;
        //    }

        //    dirNames = dirNames.ThrowIsNull(nameof(dirNames));


        //    var release_template_md_path = projPath + release_template_md;
        //    var lines = File.ReadAllLines(release_template_md_path);
        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        var line = lines[i];
        //        var rows = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
        //        if (rows.Length >= 2)
        //        {
        //            var left = rows[0].Trim();
        //            if (left.StartsWith("Steam++"))
        //            {
        //                var is7z = left.EndsWith(FileEx._7Z);
        //                var isApk = left.EndsWith(FileEx.APK);
        //                if (is7z || isApk)
        //                {
        //                    foreach (var item in dirNames)
        //                    {
        //                        var isAndroid = item.Name.StartsWith("android");
        //                        if (isAndroid && !isApk) continue;

        //                        AppDownloadType? appDownloadType = null;
        //                        if (isAndroid && item.BuildDownloads.ContainsKey(AppDownloadType.Install))
        //                        {
        //                            appDownloadType = AppDownloadType.Install;
        //                        }
        //                        else if (item.BuildDownloads.ContainsKey(AppDownloadType.Compressed_7z))
        //                        {
        //                            appDownloadType = AppDownloadType.Compressed_7z;
        //                        }

        //                        if (appDownloadType.HasValue)
        //                        {
        //                            var name = item.Name.Replace('-', '_');
        //                            if (left.Contains(name))
        //                            {
        //                                var sha256 = item.BuildDownloads[appDownloadType.Value].SHA256;
        //                                lines[i] = $"| {name} | {sha256} |";
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    File.WriteAllLines(release_template_md_path, lines);

        //    Console.WriteLine("完成");
        //}

        public static void Handler2(bool endWriteOK = true)
        {
            var pubDirPath = projPath + string.Format(DirPublish_, "Release");

            var release_template_md_path = projPath + release_template_md;
            var lines = File.ReadAllLines(release_template_md_path);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var rows = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (rows.Length >= 2)
                {
                    var left = rows[0].Trim();
                    var right = rows[1].Trim();
                    if (left.StartsWith("Steam++") && right == "SHA256")
                    {
                        var filePath = Path.Combine(pubDirPath, left);
                        if (File.Exists(filePath))
                        {
                            using var fs = File.OpenRead(filePath);
                            var hash = Hashs.String.SHA256(fs);
                            lines[i] = $"| {left}  | {hash} |";
                        }
                    }
                }
            }

            File.WriteAllLines(release_template_md_path, lines);

            if (endWriteOK) Console.WriteLine("完成");
        }
    }
}