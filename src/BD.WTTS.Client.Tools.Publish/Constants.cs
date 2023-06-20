namespace BD.WTTS.Client.Tools.Publish;

static class Constants
{
    public const string ProjectDir_AvaloniaApp = "BD.WTTS.Client.Avalonia.App";
    public const string windowssdkver = "10.0.19041.0";

    public static string DebugRuntimeConfigPath => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Debug", $"net{Environment.Version.Major}.{Environment.Version.Minor}-windows{windowssdkver}", runtimeconfigjsonfilename);

    public static string DirPublish_SCD => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Release", "Publish");

    public static string DirPublish_FDE => Path.Combine(ProjectUtils.ProjPath, "src", ProjectDir_AvaloniaApp, "bin", "Release", "Publish", "FrameworkDependent");

    public const string runtimeconfigjsonfilename = "Steam++.runtimeconfig.json";
    public const string depsjsonfilename = "Steam++.deps.json";
    public const string exefileName = "Steam++.exe";

    public static readonly string[] all_rids = new[] {
        "win-x64", /*"win-x86",*/ "win-arm64",
        "osx-x64", "osx-arm64",
        "linux-x64", "linux-arm64",
    };

    public static readonly string[] ignoreDirNames = new[]
    {
        IOPath.DirName_AppData,
        IOPath.DirName_Cache,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ThreadTask InBackground(Action action, bool longRunning = false)
    {
        TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

        if (longRunning)
        {
            options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
        }

        await ThreadTask.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
    }

    public static string GetPackPath(AppPublishInfo item, string fileEx)
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

    public static string GetFileName(AppPublishInfo item, string fileEx)
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
    public const string HARDCODED_APP_NAME = "Steam++";

    public static class LinuxPackConstants
    {
        public const string TargetName = Constants.HARDCODED_APP_NAME;
        public const string PackagePrefix = TargetName;
        public const string PackageName = PackagePrefix;
        public const string Prefix = "/usr/share/" + PackagePrefix;
        public const string Release = "0";
        public const bool CreateUser = false;
        public const string UserName = Constants.HARDCODED_APP_NAME;
        public const bool InstallService = false;
        public const string ServiceName = PackagePrefix;
        public const string RpmVendor = AssemblyInfo.Company;
        public const string Description = AssemblyInfo.Description;
        public const string Url = "https://steampp.net";
        public const string PreInstallScript = null!;
        public const string PostInstallScript = null!;
        public const string PreRemoveScript = null!;
        public const string PostRemoveScript = null!;
        public const string FileNameDesktop = Constants.HARDCODED_APP_NAME + ".desktop";
        public const string DebMaintainer = AssemblyInfo.Company;
        public const string DebSection = "misc";
        public const string DebPriority = "extra";
        public const string DebHomepage = Url;
        public static readonly string dotnet_runtime = $"dotnet-runtime-{Environment.Version.Major}.{Environment.Version.Minor}";
        public static readonly string aspnetcore_runtime = $"aspnetcore-runtime-{Environment.Version.Major}.{Environment.Version.Minor}";
    }

    public static string GetVersion()
    {
        return AssemblyInfo.Version;
    }
}
