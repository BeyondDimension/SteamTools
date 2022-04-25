using System.Application.Models;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using static System.Application.Utils;
using static System.ProjectPathUtil;

namespace System.Application.Steps
{
    internal static class Step5
    {
        /// <summary>
        /// 是否计算每个文件的哈希值，用于增量更新
        /// </summary>
        static readonly bool calcHash = false;

        static PublishDirInfo[]? Handler(DeploymentMode d, string[] val, bool dev)
        {
            if (!val.Any_Nullable())
            {
                Console.WriteLine(InputPubDirNameError);
                return null;
            }

            var pubPath = d switch
            {
                DeploymentMode.SCD => DirPublish_,
                DeploymentMode.FDE => DirPublish_FDE_,
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null),
            };

            var dirBasePath = projPath + string.Format(pubPath, GetConfiguration(dev, isLower: false));
            var dirNames = val.Select(x => new PublishDirInfo(x, Path.Combine(dirBasePath, x), d)).Where(x => Directory.Exists(x.Path)).ToArray();

            var publish_json_path = PublishJsonFilePath;
            IOPath.FileIfExistsItDelete(publish_json_path);

            foreach (var item in dirNames)
            {
                var isMacOS = item.Name.StartsWith("osx");
                // ASP.NET Core Runtime 6.0.0 缺少 macOS Installers
                if (isMacOS && d == DeploymentMode.FDE) continue;

                if (!Directory.Exists(item.Path))
                {
                    Console.WriteLine($"错误：找不到发布文件夹({item.Name})，{item.Path}");
                    return null;
                }

                List<Action> lazyActions = new();

                lazyActions.Add(() =>
                {
                    ScanPath(item.Path, item.Files, ignoreRootDirNames: ignoreDirNames);
                });

                //if (item.Name.StartsWith("win", StringComparison.OrdinalIgnoreCase))
                //{
                //    #region 适用于 Windwos 7 OS 的 疑难解答助手

                //    string win7Path;
                //    if (IsAigioPC)
                //    {
                //        win7Path = @"G:\Steam++.Win7";
                //    }
                //    else
                //    {
                //        win7Path = Path.Combine(item.Path, "..", "Steam++.Win7");
                //    }

                //    var win7ExePath = Path.Combine(win7Path, "Steam++.Win7.exe");
                //    if (!File.Exists(win7ExePath) || !File.Exists(win7ExePath + ".config"))
                //    {
                //        Console.WriteLine($"错误：Win7疑难助手程序不存在！{win7ExePath}");
                //        return;
                //    }

                //    lazyActions.Add(() =>
                //    {
                //        ScanPath(win7Path, item.Files);
                //    });

                //    #endregion

                //    var consoleExeFileName = Path.Combine(item.Path, "Steam++.Console.exe");
                //    if (!File.Exists(consoleExeFileName))
                //    {
                //        Console.WriteLine($"错误：Steam++.Console.exe 不存在！");
                //        return;
                //    }
                //}

                //if (item.Name.Equals("win-x86", StringComparison.OrdinalIgnoreCase))
                //{
                //    #region Win10SDK.NET

                //    const string netSDKFileName = "Microsoft.Windows.SDK.NET.dll";
                //    var netSDKFileRefPath = Path.Combine(projPath, "references", netSDKFileName);

                //    if (!File.Exists(netSDKFileRefPath))
                //    {
                //        Console.WriteLine($"错误：Microsoft.Windows.SDK.NET.dll不存在！{netSDKFileRefPath}");
                //        return;
                //    }

                //    var netSDKFileRefInfoVersion = GetInfoVersion(netSDKFileRefPath);
                //    var netSDKFilePath = Path.Combine(item.Path, netSDKFileName);
                //    var netSDKFileInfoVersion = GetInfoVersion(netSDKFilePath);
                //    if (netSDKFileRefInfoVersion != netSDKFileInfoVersion)
                //    {
                //        Console.WriteLine("错误：Microsoft.Windows.SDK.NET.dll 版本不正确！");
                //        Console.WriteLine($"Version: {netSDKFileInfoVersion}");
                //        Console.WriteLine($"Version(Ref): {netSDKFileRefInfoVersion}");
                //        return;
                //    }

                //    lazyActions.Add(() =>
                //    {
                //        foreach (var file in item.Files)
                //        {
                //            if (file.Path == netSDKFilePath)
                //            {
                //                file.Path = netSDKFileRefPath;
                //                file.Length = new FileInfo(netSDKFileRefPath).Length;
                //            }
                //        }
                //    });

                //    #endregion
                //}

                //string cefRootPath;
                //if (IsAigioPC)
                //{
                //    cefRootPath = @"G:\";
                //}
                //else
                //{
                //    cefRootPath = Path.Combine(item.Path, "..");
                //}

                //var cefPath = Path.Combine(cefRootPath, "CEF", item.Name);

                //if (!Directory.Exists(cefPath))
                //{
                //    Console.WriteLine($"错误：CEF库文件夹不存在！{cefPath}");
                //    return;
                //}
                //else if (!Directory.GetFiles(cefPath).Any())
                //{
                //    Console.WriteLine($"错误：CEF库文件不存在！{cefPath}");
                //    return;
                //}

                //lazyActions.Add(() =>
                //{
                //    ScanPath(cefPath, item.Files, cefRootPath);
                //});

                lazyActions.ForEach(x => x());

                Console.WriteLine($"{item.Name}, count: {item.Files.Count}");

                if (calcHash)
                {
                    foreach (var file in item.Files)
                    {
                        Console.WriteLine($"正在计算哈希值：{file.Path}");
                        using var fileStream = File.OpenRead(file.Path);
                        file.SHA256 = Hashs.String.SHA256(fileStream);
                    }
                }
            }

            return dirNames;
        }

        public static void Add(RootCommand command)
        {
            // sta -val "win-x86"
            var sta = new Command("start", "5. (本地)验证发布文件夹与计算文件哈希");
            sta.AddAlias("sta");
            sta.AddOption(new Option<string[]>("-val", InputPubDirNameDesc));
            sta.AddOption(new Option<bool>("-dev", DevDesc));
            sta.Handler = CommandHandler.Create((string[] val, bool dev) =>
            {
                if (!val.Any()) val = Step_cd.all_val;

                var dirNames = new[]
                {
                    Handler(DeploymentMode.SCD, val, dev),
                    Handler(DeploymentMode.FDE, val, dev),
                }.Where(x => x != null).SelectMany(x => x!).ToArray();

                if (dirNames.Any())
                {
                    SavePublishJson(dirNames);
                    Console.WriteLine("完成");
                }
            });
            command.AddCommand(sta);
        }
    }
}