namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// 启动应用程序测试命令
/// </summary>
interface ILaunchAppTestCommand : ICommand
{
    const string commandName = "launch";

    static Command ICommand.GetCommand()
    {
        var command = new Command(commandName, "Launch application testing")
        {

        };
        command.SetHandler(Handler);
        return command;
    }

    internal static void Handler(InvocationContext context)
    {
        var path = DebugRuntimeConfigPath;

        HandlerJsonFiles(path);

        // 测试主程序启动是否正常
        path = path[..^runtimeconfigjsonfilename.Length] + exefileName;
        if (!File.Exists(path))
            return;
        var process = Process.Start(path);
        try
        {
            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1.25));
                if (process.HasExited)
                {
                    Console.WriteLine($"Test process startup failed({i}).");
                    context.ExitCode = 500;
                    return;
                }
                else
                {
                    Console.WriteLine($"Test process startup ({i}).");
                }
            }
        }
        finally
        {
            process.KillEntireProcessTree();
        }
        Console.WriteLine($"{commandName} OK");
    }

    /// <summary>
    /// 根据 runtimeconfig.json 文件路径删除 Microsoft.WindowsDesktop.App 节点
    /// </summary>
    /// <param name="path"></param>
    static void HandlerJsonFiles(string path)
    {
        using var stream = new FileStream(path, FileMode.Open,
            FileAccess.ReadWrite, IOPath.FileShareReadWriteDelete);
        var node = JsonNode.Parse(stream!)!;
        var frameworks = node["runtimeOptions"]?["frameworks"]?.AsArray();
        var desktopNode = frameworks?.FirstOrDefault(x =>
            x?["name"]?.GetValue<string>() == "Microsoft.WindowsDesktop.App");
        if (desktopNode != null)
        {
            frameworks!.Remove(desktopNode);
            Write(path, stream, node);
        }

        // 减少空格换行符缩小 Json 文件大小
        path = path[..^runtimeconfigjsonfilename.Length] + depsjsonfilename;
        NotIndentedJson(path);
    }

    static void NotIndentedJson(string path)
    {
        using var stream = new FileStream(path, FileMode.Open,
            FileAccess.ReadWrite, IOPath.FileShareReadWriteDelete);
        var node = JsonNode.Parse(stream!)!;
        Write(path, stream, node);
    }

    static void Write(string path, Stream stream, JsonNode node)
    {
        var len = stream.Length;
        stream.Position = 0;
        using var writer = new Utf8JsonWriter(stream);
        node.WriteTo(writer);
        writer.Flush();
        stream.Flush();
        Console.WriteLine($"{Path.GetFileName(path)} size: {IOPath.GetDisplayFileSizeString(len)} => {IOPath.GetDisplayFileSizeString(stream.Position)}");
        stream.SetLength(stream.Position);
    }
}
