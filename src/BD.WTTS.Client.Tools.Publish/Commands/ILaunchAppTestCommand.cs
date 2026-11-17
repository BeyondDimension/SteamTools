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

    internal static int Handler()
    {
        var path = DebugRuntimeConfigPath;

        HandlerJsonFiles(path);

        // 测试主程序启动是否正常
        path = path[..^runtimeconfigjsonfilename.Length] + exefileName;
        if (!File.Exists(path))
            return 0;
        var process = Process.Start(path);
        try
        {
            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1.25));
                if (process.HasExited)
                {
                    Console.WriteLine($"Test process startup failed({i}).");
                    return 500;
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
        return 0;
    }

#pragma warning disable CS0612 // 类型或成员已过时

    static FileStream? GetFileStream(string path)
    {
        try
        {
            return new FileStream(path, FileMode.Open,
                FileAccess.ReadWrite, IOPath.FileShareReadWriteDelete);
        }
        catch
        {
            return null;
        }
    }

    static JsonNode? TryParseJsonNode(Stream? stream)
    {
        try
        {
            var node = JsonNode.Parse(stream!)!;
            return node;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 缩小生成的运行时配置和依赖清单，保留所有声明的框架依赖。
    /// </summary>
    /// <param name="path"></param>
    static void HandlerJsonFiles(string path)
    {
        using var stream = GetFileStream(path);
        var node = TryParseJsonNode(stream);
        if (stream != null && node != null)
        {
            Write(path, stream, node);
        }

        // 减少空格换行符缩小 Json 文件大小
        path = path[..^runtimeconfigjsonfilename.Length] + depsjsonfilename;
        NotIndentedJson(path);
    }

    static void NotIndentedJson(string path)
    {
        using var stream = GetFileStream(path);
        var node = TryParseJsonNode(stream);
        Write(path, stream, node);
    }

    static void Write(string path, Stream? stream, JsonNode? node)
    {
        if (stream == null || node == null) return;
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
