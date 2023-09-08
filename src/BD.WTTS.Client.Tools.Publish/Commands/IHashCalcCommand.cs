namespace BD.WTTS.Client.Tools.Publish.Commands;

interface IHashCalcCommand : ICommand
{
    const string commandName = "hash";

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var command = new Command(commandName, "Calc hash hex string")
        {
            debug,
        };
        command.SetHandler(Handler, debug);
        return command;
    }

    static string GetSHA256(string rootPath, string fileName)
    {
        var filePath = Path.Combine(rootPath, fileName);
        if (File.Exists(filePath))
        {
            using var fs = IOPath.OpenRead(filePath);
            if (fs != null)
            {
                var value = Hashs.String.SHA256(fs);
                return value;
            }
        }
        return string.Empty;
    }

    internal static void Handler(bool debug)
    {
        var projRootPath = ProjectPath_AvaloniaApp;
        var configuration = IDotNetPublishCommand.PublishCommandArg.GetConfiguration(debug);
        var rootPath = Path.Combine(projRootPath, "bin", configuration, "Publish");

        var hash_winx64_7z = GetSHA256(rootPath,
            $"Steam++_v{AssemblyInfo.InformationalVersion}_win_x64.7z");
        var hash_winx64_exe = GetSHA256(rootPath,
            $"Steam++_v{AssemblyInfo.InformationalVersion}_win_x64.exe");
        var hash_linuxx64_tgz = GetSHA256(rootPath,
            $"Steam++_v{AssemblyInfo.InformationalVersion}_linux_x64.tgz");
        var hash_mac_dmg = GetSHA256(rootPath,
            $"Steam++_v{AssemblyInfo.InformationalVersion}_macos.dmg");

        string fileContent =
$"""
## 文件校验
|  File  | Checksum (SHA256)  |
| :- | :- |
| <sub>Steam++_v{AssemblyInfo.InformationalVersion}_win_x64.7z</sub> | <sub>{hash_winx64_7z}</sub> |
| <sub>Steam++_v{AssemblyInfo.InformationalVersion}_win_x64.exe</sub> | <sub>{hash_winx64_exe}</sub> |
|||
| <sub>Steam++_v{AssemblyInfo.InformationalVersion}_linux_x64.tgz</sub> | <sub>{hash_linuxx64_tgz}</sub> |
|||
| <sub>Steam++_v{AssemblyInfo.InformationalVersion}_macos.dmg</sub> | <sub>{hash_mac_dmg}</sub> |
""";

        var filePath = Path.Combine(projRootPath, "Checksum.md");
        IOPath.FileTryDelete(filePath);
        File.WriteAllText(filePath, fileContent);
        Console.WriteLine(fileContent);
        Console.Write("OK, SavePath: ");
        Console.WriteLine(filePath);
    }
}
