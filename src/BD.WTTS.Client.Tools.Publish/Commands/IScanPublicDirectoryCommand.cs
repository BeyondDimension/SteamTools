namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// 扫描发布文件夹命令
/// </summary>
interface IScanPublicDirectoryCommand : ICommand
{
    const string commandName = "scan";

    static Command ICommand.GetCommand()
    {
        var sha256 = new Option<bool>("--sha256", () => true, "Calculate hash sha256");
        var sha384 = new Option<bool>("--sha384", () => true, "Calculate hash sha384");
        var signature = new Option<bool>("--signature", () => true, "Digital signature");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "Scan publish directory")
        {
            sha256, sha384, signature, rids,
        };
        command.SetHandler(Handler, sha256, sha384, signature, rids);
        return command;
    }

    /// <summary>
    /// 根据参数获取所有 RID
    /// </summary>
    /// <param name="rids"></param>
    /// <returns></returns>
    static string[] GetRuntimeIdentifiers(string[] rids)
    {
        if (rids == null || !rids.Any())
        {
            rids = all_rids;
        }
        return rids;
    }

    /// <summary>
    /// 根据 RID 创建应用程序发布信息
    /// </summary>
    /// <param name="rid"></param>
    /// <param name="deploymentMode"></param>
    /// <returns></returns>
    static AppPublishInfo? GetAppPublishInfo(string rid, DeploymentMode deploymentMode)
    {
        var info = new AppPublishInfo()
        {
            DeploymentMode = deploymentMode,
            RuntimeIdentifier = rid,
        };
        (info.Platform, info.DeviceIdiom, info.Architecture) = DeconstructRuntimeIdentifier(rid);
        return info;
    }

    /// <summary>
    /// 是否支持框架依赖部署模式
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    static bool HasFrameworkDependent(AppPublishInfo info)
    {
        if (info.Platform == Platform.Windows)
        {
            return true;
        }
        return default;
    }

    internal static void Handler(bool sha256, bool sha384, bool signature, string[] rids)
    {
        ConcurrentBag<AppPublishInfo> infos = new();
        rids = GetRuntimeIdentifiers(rids);
        var tasks = rids.Select(rid => InBackground(() =>
        {
            DeploymentMode deploymentMode = DeploymentMode.SCD;
            if (TryAddInfo(rid, deploymentMode, out var info))
            {
                if (HasFrameworkDependent(info))
                {
                    TryAddInfo(rid, DeploymentMode.FDE, out var _);
                }
            }

            bool TryAddInfo(string rid,
                DeploymentMode deploymentMode,
                [NotNullWhen(true)] out AppPublishInfo? info)
            {
                info = GetAppPublishInfo(rid, deploymentMode);
                if (info == null)
                    return false;
                if (!ScanPath(rid, info))
                    return false;
                if (signature)
                {
                    // TODO Windows 数字签名
                }
                if (sha256)
                {
                    foreach (var item in info.Files)
                    {
                        using var fileStream = File.OpenRead(item.FilePath);
                        item.SHA256 = Hashs.String.SHA256(fileStream);
                    }
                }
                if (sha384)
                {
                    foreach (var item in info.Files)
                    {
                        using var fileStream = File.OpenRead(item.FilePath);
                        item.SHA384 = Hashs.String.SHA384(fileStream);
                    }
                }
                infos.Add(info);
                return true;
            }
        })).ToArray();
        ThreadTask.WaitAll(tasks);
        if (infos.Any())
            AppPublishInfo.Save(infos);
        Console.WriteLine($"{commandName} OK");
    }

    internal static bool ScanPath(string? rid, AppPublishInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.DirectoryPath))
        {
            var pubPath = info.DeploymentMode switch
            {
                DeploymentMode.SCD => DirPublish_SCD,
                DeploymentMode.FDE => DirPublish_FDE,
                _ => throw new ArgumentOutOfRangeException(nameof(info.DeploymentMode), info.DeploymentMode, null),
            };
            info.DirectoryPath = Path.Combine(pubPath, rid.ThrowIsNull());
        }
        if (!Directory.Exists(info.DirectoryPath))
            return false;

        ScanPathCore(info.DirectoryPath, info.Files, ignoreRootDirNames: ignoreDirNames);
        return true;
    }

    static void ScanPathCore(string dirPath, List<AppPublishFileInfo>? list = null, string? relativeTo = null, string[]? ignoreRootDirNames = null)
    {
        list ??= new();
        relativeTo ??= dirPath;
        var files = from x in Directory.EnumerateFiles(dirPath)
                    let fileEx = Path.GetExtension(x)
                    where fileEx != ".pdb" && fileEx != ".xml"
                    && !x.EndsWith(".runtimeconfig.dev.json", StringComparison.OrdinalIgnoreCase)
                    select new AppPublishFileInfo(x, relativeTo, fileEx);
        if (files.Any())
        {
            list.AddRange(files);
        }
        var dirs = Directory.GetDirectories(dirPath);
        foreach (var dir in dirs)
        {
            if (ignoreRootDirNames != null &&
                ignoreRootDirNames.Contains(Path.GetDirectoryName(dir)))
            {
                // 忽略顶级文件夹
                continue;
            }
            ScanPathCore(dir, list, relativeTo);
        }
    }
}
