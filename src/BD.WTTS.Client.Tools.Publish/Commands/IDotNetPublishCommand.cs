using BD.WTTS.Client.Tools.Publish.Helpers;
using static BD.WTTS.Client.Tools.Publish.Helpers.DotNetCLIHelper;
using static BD.WTTS.GlobalDllImportResolver;

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

#pragma warning disable CS0612 // 类型或成员已过时

    static void DirTryDelete(string path)
    {
        if (Directory.Exists(path))
        {
            foreach (var item in Directory.EnumerateDirectories(path))
            {
                Directory.Delete(item, true);
            }
            foreach (var item in Directory.EnumerateFiles(path))
            {
                File.Delete(item);
            }
        }
    }

    internal static void Handler(bool debug, string[] rids)
    {
        foreach (var rid in rids)
        {
            var info = DeconstructRuntimeIdentifier(rid);
            if (info == default) continue;
            var projRootPath = ProjectPath_AvaloniaApp;
            var psi = GetProcessStartInfo(projRootPath);
            var arg = SetPublishCommandArgumentList(psi.ArgumentList, debug, info.Platform, info.DeviceIdiom, info.Architecture);
            Console.Write("[");
            Console.Write(rid);
            Console.Write("] dotnet ");
            Console.WriteLine(string.Join(' ', psi.ArgumentList));

            var publishDir = Path.Combine(projRootPath, arg.PublishDir);
            var rootPublishDir = Path.Combine(publishDir, "..");
            DirTryDelete(rootPublishDir);

            // 发布主体
            StartProcessAndWaitForExit(psi);

            // 删除 CreateDump
            RemoveCreateDump(publishDir);

            // 移动本机库
            MoveNativeLibrary(publishDir, arg.RuntimeIdentifier, info.Platform);

            // 处理 json 文件
            var runtimeconfigjsonpath = Path.Combine(publishDir, runtimeconfigjsonfilename);
            ILaunchAppTestCommand.HandlerJsonFiles(runtimeconfigjsonpath);

            // 发布 apphost
            PublishAppHost(publishDir, info.Platform);

            // 发布插件
            PublishPlugins(debug, info.Platform, info.Architecture, publishDir, arg.Configuration, arg.Framework);
        }
    }

    /// <summary>
    /// 移除 createdump.exe
    /// </summary>
    /// <param name="arg"></param>
    static void RemoveCreateDump(string publishDir)
    {
        var path = Path.Combine(publishDir, "createdump.exe");
        if (File.Exists(path)) File.Delete(path);
        path = Path.Combine(publishDir, "createdump");
        if (File.Exists(path)) File.Delete(path);
    }

    /// <summary>
    /// 获取本机库文件名
    /// </summary>
    /// <param name="libraryName"></param>
    /// <param name="platform"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static string GetLibraryFileName(string libraryName, Platform platform, string? fileExtension = null)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            fileExtension = platform switch
            {
                Platform.UWP or Platform.Windows or Platform.WinUI => ".dll",
                Platform.Linux or Platform.Android => ".so",
                Platform.Apple => ".dylib",
                _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
            };
        }
        if (!libraryName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
            libraryName += fileExtension;
        return libraryName;
    }

    /// <summary>
    /// 移动本机库
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="platform"></param>
    static void MoveNativeLibrary(string publishDir, string runtimeIdentifier, Platform platform)
    {
        var nativeDir = Path.Combine(publishDir, "..", "native");
        var nativeWithRuntimeIdentifierDir = Path.Combine(nativeDir, runtimeIdentifier);
        IOPath.DirCreateByNotExists(nativeWithRuntimeIdentifierDir);
        foreach (var libraryName in libraryNames)
        {
            var libFileName = GetLibraryFileName(libraryName, platform);
            MoveNativeLibrary(libFileName);
        }
        switch (platform)
        {
            case Platform.Windows:
            case Platform.UWP:
            case Platform.WinUI:
                MoveNativeLibrary(GetLibraryFileName(WinDivert32, platform, ".sys"));
                MoveNativeLibrary(GetLibraryFileName(WinDivert64, platform, ".sys"));
                break;
        }

        void MoveNativeLibrary(string libFileName)
        {
            var libPath = Path.Combine(publishDir, libFileName);
            if (File.Exists(libPath))
                File.Move(libPath, Path.Combine(nativeWithRuntimeIdentifierDir, libFileName), true);
        }
    }

    static string GetPublishCommandByMacOSArm64()
    {
        var list = new List<string>();
        SetPublishCommandArgumentList(list, false, Platform.Apple, DeviceIdiom.Desktop, Architecture.Arm64);
        return $"dotnet {string.Join(' ', list)}";
    }

    /// <summary>
    /// 根据枚举值设置发布命令行参数
    /// </summary>
    /// <param name="argumentList"></param>
    /// <param name="isDebug"></param>
    /// <param name="platform"></param>
    /// <param name="deviceIdiom"></param>
    /// <param name="architecture"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static PublishCommandArg SetPublishCommandArgumentList(
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
                        arg.UseAppHost = false;
                        arg.SingleFile = false;
                        arg.ReadyToRun = false;
                        arg.Trimmed = false;
                        arg.SelfContained = false;
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
        return arg;
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
        bool? CreatePackage = null)
    {
        string? _Configuration;

        public string Configuration
        {
            get
            {
                _Configuration ??= IsDebug ? "Debug" : "Release";
                return _Configuration;
            }
        }

        string? _PublishDir;

        public string PublishDir
        {
            get
            {
                _PublishDir ??= string.Join(Path.DirectorySeparatorChar, new[]
                {
                    "bin",
                    Configuration,
                    "Publish",
                    RuntimeIdentifier,
                    "assemblies",
                });
                return _PublishDir;
            }
        }
    }

    const string publish_apphost_winany_arg = "publish -p:OutputType=WinExe -p:PublishProfile=win-any -f net35 -p:DebugType=none -p:DebugSymbols=false --nologo -v q /property:WarningLevel=1";

    static void PublishAppHost(string publishDir, Platform platform)
    {
        string? arguments = null;
        bool isWindows = false;
        switch (platform)
        {
            case Platform.Windows:
            case Platform.UWP:
            case Platform.WinUI:
                isWindows = true;
                arguments = publish_apphost_winany_arg;
                break;
        }
        var projRootPath = ProjectPath_AppHost;
        CleanProjDir(projRootPath);
        StartProcessAndWaitForExit(projRootPath,
            arguments ?? // 多次相同的编译产生的文件不会变化
            throw new ArgumentOutOfRangeException(nameof(platform), platform, null));

        var rootPublishDir = Path.Combine(publishDir, "..");
        if (isWindows)
        {
            var appHostPublishDir = Path.Combine(projRootPath, "bin", "Release", "Publish", "win-any");
            var apphostfilenames = new[]
            {
               "Steam++.exe",
               "Steam++.exe.config",
            };
            foreach (var item in apphostfilenames)
            {
                var sourceFileName = Path.Combine(appHostPublishDir, item);
                var destFileName = Path.Combine(rootPublishDir, item);
                File.Copy(sourceFileName, destFileName, true);
            }
        }
    }

    /// <summary>
    /// 设置发布命令行参数
    /// </summary>
    /// <param name="argumentList"></param>
    /// <param name="arg"></param>
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
        var configuration = arg.Configuration;

        // 定义生成配置。 大多数项目的默认配置为 Debug，但你可以覆盖项目中的生成配置设置。
        argumentList.Add("-c");
        argumentList.Add(configuration);

        // UseAppHost 属性控制是否为部署创建本机可执行文件。 自包含部署需要本机可执行文件。
        if (arg.UseAppHost.HasValue)
            argumentList.Add($"-p:UseAppHost={arg.UseAppHost.Value.ToLowerString()}");

        // PublishDir is used by the CLI to denote the Publish target.
        argumentList.Add($@"-p:PublishDir={arg.PublishDir}");

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
        argumentList.Add("-f");
        argumentList.Add(arg.Framework);

        // 发布针对给定运行时的应用程序。 有关运行时标识符 (RID) 的列表，请参阅 RID 目录。
        argumentList.Add("-r");
        argumentList.Add(arg.RuntimeIdentifier);

        argumentList.Add("-v");
        argumentList.Add("q");
        argumentList.Add("/property:WarningLevel=1");

        // .NET 运行时随应用程序一同发布，因此无需在目标计算机上安装运行时。 如果指定了运行时标识符，并且项目是可执行项目（而不是库项目），则默认值为 true。
        if (arg.SelfContained.HasValue)
        {
            argumentList.Add("--sc");
            argumentList.Add(arg.SelfContained.Value.ToLowerString());
        }

        // 强制解析所有依赖项，即使上次还原已成功，也不例外。
        // 指定此标记等同于删除 project.assets.json 文件。
        argumentList.Add("--force");

        // 不显示启动版权标志或版权消息。
        argumentList.Add("--nologo");
    }

    static IEnumerable<string> GetPluginNames()
    {
        yield return AssemblyInfo.Accelerator;
        yield return AssemblyInfo.GameAccount;
        yield return AssemblyInfo.GameList;
        //yield return AssemblyInfo.ArchiSteamFarmPlus;
        yield return AssemblyInfo.Authenticator;
        yield return AssemblyInfo.GameTools;
    }

    /// <summary>
    /// 发布插件
    /// </summary>
    /// <param name="publishDir"></param>
    /// <param name="configuration"></param>
    /// <param name="framework"></param>
    /// <exception cref="FileNotFoundException"></exception>
    static void PublishPlugins(
                bool isDebug,
                Platform platform,
                Architecture architecture,
                string publishDir,
                string configuration,
                string framework)
    {
        foreach (var pluginName in GetPluginNames())
        {
            var projRootPath = Path.Combine(ProjectUtils.ProjPath, "src", $"BD.WTTS.Client.Plugins.{pluginName}");
            StartProcessAndWaitForExit(projRootPath, $"build -c {configuration} --nologo -v q /property:WarningLevel=1");

            var dllFileName = $"BD.WTTS.Client.Plugins.{pluginName}.dll";
            var pluginBuildDir = Path.Combine(projRootPath, "bin", configuration);
            var dllPath = Path.Combine(pluginBuildDir, framework, dllFileName);
            if (!File.Exists(dllPath))
            {
                framework = framework.Split('-').FirstOrDefault()!;
                if (!string.IsNullOrEmpty(framework))
                {
                    dllPath = Path.Combine(pluginBuildDir, framework, dllFileName);
                    if (!File.Exists(dllPath))
                        throw new FileNotFoundException(null, dllPath);
                }
                else
                {
                    throw new FileNotFoundException(null, dllPath);
                }
            }

            var pluginDir = Path.Combine(publishDir, "..", "modules", pluginName);
            IOPath.DirCreateByNotExists(pluginDir);
            var destFileName = Path.Combine(pluginDir, dllFileName);
            File.Copy(dllPath, destFileName, true);

            // 复制 deps.json
            //File.Copy(dllPath.TrimEnd(".dll") + ".deps.json",
            //    destFileName.TrimEnd(".dll") + ".deps.json", true);

            switch (pluginName)
            {
                case AssemblyInfo.Accelerator:
                    PublishAcceleratorReverseProxy(pluginDir, isDebug, platform, architecture);
                    break;
            }

            static void PublishAcceleratorReverseProxy(
                string destinationDir,
                bool isDebug,
                Platform platform,
                Architecture architecture)
            {
                PublishCommandArg arg = default;
                arg.IsDebug = isDebug;

                bool isWindows = false;
                switch (platform)
                {
                    case Platform.Windows:
                    case Platform.UWP:
                    case Platform.WinUI:
                        isWindows = true;
                        break;
                }

                arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
                if (isWindows)
                {
                    //arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}-windows{windowssdkver}";
                    arg.RuntimeIdentifier = $"win-{ArchToString(architecture)}";
                }
                else
                {
                    //arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
                    switch (platform)
                    {
                        case Platform.Linux:
                            arg.RuntimeIdentifier = $"linux-{ArchToString(architecture)}";
                            break;
                        case Platform.Apple:
                            arg.RuntimeIdentifier = $"osx-{ArchToString(architecture)}";
                            break;
                    }
                }
                arg.UseAppHost = true;
                arg.SingleFile = true;
                arg.ReadyToRun = false;
                arg.Trimmed = false;
                arg.SelfContained = false;

                var projRootPath = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Plugins.Accelerator.ReverseProxy");

                CleanProjDir(projRootPath);
                var psi = GetProcessStartInfo(projRootPath);
                SetPublishCommandArgumentList(psi.ArgumentList, arg);
                if (!isWindows)
                {
                    psi.ArgumentList.Add("-p:DefineConstants=NOT_WINDOWS;$(DefineConstants)");
                }
                StartProcessAndWaitForExit(psi);

                var publishDir = Path.Combine(projRootPath, arg.PublishDir);
                CopyDirectory(publishDir, destinationDir, true);
            }
        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir, bool recursive) // https://learn.microsoft.com/zh-cn/dotnet/standard/io/how-to-copy-directories
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
        {
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, true);
            }
        }
    }
}
