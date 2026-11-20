using BD.WTTS.Client.Tools.Publish.Helpers;
using static BD.WTTS.Client.Tools.Publish.Commands.IDotNetPublishCommand;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface INSISBuildCommand : ICommand
{
    const string commandName = "nsis";

    static Command ICommand.GetCommand()
    {
        var debug = CommandCompat.GetOption<bool>("--debug", "Defines the build configuration");
        var rids = CommandCompat.GetOption<string[]>("--rids", "RID is short for runtime identifier");
        var timestamp = CommandCompat.GetOption<string>("--t", "Release timestamp");
        var force_sign = CommandCompat.GetOption<bool>("--force-sign", GetDefForceSign, "Mandatory verification must be digitally signed");
        var hsm_sign = CommandCompat.GetOption<bool>("--hsm-sign", "");
        var command = new Command(commandName, "NSIS build generate")
        {
           debug, rids, timestamp, force_sign, hsm_sign,
        };
        command.SetHandler(Handler, debug, rids, timestamp, force_sign, hsm_sign);
        return command;
    }

    internal static void Handler(bool debug, string[] rids, string timestamp, bool force_sign, bool hsm_sign)
    {
        if (ProjectUtils.ProjPath.Contains("actions-runner"))
        {
            hsm_sign = false; // hsm 目前无法映射到 CI VM 中
        }

        var projRootPath = ProjectPath_AvaloniaApp;
        if (string.Equals("latest", timestamp, StringComparison.OrdinalIgnoreCase))
        {
            // 从发布文件夹中根据文件夹名称倒序查找最新的时间戳
            var sPath = Path.Combine(projRootPath, "bin", PublishCommandArg.GetConfiguration(debug), "Publish");
            var query = from p in Directory.EnumerateDirectories(sPath)
                        let s = Path.GetFileName(p).Split('_')
                        where s.Length > 2
                        let t = s.LastOrDefault()
                        let d = s[^2]
                        where !string.IsNullOrWhiteSpace(t) && !string.IsNullOrWhiteSpace(d)
                        let tN = ulong.TryParse(t, out var tUL) ? tUL : (ulong?)null
                        let dN = ulong.TryParse(d, out var dUL) ? dUL : (ulong?)null
                        where tN.HasValue && dN.HasValue
                        select new
                        {
                            tN,
                            dN,
                            p,
                            t,
                        };
            var latest = query.OrderByDescending(x => x.tN).ThenByDescending(x => x.dN).FirstOrDefault();
            ArgumentNullException.ThrowIfNull(latest);
            timestamp = $"{latest.dN}_{latest.t}";
        }

        releaseTimestamp = timestamp;

        var rootDirPath = Path.Combine(ProjectUtils.ProjPath, "..", "NSIS-Build");
        var nsiFilePath = Path.Combine(rootDirPath, "AppCode", "Steampp", "app", "SteamPP_setup.nsi");

        if (!File.Exists(nsiFilePath))
        {
            Console.WriteLine(string.Format(ToolStrings.Get("MissingNsisBuild"), nsiFilePath));
            return;
        }

        var installer = new NsisInstaller(rootDirPath);

        foreach (var rid in rids)
        {
            var info = DeconstructRuntimeIdentifier(rid);
            if (info == default) continue;

            var arg = SetPublishCommandArgumentList(debug, info.Platform, info.DeviceIdiom, info.Architecture);
            var publishDir = Path.Combine(projRootPath, arg.PublishDir);
            Console.WriteLine(publishDir);
            var rootPublishDir = Path.GetFullPath(Path.Combine(publishDir, ".."));
            var packPath = $"{rootPublishDir}{FileEx._7Z}";

            var outputFilePath = Path.ChangeExtension(packPath, FileEx.EXE);
            IOPath.FileTryDelete(outputFilePath);
            BuildInstaller(installer, packPath, outputFilePath, debug, force_sign, hsm_sign);
        }
    }

    internal static void BuildInstaller(NsisInstaller installer, string archive, string output, bool debug, bool forceSign, bool hsmSign, string? thumbprint = null)
    {
        installer.Build(archive, output, AppVersion4);
        if (thumbprint is not null)
        {
            MSIXHelper.SignTool.SignWithThumbprint(output, thumbprint);
            return;
        }
        if (debug)
            return;

        var fileNames = $"\"{output}\"";
        try
        {
            MSIXHelper.SignTool.Start(forceSign, fileNames, hsmSign ? MSIXHelper.SignTool.pfxFilePath_HSM_CodeSigning : null);
        }
        catch when (hsmSign)
        {
            MSIXHelper.SignTool.Start(forceSign, fileNames, MSIXHelper.SignTool.pfxFilePath_BeyondDimension_CodeSigning);
        }
    }
}
