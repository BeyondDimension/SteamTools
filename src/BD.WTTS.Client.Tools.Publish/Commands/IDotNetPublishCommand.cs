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
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = ProjectPath_AvaloniaApp,
            };
            SetPublishCommandArgumentList(psi.ArgumentList, debug, info.Platform, info.DeviceIdiom, info.Architecture);
            var process = Process.Start(psi);
            process.ThrowIsNull();
            process.WaitForExit();
        }
    }

    static string GetPublishCommandByMacOSArm64()
    {
        var list = new List<string>();
        SetPublishCommandArgumentList(list, false, Platform.Apple, DeviceIdiom.Desktop, Architecture.Arm64);
        return $"dotnet {string.Join(' ', list)}";
    }

    static void SetPublishCommandArgumentList(
        IList<string> argumentList,
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
                        arg.UseAppHost = null;
                        arg.SingleFile = null;
                        arg.ReadyToRun = null;
                        arg.Trimmed = null;
                        arg.SelfContained = null;
                        arg.CreatePackage = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceIdiom), deviceIdiom, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
        SetPublishCommandArgumentList(argumentList, arg);
    }

    record struct PublishCommandArg(
        bool IsDebug,
        string Framework,
        string RuntimeIdentifier,
        bool? UseAppHost = false,
        bool? SingleFile = false,
        bool? ReadyToRun = false,
        bool? Trimmed = false,
        bool? SelfContained = false,
        bool? EnableMsixTooling = null,
        bool? GenerateAppxPackageOnBuild = null,
        bool? StripSymbols = null,
        bool? CreatePackage = null);

    static void SetPublishCommandArgumentList(
        IList<string> argumentList,
        PublishCommandArg arg)
    {
        // https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish
        // https://learn.microsoft.com/zh-cn/dotnet/core/project-sdk/msbuild-props
        // https://learn.microsoft.com/zh-cn/dotnet/maui/mac-catalyst/deployment/publish-unsigned
        // https://learn.microsoft.com/zh-cn/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp
        // https://learn.microsoft.com/zh-cn/windows/apps/package-and-deploy/project-properties
        // https://learn.microsoft.com/zh-cn/dotnet/core/compatibility/deployment/8.0/stripsymbols-default

        argumentList.Add("publish");
        var configuration = arg.IsDebug ? "Debug" : "Release";

        // 定义生成配置。 大多数项目的默认配置为 Debug，但你可以覆盖项目中的生成配置设置。
        argumentList.Add($"-c {configuration}");

        // UseAppHost 属性控制是否为部署创建本机可执行文件。 自包含部署需要本机可执行文件。
        if (arg.UseAppHost.HasValue)
            argumentList.Add($"-p:UseAppHost={arg.UseAppHost.Value.ToLowerString()}");

        // PublishDir is used by the CLI to denote the Publish target.
        argumentList.Add($@"-p:PublishDir=bin\{configuration}\Publish\{arg.RuntimeIdentifier}");

        // 将应用打包到特定于平台的单个文件可执行文件中。
        if (arg.SingleFile.HasValue)
            argumentList.Add($"-p:PublishSingleFile={arg.SingleFile.Value.ToLowerString()}");

        // 以 ReadyToRun (R2R) 格式编译应用程序集。 R2R 是一种预先 (AOT) 编译形式。 
        if (arg.ReadyToRun.HasValue)
            argumentList.Add($"-p:PublishReadyToRun={arg.ReadyToRun.Value.ToLowerString()}");

        // 在发布自包含的可执行文件时，剪裁未使用的库以减小应用的部署大小。
        if (arg.Trimmed.HasValue)
            argumentList.Add($"-p:PublishTrimmed={arg.Trimmed.Value.ToLowerString()}");

        // 当此属性为 true 时，项目的 XML 文档文件（如果已生成）包含在项目的发布输出中。 此属性的默认值为 true。
        argumentList.Add("-p:PublishDocumentationFile=false");

        // 此属性是其他几个属性的启用标志，用于控制默认是否将各种 XML 文档文件复制到发布目录，
        // 即 PublishDocumentationFile 和 PublishReferencesDocumentationFiles。
        // 如果未设置那些属性，而是设置了此属性，则这些属性将默认为 true。 此属性的默认值为 true。
        argumentList.Add("-p:PublishDocumentationFiles=false");

        // 当此属性为 true 时，将项目的引用的 XML 文档文件复制到发布目录，
        // 而不只是运行时资产（如 DLL 文件）。 此属性的默认值为 true。
        argumentList.Add("-p:PublishReferencesDocumentationFiles=false");

        //  为项目启用单项目 MSIX 功能。
        if (arg.EnableMsixTooling.HasValue)
            argumentList.Add($"-p:EnableMsixTooling={arg.EnableMsixTooling.Value.ToLowerString()}");

        if (arg.GenerateAppxPackageOnBuild.HasValue)
            argumentList.Add($"-p:GenerateAppxPackageOnBuild={arg.GenerateAppxPackageOnBuild.Value.ToLowerString()}");

        if (arg.StripSymbols.HasValue)
            argumentList.Add($"-p:StripSymbols={arg.StripSymbols.Value.ToLowerString()}");

        // (macos/maccatalyst)一个可选参数，用于控制是创建 .app 还是 .pkg。 将 false 用于 .app。
        if (arg.CreatePackage.HasValue)
            argumentList.Add($"-p:CreatePackage={arg.CreatePackage.Value.ToLowerString()}");

        // 为指定的目标框架发布应用程序。 必须在项目文件中指定目标框架。
        argumentList.Add($"-f {arg.Framework}");

        // 发布针对给定运行时的应用程序。 有关运行时标识符 (RID) 的列表，请参阅 RID 目录。
        argumentList.Add($"-r {arg.RuntimeIdentifier}");

        // .NET 运行时随应用程序一同发布，因此无需在目标计算机上安装运行时。 如果指定了运行时标识符，并且项目是可执行项目（而不是库项目），则默认值为 true。
        if (arg.SelfContained.HasValue)
            argumentList.Add($"--sc {arg.SelfContained.Value.ToLowerString()}");

        // 强制解析所有依赖项，即使上次还原已成功，也不例外。
        // 指定此标记等同于删除 project.assets.json 文件。
        argumentList.Add("--force");

        // 不显示启动版权标志或版权消息。
        argumentList.Add("--nologo");
    }
}
