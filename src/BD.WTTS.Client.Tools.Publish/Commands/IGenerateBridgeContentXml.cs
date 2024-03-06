using static BD.WTTS.Client.Tools.Publish.Commands.IDotNetPublishCommand;
using static BD.WTTS.Client.Tools.Publish.Helpers.DotNetCLIHelper;
using static BD.WTTS.GlobalDllImportResolver;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.Client.Tools.Publish.Commands;

interface IGenerateBridgeContentXml : ICommand
{
    const string commandName = "csproj";

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "Bridge csproj xml generate")
        {
           debug, rids,
        };
        command.SetHandler(Handler, debug, rids);
        return command;
    }

    internal static void Handler(bool debug, string[] rids)
    {
        foreach (var rid in rids)
        {
            var info = DeconstructRuntimeIdentifier(rid);
            if (info == default) continue;

            var projRootPath = ProjectPath_AvaloniaApp;
            var psi = GetProcessStartInfo(projRootPath);
            var arg = SetPublishCommandArgumentList(debug, info.Platform, info.DeviceIdiom, info.Architecture);

            var publishDir = Path.Combine(projRootPath, arg.PublishDir);
            Console.WriteLine(publishDir);
            var rootPublishDir = Path.GetFullPath(Path.Combine(publishDir, ".."));

            var jsonFilePath = $"{rootPublishDir}.json";
            using var jsonFileStream = File.Open(jsonFilePath, FileMode.Open, FileAccess.Read);
            var appPublish = JsonSerializer.Deserialize(jsonFileStream, new AppPublishInfoContext(new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }).AppPublishInfo);

            if (!(appPublish?.Files.Any_Nullable() ?? false))
                continue;

            using var stream = new MemoryStream();
            stream.Write($"  <!--[Start] BD.WTTS.Client.Tools.Publish({(debug ? "Debug" : "Release")}-{rid}) -->");
            stream.WriteNewLine();
            stream.Write("  <ItemGroup>"u8);
            stream.WriteNewLine();
            var bridgeProjectDirPath = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.AppHost.Bridge.Package");
            foreach (var item in appPublish.Files)
            {
                var fileName = Path.GetFileName(item.FilePath);
                if (fileName == "Steam++.exe" || fileName == "Steam++.exe.config")
                    continue;

                var include = Path.GetRelativePath(bridgeProjectDirPath, item.FilePath);
                stream.Write(
$"""
    <Content Include="{include}">

""");
                stream.Write(
$"""
      <Link>Steam++\{item.RelativePath.TrimStart('\\')}</Link>

""");
                stream.Write(
"""
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>

"""u8);
                stream.Write(
"""
    </Content>

"""u8);
            }
            stream.Write("  </ItemGroup>"u8);
            stream.WriteNewLine();
            stream.Write($"  <!--[End] BD.WTTS.Client.Tools.Publish({(debug ? "Debug" : "Release")}-{rid}) -->");

            stream.Position = 0;

            var str = Encoding.UTF8.GetString(stream.ToByteArray());
            Console.WriteLine(str);
        }
    }
}
