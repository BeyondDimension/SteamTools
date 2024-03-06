using BD.WTTS.Client.Tools.Publish.Helpers;
using static BD.WTTS.Client.Tools.Publish.Commands.IDotNetPublishCommand;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface INSISBuildCommand : ICommand
{
    const string commandName = "nsis";

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var timestamp = new Option<string>("--t", "Release timestamp");
        var force_sign = new Option<bool>("--force-sign", GetDefForceSign, "Mandatory verification must be digitally signed");
        var hsm_sign = new Option<bool>("--hsm-sign", "");
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

        releaseTimestamp = timestamp;

        var rootDirPath = Path.Combine(ProjectUtils.ProjPath, "..", "NSIS-Build");
        var nsiFilePath = Path.Combine(rootDirPath, "AppCode", "Steampp", "app", "SteamPP_setup.nsi");

        if (!File.Exists(nsiFilePath))
        {
            Console.WriteLine($"找不到 NSIS-Build 文件，值：{nsiFilePath}");
            return;
        }

        var nsiFileContent = File.ReadAllText(nsiFilePath);
        var nsiFileContentBak = nsiFileContent;

        var appFileDirPath = Path.Combine(rootDirPath, "AppCode", "Steampp");
        var nsisExeFilePath = Path.Combine(rootDirPath, "NSIS", "makensis.exe");

        foreach (var rid in rids)
        {
            var info = DeconstructRuntimeIdentifier(rid);
            if (info == default) continue;

            var projRootPath = ProjectPath_AvaloniaApp;
            var arg = SetPublishCommandArgumentList(debug, info.Platform, info.DeviceIdiom, info.Architecture);
            var publishDir = Path.Combine(projRootPath, arg.PublishDir);
            Console.WriteLine(publishDir);
            var rootPublishDir = Path.GetFullPath(Path.Combine(publishDir, ".."));
            var packPath = $"{rootPublishDir}{FileEx._7Z}";

            var install7zFilePath = packPath;
            var install7zFileName = Path.GetFileName(install7zFilePath);
            var outputFileName = Path.GetFileNameWithoutExtension(install7zFilePath) + FileEx.EXE;
            var outputFilePath = Path.Combine(new FileInfo(install7zFilePath).DirectoryName!, outputFileName);
            IOPath.FileTryDelete(outputFilePath);
            var exeName = "Steam++.exe";

            var nsiFileContent2 = nsiFileContent
                     .Replace("${{ Steam++_Company }}", AssemblyInfo.Company)
                     .Replace("${{ Steam++_Copyright }}", AssemblyInfo.Copyright)
                     .Replace("${{ Steam++_ProductName }}", AssemblyInfo.Trademark)
                     .Replace("${{ Steam++_ExeName }}", exeName)
                     .Replace("${{ Steam++_Version }}", AppVersion4)
                     .Replace("${{ Steam++_OutPutFileName }}", outputFileName)
                     .Replace("${{ Steam++_AppFileDir }}", appFileDirPath)
                     .Replace("${{ Steam++_7zFilePath }}", install7zFilePath)
                     .Replace("${{ Steam++_7zFileName }}", install7zFileName)
                     .Replace("${{ Steam++_OutPutFilePath }}", outputFilePath)
                     .Replace("${{ Steam++_UninstFileName }}", Path.Combine(appFileDirPath, "app", "uninst.exe"))
                     ;
            File.WriteAllText(nsiFilePath, nsiFileContent2);

            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = nsisExeFilePath,
                Arguments = $" /DINSTALL_WITH_NO_NSIS7Z=1 \"{nsiFilePath}\"",
                UseShellExecute = false,
            });
            process!.WaitForExit();

            if (!debug) // 调试模式不进行数字签名
            {
                var fileNames =
$"""
"{outputFilePath}"
""";
                var pfxFilePath = hsm_sign ? MSIXHelper.SignTool.pfxFilePath_HSM_CodeSigning : null;
                try
                {
                    MSIXHelper.SignTool.Start(force_sign, fileNames, pfxFilePath, rootPublishDir);
                }
                catch
                {
                    if (debug)
                        throw;
                    pfxFilePath = MSIXHelper.SignTool.pfxFilePath_BeyondDimension_CodeSigning;
                    MSIXHelper.SignTool.Start(force_sign, fileNames, pfxFilePath, rootPublishDir);
                }
            }
        }

        File.WriteAllText(nsiFilePath, nsiFileContentBak);
    }
}
