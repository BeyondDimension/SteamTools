namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// DotNet 发布命令
/// </summary>
interface IDotNetPublishCommand : ICommand
{
    const string commandName = "run";

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "DotNet publish app")
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
            var command = GetPublishCommand(debug, info.Platform, info.DeviceIdiom, info.Architecture);
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = command,
                WorkingDirectory = ProjectPath_AvaloniaApp,
            });
            process.ThrowIsNull();
            process.WaitForExit();
        }
    }

    static string GetPublishCommand(
        bool isDebug,
        Platform platform,
        DeviceIdiom deviceIdiom,
        Architecture architecture)
    {
        PublishCommandArg arg = default;
        arg.IsDebug = isDebug;
        switch (platform)
        {
            case Platform.Windows:
                switch (deviceIdiom)
                {
                    case DeviceIdiom.Desktop:
                        arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}-windows{windowssdkver}";
                        arg.RuntimeIdentifier = $"win-{ArchToString(architecture)}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceIdiom), deviceIdiom, null);
                }
                break;
            case Platform.Linux:
                switch (deviceIdiom)
                {
                    case DeviceIdiom.Desktop:
                        arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
                        arg.RuntimeIdentifier = $"linux-{ArchToString(architecture)}";
                        // https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-run
                        // https://download.visualstudio.microsoft.com/download/pr/c1e2729e-ab96-4929-911d-bf0f24f06f47/1b2f39cbc4eb530e39cfe6f54ce78e45/aspnetcore-runtime-7.0.7-linux-x64.tar.gz
                        // dotnet "Steam++.dll" -clt devtools
                        //arg.UseAppHost = true;
                        //arg.SingleFile = true;
                        //arg.SelfContained = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceIdiom), deviceIdiom, null);
                }
                break;
            case Platform.Apple:
                switch (deviceIdiom)
                {
                    case DeviceIdiom.Desktop:
                        arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}-macos";
                        arg.RuntimeIdentifier = $"osx-{ArchToString(architecture)}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceIdiom), deviceIdiom, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
        return GetPublishCommand(arg);
    }

    record struct PublishCommandArg(
        bool IsDebug,
        string Framework,
        string RuntimeIdentifier,
        bool UseAppHost = false,
        bool SingleFile = false,
        bool ReadyToRun = false,
        bool Trimmed = false,
        bool SelfContained = false);

    /// <summary>
    /// 获取发布命令
    /// </summary>
    /// <param name="isDebug"></param>
    /// <param name="framework"></param>
    /// <param name="rid"></param>
    /// <param name="useAppHost"></param>
    /// <param name="singleFile"></param>
    /// <param name="readyToRun"></param>
    /// <param name="trimmed"></param>
    /// <param name="selfContained"></param>
    /// <returns></returns>
    static string GetPublishCommand(PublishCommandArg arg)
    {
        // https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
        // https://learn.microsoft.com/zh-cn/dotnet/core/project-sdk/msbuild-props
        // https://learn.microsoft.com/zh-cn/dotnet/maui/mac-catalyst/deployment/publish-unsigned
        var c = arg.IsDebug ? "Debug" : "Release";
        var args = new object?[]
        {
            c,
            arg.UseAppHost.ToLowerString(),
            arg.RuntimeIdentifier,
            arg.SingleFile.ToLowerString(),
            arg.ReadyToRun.ToLowerString(),
            arg.Trimmed.ToLowerString(),
            arg.Framework,
            arg.SelfContained.ToLowerString(),
        };
        const string command =
"publish " +
"-c {0} " + // 定义生成配置。 大多数项目的默认配置为 Debug，但你可以覆盖项目中的生成配置设置。
"-p:UseAppHost={1} " + // UseAppHost 属性控制是否为部署创建本机可执行文件。 自包含部署需要本机可执行文件。
@"-p:PublishDir=bin\{0}\Publish\{2} " + // PublishDir is used by the CLI to denote the Publish target.
"-p:PublishSingleFile={3} " + // 将应用打包到特定于平台的单个文件可执行文件中。
"-p:PublishReadyToRun={4} " + // 以 ReadyToRun (R2R) 格式编译应用程序集。 R2R 是一种预先 (AOT) 编译形式。 
"-p:PublishTrimmed={5} " + // 在发布自包含的可执行文件时，剪裁未使用的库以减小应用的部署大小。
"-p:PublishDocumentationFile=false " + // 当此属性为 true 时，项目的 XML 文档文件（如果已生成）包含在项目的发布输出中。 此属性的默认值为 true。
"-p:PublishDocumentationFiles=false " + // 此属性是其他几个属性的启用标志，用于控制默认是否将各种 XML 文档文件复制到发布目录，即 PublishDocumentationFile 和 PublishReferencesDocumentationFiles。 如果未设置那些属性，而是设置了此属性，则这些属性将默认为 true。 此属性的默认值为 true。
"-p:PublishReferencesDocumentationFiles=false " + // 当此属性为 true 时，将项目的引用的 XML 文档文件复制到发布目录，而不只是运行时资产（如 DLL 文件）。 此属性的默认值为 true。
"-f {6} " + // 为指定的目标框架发布应用程序。 必须在项目文件中指定目标框架。
"-r {2} " + // (RuntimeIdentifier)发布针对给定运行时的应用程序。 有关运行时标识符 (RID) 的列表，请参阅 RID 目录。
"--sc {7} " + // (SelfContained).NET 运行时随应用程序一同发布，因此无需在目标计算机上安装运行时。 如果指定了运行时标识符，并且项目是可执行项目（而不是库项目），则默认值为 true。
"--force " + // 强制解析所有依赖项，即使上次还原已成功，也不例外。 指定此标记等同于删除 project.assets.json 文件。
"--nologo " + // 不显示启动版权标志或版权消息。
"";
        // StripSymbols
        // https://learn.microsoft.com/zh-cn/dotnet/core/compatibility/deployment/8.0/stripsymbols-default
        return string.Format(command, args);
    }
}
