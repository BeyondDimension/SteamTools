using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using static System.ProjectPathUtil;
using static System.Application.Utils;

namespace System.Application.Steps
{
    internal static class StepAppHostPatcher
    {
        const string DirName = "Bin";
        static readonly string[] ignoreDirNames = new[] { "Assets", IOPath.DirName_AppData, IOPath.DirName_Cache, "Logs", "Bin" };

        public static void Handler(bool dev, DeploymentMode d, bool endWriteOK = true)
        {
            var configuration = GetConfiguration(dev, isLower: false);
            var pubPath = d switch
            {
                DeploymentMode.SCD => projPath + string.Format(DirPublishWinX64_, configuration),
                DeploymentMode.FDE => projPath + string.Format(DirPublishWinX64_FDE_, configuration),
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null),
            };

            var exePaths = new[] {
                    pubPath + Path.DirectorySeparatorChar + "Steam++.exe",
                    //pubPath + Path.DirectorySeparatorChar + "Steam++.Console.exe"
                };

            foreach (var exePath in exePaths)
            {
                if (!File.Exists(exePath))
                {
                    Console.WriteLine($"错误：找不到 exe 文件，路径：{exePath}");
                    return;
                }
            }

            foreach (var exePath in exePaths)
            {
                // apphostpatcher "C:\Code\2021\SteamTools\src\ST.Client.Desktop.Avalonia.App\bin\Release\Publish\win-x64\Steam++.exe" -d Bin
                _ = AppHostPatcher.Program.M(new string[] { exePath, "-d", DirName });
            }

            static string[] GetFiles(string dirPath)
            {
                return Directory.GetFiles(dirPath)
                .Where(x => (!string.Equals(Path.GetExtension(x), ".exe", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetFileName(x), "createdump.exe", StringComparison.OrdinalIgnoreCase))
                && !x.EndsWith(".VisualElementsManifest.xml", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            }
            static string[] GetDirs(string dirPath)
            {
                return Directory.GetDirectories(dirPath)
                    .Where(x => !ignoreDirNames.Contains(new DirectoryInfo(x).Name, StringComparer.OrdinalIgnoreCase)).ToArray();
            }
            static List<string> GetAllFiles(string dirPath, List<string>? list = null)
            {
                list ??= new List<string>();
                var files = GetFiles(dirPath);
                list.AddRange(files);
                var dirs = GetDirs(dirPath);
                foreach (var dir in dirs)
                {
                    _ = GetAllFiles(dir, list);
                }
                return list;
            }

            var allFiles = GetAllFiles(pubPath);
            foreach (var file in allFiles)
            {
                var rPath = Path.GetRelativePath(pubPath, file);
                rPath = Path.Combine(DirName, rPath);
                var newPath = pubPath + Path.DirectorySeparatorChar + rPath;
                Console.WriteLine($"正在移动文件：{file} -> {newPath}");
                var rootDirPath = Path.GetDirectoryName(newPath);
                if (!Directory.Exists(rootDirPath)) Directory.CreateDirectory(rootDirPath);
                File.Move(file, newPath);
            }

            void ClearEmptyDir()
            {
                var dirs = Directory.GetDirectories(pubPath);
                foreach (var dir in dirs)
                {
                    if (Directory.GetDirectories(dir).Length == 0 && Directory.GetFiles(dir).Length == 0)
                    {
                        Directory.Delete(dir);
                    }
                }
            }
            ClearEmptyDir();

            if (endWriteOK) Console.WriteLine("OK");
        }

        public static void Add(RootCommand command)
        {
            var hp = new Command("hostpath", "X. (本地)将发布 Host 入口点重定向到 Bin 目录中");
            hp.AddAlias("hp");
            hp.AddOption(new Option<bool>("-dev", DevDesc));
            hp.Handler = CommandHandler.Create((bool dev) =>
            {
                Handler(dev, DeploymentMode.SCD);
                Handler(dev, DeploymentMode.FDE);
            });
            command.AddCommand(hp);
        }
    }
}