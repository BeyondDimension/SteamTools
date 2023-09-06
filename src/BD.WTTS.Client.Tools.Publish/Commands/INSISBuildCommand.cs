using static BD.WTTS.Client.Tools.Publish.Commands.IDotNetPublishCommand;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface INSISBuildCommand : ICommand
{
    const string commandName = "nsis";

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "NSIS build generate")
        {
           debug, rids,
        };
        command.SetHandler(Handler, debug, rids);
        return command;
    }

    internal static void Handler(bool debug, string[] rids)
    {
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
                     ;
            File.WriteAllText(nsiFilePath, nsiFileContent2);

            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = nsisExeFilePath,
                Arguments = $" /DINSTALL_WITH_NO_NSIS7Z=1 \"{nsiFilePath}\"",
                UseShellExecute = false,
            });
            process!.WaitForExit();
        }

        File.WriteAllText(nsiFilePath, nsiFileContentBak);
    }
}
