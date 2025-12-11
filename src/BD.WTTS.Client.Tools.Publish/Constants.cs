namespace BD.WTTS.Client.Tools.Publish;

#pragma warning disable IDE1006 // 命名样式
#pragma warning disable SA1302 // Interface names should begin with I
interface Constants
{
    const string ProjectDir_AvaloniaApp = "BD.WTTS.Client.Avalonia.App";
    const string ProjectDir_AppHost = "BD.WTTS.Client.AppHost";
    const string windowssdkver = "10.0.19041";

    static string DebugRuntimeConfigPath => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Debug", $"net{Environment.Version.Major}.{Environment.Version.Minor}-windows{windowssdkver}", runtimeconfigjsonfilename);

    static string DirPublish_SCD => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Release", "Publish");

    static string DirPublish_FDE => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Release", "Publish", "FrameworkDependent");

    static string ProjectPath_AvaloniaApp => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp);

    static string ProjectPath_AppHost => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AppHost);

    const string runtimeconfigjsonfilename = "Steam++.runtimeconfig.json";
    const string depsjsonfilename = "Steam++.deps.json";
    const string exefileName = "Steam++.exe";

    static readonly string[] all_rids = new[] {
        "win-x64", "win-x86", "win-arm64",
        "osx-x64", "osx-arm64",
        "linux-x64", "linux-arm64", "linux-loongarch64",
    };

    static readonly string[] ignoreDirNames = new[]
    {
        IOPath.DirName_AppData,
        IOPath.DirName_Cache,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async ThreadTask InBackground(Action action, bool longRunning = false)
    {
        TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

        if (longRunning)
        {
            options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
        }

        await ThreadTask.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
    }

    static string GetPackPath(AppPublishInfo item, string fileEx)
    {
        var fileName = GetFileName(item, fileEx);
        var packPath = item.DeploymentMode switch
        {
            DeploymentMode.SCD => Path.Combine(item.DirectoryPath, "..", fileName),
            DeploymentMode.FDE => Path.Combine(item.DirectoryPath, "..", "..", fileName),
            _ => throw new ArgumentOutOfRangeException(nameof(item.DeploymentMode), item.DeploymentMode, null),
        };
        return packPath;
    }

    static string GetFileName(AppPublishInfo item, string fileEx)
    {
        var name = item.DirectoryPath.Replace("-", "_");
        if (name.Contains("osx_")) name = name.Replace("osx_", "macos_");
        var version = GetVersion();
        var fileName = item.DeploymentMode switch
        {
            DeploymentMode.SCD =>
                $"{AssemblyInfo.Trademark.Replace(" ", "_")}_with_runtime_v{version}_{name}{fileEx}",
            DeploymentMode.FDE =>
                $"{AssemblyInfo.Trademark.Replace(" ", "_")}_v{version}_{name}{fileEx}",
            _ => throw new ArgumentOutOfRangeException(nameof(item.DeploymentMode), item.DeploymentMode, null),
        };
        return fileName;
    }

    /// <summary>
    /// 当前应用程序的硬编码名称，此值不可更改！
    /// <para></para>
    /// 通常用于文件，文件夹名，等不可变值。
    /// <para></para>
    /// 可更变名称的值为 <see cref="ThisAssembly.AssemblyTrademark"/>
    /// </summary>
    const string HARDCODED_APP_NAME = "Steam++";

    interface LinuxPackConstants
    {
        const string TargetName = Constants.HARDCODED_APP_NAME;
        const string PackagePrefix = TargetName;
        const string PackageName = PackagePrefix;
        const string Prefix = "/usr/share/" + PackagePrefix;
        const string Release = "0";
        const bool CreateUser = false;
        const string UserName = Constants.HARDCODED_APP_NAME;
        const bool InstallService = false;
        const string ServiceName = PackagePrefix;
        const string RpmVendor = AssemblyInfo.Company;
        const string Description = AssemblyInfo.Description;
        const string Url = "https://steampp.net";
        const string PreInstallScript = null!;
        const string PostInstallScript = null!;
        const string PreRemoveScript = null!;
        const string PostRemoveScript = null!;
        const string FileNameDesktop = Constants.HARDCODED_APP_NAME + ".desktop";
        const string DebMaintainer = AssemblyInfo.Company;
        const string DebSection = "misc";
        const string DebPriority = "extra";
        const string DebHomepage = Url;
        static readonly string dotnet_runtime = $"dotnet-runtime-{Environment.Version.Major}.{Environment.Version.Minor}";
        static readonly string aspnetcore_runtime = $"aspnetcore-runtime-{Environment.Version.Major}.{Environment.Version.Minor}";
    }

    static string GetVersion()
    {
        return AssemblyInfo.Version;
    }

    static string ArchToString(Architecture architecture) => architecture switch
    {
        Architecture.Arm => "arm",
        Architecture.Arm64 => "arm64",
        Architecture.X64 => "x64",
        Architecture.X86 => "x86",
        Architecture.LoongArch64 => "loongarch64",
        _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, null),
    };

    static (Platform Platform, DeviceIdiom DeviceIdiom, Architecture Architecture) DeconstructRuntimeIdentifier(string rid)
    {
        (Platform Platform, DeviceIdiom DeviceIdiom, Architecture Architecture) info = default;
        var array = rid.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (array.Length == 2)
        {
            switch (array[0]?.ToLower())
            {
                case "win":
                    info.Platform = Platform.Windows;
                    info.DeviceIdiom = DeviceIdiom.Desktop;
                    break;
                case "linux":
                    info.Platform = Platform.Linux;
                    info.DeviceIdiom = DeviceIdiom.Desktop;
                    break;
                case "osx":
                    info.Platform = Platform.Apple;
                    info.DeviceIdiom = DeviceIdiom.Desktop;
                    break;
            }
            switch (array[1]?.ToLower())
            {
                case "x86":
                    info.Architecture = Architecture.X86;
                    break;
                case "x64":
                    info.Architecture = Architecture.X64;
                    break;
                case "arm":
                    info.Architecture = Architecture.Arm;
                    break;
                case "arm64":
                    info.Architecture = Architecture.Arm64;
                    break;
                case "loongarch64":
                    info.Architecture = Architecture.LoongArch64;
                    break;
                case "riscv64":
                    info.Architecture = Architecture.RiscV64;
                    break;
            }
        }
        return info;
    }

    const string feed = "https://dotnetcli.azureedge.net/dotnet";
    const string feed_no_cdn = "https://dotnetcli.blob.core.windows.net/dotnet";

    static string GetRuntimeDownloadLink(
        string osname,
        string normalized_architecture,
        bool? no_cdn = null)
    {
        // https://learn.microsoft.com/zh-cn/dotnet/core/tools/dotnet-install-script
        // https://dot.net/v1/dotnet-install.sh
        // https://dot.net/v1/dotnet-install.ps1
        if (!no_cdn.HasValue) no_cdn = !ProjectUtils.IsCI();
        var specific_version = $"{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}";
        var download_link = $"{(no_cdn.Value ? feed_no_cdn : feed)}/aspnetcore/Runtime/{specific_version}/aspnetcore-runtime-{specific_version}-{osname}-{normalized_architecture}.{(osname == "win" ? "zip" : "tar.gz")}";
        return download_link;
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
        IOPath.DirCreateByNotExists(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
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
