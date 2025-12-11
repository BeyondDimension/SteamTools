using BD.WTTS.Client.Tools.Publish.Helpers;
using System.Reflection;
using System;
using System.Security.Policy;
using static BD.WTTS.Client.Tools.Publish.Helpers.DotNetCLIHelper;
using static BD.WTTS.GlobalDllImportResolver;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Runtime.InteropServices;

namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// DotNet 发布命令
/// </summary>
interface IDotNetPublishCommand : ICommand
{
    const string commandName = "run";

    const string EntryPointAssemblyName = "Steam++";

    static string releaseTimestamp = DateTimeOffset.Now.ToString("yyMMdd_HHmmssfffffff");

    static string GetPublishFileName(bool debug, string rid, string fileEx = "")
    {
        var value = $"[{(debug ? "Debug" : "Release")}] {EntryPointAssemblyName}_v{AssemblyInfo.InformationalVersion}_{rid.Replace('-', '_')}_{releaseTimestamp}{fileEx}";
        return value;
    }

    static bool GetDefForceSign()
    {
        var machineName = Hashs.String.SHA256(Environment.MachineName, false);
        return machineName switch
        {
            "EACD5C77C0E7160CF8D2A6C21C4F0C1F04CEF40097DB4799127AABB2CF8786B6" or
            "E34AB34336AF93190C550A082960F7610D01DE121897F432D9A5CBC6E326B5AB"
            => true,
            _ => false,
        };
    }

    static Command ICommand.GetCommand()
    {
        var debug = new Option<bool>("--debug", "Defines the build configuration");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var force_sign = new Option<bool>("--force-sign", GetDefForceSign, "Mandatory verification must be digitally signed");
        var hsm_sign = new Option<bool>("--hsm-sign", "");
        var sha256 = new Option<bool>("--sha256", () => true, "Calculate file hash value");
        var sha384 = new Option<bool>("--sha384", () => true, "Calculate file hash value");
        var stm_upload = new Option<bool>("--stm-upload", "Steam upload zip file");
        var command = new Command(commandName, "DotNet publish app")
        {
           debug, rids, force_sign, sha256, sha384, stm_upload, hsm_sign,
        };
        command.SetHandler(Handler, debug, rids, force_sign, sha256, sha384, stm_upload, hsm_sign);
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

    const string DllSystemDrawingCommon = "System.Drawing.Common.dll";
    const string DllMicrosoftWin32SystemEvents = "Microsoft.Win32.SystemEvents.dll";
    const string DllSystemManagement = "System.Management.dll";
    const string DllSplatDrawing = "Splat.Drawing.dll";
    const string DllSystemReactive = "System.Reactive.dll";

    static readonly Lazy<string> nugetPkgPath = new(() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget",
            "packages"));

    static string GetMatchTfmDir(string dllPath)
    {
        var versions = from m in Directory.GetDirectories(dllPath)
                       let dirName = Path.GetFileName(m)
                       let ver = Version.TryParse(dirName.TrimStart("net"), out var ver_) ? ver_ : null
                       where ver != null && ver.Major <= Environment.Version.Major
                       orderby ver descending
                       select (ver, m);
        var path = versions.FirstOrDefault().m;
        return path;
    }

    static bool MatchPackageVersion(string dllName, Version ver, FileVersionInfo? fvi = null) => dllName switch
    {
        DllSystemDrawingCommon => ver.Major <= Environment.Version.Major,
        _ => fvi == null || (!Version.TryParse(fvi.FileVersion, out var fVer) || (ver.Major <= fVer.Major)),
    };

    static void CopyWindowsDlls(string publishDir)
    {
        // 修复：存在 Microsoft.WindowsDesktop.App 依赖时不会将 System.Drawing.Common.dll 复制到输出目录
        string[] copyToDlls = [DllSystemDrawingCommon, DllMicrosoftWin32SystemEvents, DllSystemManagement];
        foreach (var copyToDll in copyToDlls)
        {
            var dllExistsPath = Path.Combine(publishDir, copyToDll);
            if (!File.Exists(dllExistsPath))
            {
                var pkgDir = Path.Combine(nugetPkgPath.Value, copyToDll.TrimEnd(".dll", StringComparison.OrdinalIgnoreCase));
                var versions = from m in Directory.GetDirectories(pkgDir)
                               let dirName = Path.GetFileName(m)
                               let ver = Version.TryParse(dirName, out var ver_) ? ver_ : null
                               where ver != null && MatchPackageVersion(copyToDll, ver)
                               orderby ver descending
                               select (ver, m);
                var path = versions.FirstOrDefault().m;
                var dllPath = Path.Combine(path, "lib");
                dllPath = GetMatchTfmDir(dllPath);
                dllPath = Path.Combine(dllPath, copyToDll);
                File.Copy(dllPath, dllExistsPath);
            }
        }

        // 修复：Splat.Drawing 包存在 WPF 的依赖项
        // 修复：System.Reactive 包存在 System.Windows.Forms 的依赖项
        string[] replaceToDlls = [DllSplatDrawing, DllSystemReactive];
        foreach (var replaceToDll in replaceToDlls)
        {
            var dllExistsPath = Path.Combine(publishDir, replaceToDll);
            if (File.Exists(dllExistsPath))
            {
                var fvi = FileVersionInfo.GetVersionInfo(dllExistsPath);
                var pkgDir = Path.Combine(nugetPkgPath.Value, replaceToDll.TrimEnd(".dll", StringComparison.OrdinalIgnoreCase));
                var versions = from m in Directory.GetDirectories(pkgDir)
                               let dirName = Path.GetFileName(m)
                               let ver = Version.TryParse(dirName, out var ver_) ? ver_ : null
                               where ver != null && MatchPackageVersion(replaceToDll, ver, fvi)
                               orderby ver descending
                               select (ver, m);
                var path = versions.FirstOrDefault().m;
                var dllPath = Path.Combine(path, "lib");
                dllPath = GetMatchTfmDir(dllPath);
                dllPath = Path.Combine(dllPath, replaceToDll);
                File.Delete(dllExistsPath);
                File.Copy(dllPath, dllExistsPath);
            }
        }
    }

    internal static void Handler(bool debug, string[] rids, bool force_sign, bool sha256, bool sha384, bool stm_upload, bool hsm_sign)
    {
        if (ProjectUtils.ProjPath.Contains("actions-runner"))
        {
            hsm_sign = false; // hsm 目前无法映射到 CI VM 中
        }

        var bgOriginalColor = Console.BackgroundColor;
        var fgOriginalColor = Console.ForegroundColor;

        void ResetConsoleColor()
        {
            Console.BackgroundColor = bgOriginalColor;
            Console.ForegroundColor = fgOriginalColor;
        }
        void SetConsoleColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
        }

        try
        {
            foreach (var rid in rids)
            {
                var info = DeconstructRuntimeIdentifier(rid);
                if (info == default) continue;

                bool isWindows = false;
                bool isCopyRuntime = false;
                switch (info.Platform)
                {
                    case Platform.Windows:
                    case Platform.UWP:
                    case Platform.WinUI:
                        isWindows = true;
                        isCopyRuntime = true;
                        break;
                    case Platform.Linux:
                        isCopyRuntime = true;
                        break;
                }

                var isWinArm64 = isWindows && info.Architecture == Architecture.Arm64;

                var projRootPath = ProjectPath_AvaloniaApp;
                var psi = GetProcessStartInfo(projRootPath);
                var arg = SetPublishCommandArgumentList(debug, info.Platform, info.DeviceIdiom, info.Architecture);
                if (isWinArm64) // win-arm64
                {
                    // steamclient.dll 缺少 Arm64，以及 Arm64EC 在 dotnet 中目前不可用
                    // 更改为 x64 兼容运行主进程
                    arg.RuntimeIdentifier = "win-x64";
                    arg.PublishDir = arg.PublishDir.Replace("win-x64", "win-arm64");
                }
                SetPublishCommandArgumentList(psi.ArgumentList, arg);

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.Write("[");
                Console.Write(rid);
                Console.Write("] dotnet ");
                Console.WriteLine(string.Join(' ', psi.ArgumentList));
                ResetConsoleColor();

                var publishDir = Path.Combine(projRootPath, arg.PublishDir);
                Console.WriteLine(publishDir);
                var rootPublishDir = Path.GetFullPath(Path.Combine(publishDir, ".."));
                //switch (info.Platform)
                //{
                //    case Platform.Linux:
                //        rootPublishDir = Path.GetFullPath(publishDir);
                //        break;
                //}
                DirTryDelete(rootPublishDir);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.Write("已删除目录：");
                Console.WriteLine(rootPublishDir);
                ResetConsoleColor();

                // 发布主体
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始发布【主项目】");
                ResetConsoleColor();
                ProcessHelper.StartAndWaitForExit(psi);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("发布成功【主项目】");
                ResetConsoleColor();

                // 验证 Avalonia.Base.dll 版本号必须为 11+
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始验证【Avalonia.Base.dll 版本号必须为 11+】");
                ResetConsoleColor();
                if (arg.SingleFile.HasValue && !arg.SingleFile.Value)
                {
                    var avaloniaBaseDllPath = Path.Combine(publishDir, "Avalonia.Base.dll");
                    var avaloniaBaseDllVersion = Version.Parse(FileVersionInfo.GetVersionInfo(avaloniaBaseDllPath).FileVersion!);
                    if (avaloniaBaseDllVersion < new Version(11, 0))
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(avaloniaBaseDllVersion),
                            avaloniaBaseDllVersion, null);
                    }
                }

                if (info.Platform == Platform.Linux)
                {
                    Console.WriteLine("Linux 需要 Ico 导入系统图标");
                    var ico_path = Path.Combine(ProjectUtils.ProjPath, "res", "icons", "app", "v3", "Logo_512.png");
                    var save_dir_path = Path.Combine(rootPublishDir, "Icons");
                    if (File.Exists(ico_path))
                    {
                        IOPath.DirCreateByNotExists(save_dir_path);
                        // 不能使用下划线
                        File.Copy(ico_path, Path.Combine(save_dir_path, "Watt-Toolkit.png"), true);
                    }
                    Console.WriteLine("Linux 复制启动 环境检查 卸载脚本");
                    var script_path = Path.Combine(rootPublishDir, "..", "ShellScript", "Linux");//Path.Combine(ProjectUtils.ProjPath, "build", "linux");
                    CopyDirectory(script_path, Path.Combine(rootPublishDir, "script"), true);
                    File.Move(Path.Combine(rootPublishDir, "script", "Steam++.sh"), Path.Combine(rootPublishDir, "Steam++.sh"));
                }

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("验证成功【Avalonia.Base.dll 版本号必须为 11+】");
                ResetConsoleColor();

                // 删除 CreateDump
                RemoveCreateDump(publishDir);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("已删除 CreateDump");
                ResetConsoleColor();

                // 移动本机库
                MoveNativeLibrary(publishDir, arg.RuntimeIdentifier, info.Platform);
                if (isWinArm64)
                {
                    // 删除相关文件来禁用 DNS 驱动模式，该驱动不支持 Arm64
                    var nativeDir = Path.Combine(publishDir, "..", "native", "win-x64");
                    foreach (var item in Directory.GetFiles(nativeDir, "WinDivert*"))
                    {
                        IOPath.FileTryDelete(item);
                    }
                }
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("移动本机库");
                ResetConsoleColor();

                // 处理 json 文件
                var runtimeconfigjsonpath = Path.Combine(publishDir, runtimeconfigjsonfilename);
                ILaunchAppTestCommand.HandlerJsonFiles(runtimeconfigjsonpath, info.Platform);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("已处理 json 文件");
                ResetConsoleColor();

                if (isWindows)
                {
                    // 发布 apphost
                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                    Console.WriteLine("开始发布【AppHost】");
                    ResetConsoleColor();
                    PublishAppHost(publishDir, info.Platform, debug);
                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                    Console.WriteLine("发布成功【AppHost】");
                    ResetConsoleColor();

                    CopyWindowsDlls(publishDir);
                }

                // 发布插件
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始发布【插件】");
                ResetConsoleColor();
                PublishPlugins(debug, info.Platform, info.Architecture, publishDir, arg.Configuration, arg.Framework);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("发布成功【插件】");
                ResetConsoleColor();

                // 复制运行时
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始复制运行时");
                ResetConsoleColor();
                CopyRuntime(rootPublishDir, info.Platform, isCopyRuntime, info.Architecture);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("完成复制运行时");
                ResetConsoleColor();

                var appPublish = new AppPublishInfo()
                {
                    DeploymentMode = DeploymentMode.SCD,
                    RuntimeIdentifier = arg.RuntimeIdentifier,
                    DirectoryPath = rootPublishDir,
                };

                IOPath.DirTryDelete(Path.Combine(rootPublishDir, IOPath.DirName_AppData));
                IOPath.DirTryDelete(Path.Combine(rootPublishDir, IOPath.DirName_Cache));
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("已删除 AppData/Cache");
                ResetConsoleColor();

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始扫描文件");
                ResetConsoleColor();
                IScanPublicDirectoryCommand.ScanPathCore(appPublish.DirectoryPath,
                    appPublish.Files,
                    ignoreRootDirNames: ignoreDirNames);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("完成扫描文件");
                ResetConsoleColor();

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始文件计算哈希值");
                ResetConsoleColor();
                if (sha256)
                {
                    foreach (var item in appPublish.Files)
                    {
                        using var fileStream = File.OpenRead(item.FilePath);
                        item.SHA256 = Hashs.String.SHA256(fileStream);
                    }
                }
                if (sha384)
                {
                    foreach (var item in appPublish.Files)
                    {
                        using var fileStream = File.OpenRead(item.FilePath);
                        item.SHA384 = Hashs.String.SHA384(fileStream);
                    }
                }
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine("完成文件计算哈希值");
                ResetConsoleColor();

                if (OperatingSystem.IsWindows() && isWindows)
                {
                    // 数字签名
                    List<AppPublishFileInfo> toBeSignedFiles = new();
                    HashSet<string> toBeSignedFilePaths = new();
                    foreach (var item in appPublish.Files!)
                    {
                        switch (item.FileEx.ToLowerInvariant())
                        {
                            case ".dll" or ".exe" or ".sys":
                                {
                                    if (item.FileInfo?.Name.Contains("xunyoucall",
                                        StringComparison.OrdinalIgnoreCase) ?? false)
                                    {
                                        continue;
                                    }
                                    if (!MSIXHelper.IsDigitalSigned(item.FilePath))
                                    {
                                        toBeSignedFiles.Add(item);
                                        toBeSignedFilePaths.Add(item.FilePath);
                                    }
                                }
                                break;
                        }
                    }

                    if (toBeSignedFilePaths.Count != 0)
                    {
                        SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                        Console.Write("正在进行数字签名，文件数量：");
                        Console.WriteLine(toBeSignedFilePaths.Count);
                        ResetConsoleColor();
                        var fileNames = string.Join(' ', toBeSignedFilePaths.Select(x =>
$"""
"{x.TrimStart(Path.DirectorySeparatorChar).TrimStart(rootPublishDir).TrimStart(Path.DirectorySeparatorChar)}"
"""));
                        if (!debug) // 调试模式不进行数字签名
                        {
                            var pfxFilePath = hsm_sign ? MSIXHelper.SignTool.pfxFilePath_HSM_CodeSigning : null;
                            try
                            {
                                MSIXHelper.SignTool.Start(force_sign, fileNames, pfxFilePath, rootPublishDir);
                            }
                            catch
                            {
                                Console.WriteLine("数字签名失败，输入回车使用自签继续");
                                Console.ReadLine();
                                if (debug)
                                    throw;
                                MSIXHelper.SignTool.Start(force_sign, fileNames, MSIXHelper.SignTool.pfxFilePath_BeyondDimension_CodeSigning, rootPublishDir);
                            }
                        }
                        foreach (var item in toBeSignedFiles)
                        {
                            if (sha256)
                            {
                                using var fileStream = File.OpenRead(item.FilePath);
                                item.SignatureSHA256 = Hashs.String.SHA256(fileStream);
                            }
                            if (sha384)
                            {
                                using var fileStream = File.OpenRead(item.FilePath);
                                item.SignatureSHA384 = Hashs.String.SHA384(fileStream);
                            }
                        }
                    }

                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                    Console.WriteLine("开始生成【MSIX 包】");
                    ResetConsoleColor();
                    // 生成清单文件
                    MSIXHelper.MakeAppx.GenerateAppxManifestXml(rootPublishDir, AppVersion4, info.Architecture);

                    // 打包资源 images
                    MSIXHelper.MakePri.Start(rootPublishDir);

                    var msixDir = $"{rootPublishDir}_MSIX";
                    IOPath.DirCreateByNotExists(msixDir);
                    var msixFilePath = Path.Combine(msixDir, GetPublishFileName(debug, rid, ".msix"));

                    // 生成 msix 包
                    MSIXHelper.MakeAppx.Start(msixFilePath, rootPublishDir, AppVersion4, info.Architecture);
                    Thread.Sleep(TimeSpan.FromSeconds(1.15d));

                    // 签名 msix 包
                    // msix 签名证书名必须与包名一致
                    MSIXHelper.SignTool.Start(force_sign, $"\"{msixFilePath}\"", MSIXHelper.SignTool.pfxFilePath_MSStore_CodeSigning);

                    var msixBundleFilePath = $"{rootPublishDir}.msixbundle";
                    MSIXHelper.MakeAppx.StartBundle(msixBundleFilePath, msixDir, AppVersion4);
                    Thread.Sleep(TimeSpan.FromSeconds(1.15d));

                    // 签名 msix 包
                    // msix 签名证书名必须与包名一致
                    MSIXHelper.SignTool.Start(force_sign, $"\"{msixBundleFilePath}\"", MSIXHelper.SignTool.pfxFilePath_MSStore_CodeSigning);

                    using var msixFileStream = File.OpenRead(msixBundleFilePath);

                    var msixInfo = new AppPublishFileInfo
                    {
                        FileEx = ".msixbundle",
                        FilePath = msixBundleFilePath,
                        Length = msixFileStream.Length,
                        SignatureSHA384 = Hashs.String.SHA384(msixFileStream),
                    };
                    appPublish.SingleFile.Add(CloudFileType.MsixBundle, msixInfo);

                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                    Console.Write("已生成【MSIX 包】，文件大小：");
                    Console.Write(IOPath.GetDisplayFileSizeString(msixInfo.Length));
                    Console.Write("，路径：");
                    Console.WriteLine(msixFilePath);
                    ResetConsoleColor();
                }

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine("开始创建【压缩包】");
                ResetConsoleColor();
                string? packPath = null;
                string GetPackPathWithTryDelete(string fileEx)
                {
                    var packPath = $"{rootPublishDir}{fileEx}";
                    IOPath.FileTryDelete(packPath);
                    return packPath;
                }
                switch (info.Platform)
                {
                    case Platform.Windows:
                    case Platform.UWP:
                    case Platform.WinUI:
                    case Platform.Apple:
                        ICompressedPackageCommand.CreateSevenZipPack(packPath = GetPackPathWithTryDelete(FileEx._7Z), appPublish.Files);
                        break;
                    case Platform.Linux:
                        ICompressedPackageCommand.CreateGZipPack(packPath = GetPackPathWithTryDelete(FileEx.TAR_GZ), appPublish.Files);
                        break;
                }
                if (stm_upload)
                {
                    List<AppPublishFileInfo> mainFiles = new();
                    List<AppPublishFileInfo> modulesFiles = new();
                    List<AppPublishFileInfo> stmUploads = new();
                    foreach (var item in appPublish.Files)
                    {
                        if (item.RelativePath.Contains("modules"))
                        {
                            modulesFiles.Add(item);
                        }
                        else
                        {
                            mainFiles.Add(item);
                        }
                    }
                    var stmupload_main_zip_path = GetPackPathWithTryDelete("_Main.stmupload.zip");
                    stmUploads.Add(new()
                    {
                        FilePath = stmupload_main_zip_path,
                        RelativePath = Path.GetFileName(stmupload_main_zip_path),
                    });
                    ICompressedPackageCommand.CreateZipPack(stmupload_main_zip_path, mainFiles);

                    var query = from module in modulesFiles
                                let split = module.RelativePath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                                let name = split.Length >= 2 ? split[1] : null
                                where name != null
                                group module by name;
                    var items = query.ToArray();
                    foreach (var item in items)
                    {
                        if (string.IsNullOrWhiteSpace(item.Key))
                            continue;
                        var files = item.ToArray();
                        var stmupload_item_zip_path = GetPackPathWithTryDelete($"_{item.Key}.stmupload.zip");
                        stmUploads.Add(new()
                        {
                            FilePath = stmupload_item_zip_path,
                            RelativePath = Path.GetFileName(stmupload_item_zip_path),
                        });
                        ICompressedPackageCommand.CreateZipPack(stmupload_item_zip_path, files);
                    }
                    ICompressedPackageCommand.CreateZipPack(GetPackPathWithTryDelete(".stmupload.zip"), stmUploads);
                }
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.Write("创建成功【压缩包】，文件大小：");
                if (packPath != null)
                {
                    Console.Write(IOPath.GetDisplayFileSizeString(new FileInfo(packPath).Length));
                }
                else
                {
                    Console.Write(0);
                }
                Console.Write("，路径：");
                Console.WriteLine(packPath);
                ResetConsoleColor();

                var jsonFilePath = $"{rootPublishDir}.json";
                using var jsonFileStream = File.Open(jsonFilePath, FileMode.OpenOrCreate);
                JsonSerializer.Serialize(jsonFileStream, appPublish, new AppPublishInfoContext(new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                }).AppPublishInfo);
                jsonFileStream.Flush();
                jsonFileStream.SetLength(jsonFileStream.Position);

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.Write("发布文件信息清单已生成，文件大小：");
                Console.Write(IOPath.GetDisplayFileSizeString(appPublish.Files.Sum(x => x.Length)));
                Console.Write("，路径：");
                Console.WriteLine(jsonFilePath);
                ResetConsoleColor();
            }

            SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
            Console.WriteLine("OK");
            ResetConsoleColor();
        }
        finally
        {
            ResetConsoleColor();
        }
    }

    private static readonly Lazy<string> _AppVersion4 = new(() =>
    {
        var v = new Version(AssemblyInfo.FileVersion);
        static int GetInt32(int value) => value < 0 ? 0 : value;
        return $"{GetInt32(v.Major)}.{GetInt32(v.Minor)}.{GetInt32(v.Build)}.{GetInt32(v.Revision)}";
    });

    static string AppVersion4 => _AppVersion4.Value;

    /// <summary>
    /// 将运行时复制到发布根目录下
    /// </summary>
    /// <param name="rootPublishDir"></param>
    /// <param name="isWindows"></param>
    /// <param name="architecture"></param>
    static void CopyRuntime(string rootPublishDir, Platform platform, bool isCopyRuntime, Architecture architecture)
    {
        switch (platform)
        {
            case Platform.UWP:
            case Platform.WinUI:
            case Platform.Windows:
                if (isCopyRuntime)
                {
                    //if (architecture == Architecture.Arm64 &&
                    //    RuntimeInformation.OSArchitecture != Architecture.Arm64)
                    //{
                    //    // TODO
                    //    return;
                    //}

                    var programFiles = Environment.GetFolderPath(
                        architecture == Architecture.X86 ?
                        Environment.SpecialFolder.ProgramFilesX86 :
                        Environment.SpecialFolder.ProgramFiles);

                    static string get_hostfxr_path(string rootPath) => Path.Combine(rootPath,
                        "dotnet",
                        "host",
                        "fxr",
                        $"{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}",
                        "hostfxr.dll");

                    var hostfxr_path = get_hostfxr_path(programFiles);

                    static string get_aspnetcore_path(string rootPath) => Path.Combine(rootPath,
                        "dotnet",
                        "shared",
                        "Microsoft.AspNetCore.App",
                        $"{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}");

                    var aspnetcore_path = get_aspnetcore_path(programFiles);

                    static string get_win_desktop_path(string rootPath) => Path.Combine(rootPath,
                        "dotnet",
                        "shared",
                        "Microsoft.WindowsDesktop.App",
                        $"{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}");

                    var win_desktop_path = get_win_desktop_path(programFiles);

                    static string get_netcore_path(string rootPath) => Path.Combine(rootPath,
                        "dotnet",
                        "shared",
                        "Microsoft.NETCore.App",
                        $"{Environment.Version.Major}.{Environment.Version.Minor}.{Environment.Version.Build}");

                    var netcore_path = get_netcore_path(programFiles);

                    if (Directory.Exists(netcore_path) &&
                        Directory.Exists(aspnetcore_path) &&
                        File.Exists(hostfxr_path))
                    {
                        var dest_hostfxr_path = get_hostfxr_path(rootPublishDir);
                        IOPath.DirCreateByNotExists(Path.GetDirectoryName(dest_hostfxr_path)!);
                        File.Copy(hostfxr_path, dest_hostfxr_path);
                        CopyDirectory(netcore_path, get_netcore_path(rootPublishDir), true);
                        CopyDirectory(aspnetcore_path, get_aspnetcore_path(rootPublishDir), true);
                        CopyDirectory(win_desktop_path, get_win_desktop_path(rootPublishDir), true);
                    }
                }
                break;
            case Platform.Linux:
                if (isCopyRuntime)
                {
                    var dotnet_path = Path.Combine(rootPublishDir, "..", "dotnet-Runtime", $"{Platform.Linux}-{architecture}");
                    CopyDirectory(dotnet_path, Path.Combine(rootPublishDir, "dotnet"), true);
                    // TODO
                }
                break;
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
        //switch (platform)
        //{
        //    case Platform.Linux:
        //        nativeDir = Path.Combine(publishDir, "native");
        //        nativeWithRuntimeIdentifierDir = Path.Combine(nativeDir, runtimeIdentifier);
        //        break;
        //}
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
            else if (!libFileName.StartsWith("lib", StringComparison.OrdinalIgnoreCase))
            {
                libPath = Path.Combine(publishDir, "lib" + libFileName);
                if (File.Exists(libPath))
                    File.Move(libPath, Path.Combine(nativeWithRuntimeIdentifierDir, libFileName), true);
            }
            else
            {
                libFileName = libFileName.TrimStart("lib", StringComparison.OrdinalIgnoreCase);
                libPath = Path.Combine(publishDir, libFileName);
                if (File.Exists(libPath))
                    File.Move(libPath, Path.Combine(nativeWithRuntimeIdentifierDir, libFileName), true);
            }
        }
    }

    static string GetPublishCommandByMacOSArm64()
    {
        var list = new List<string>();
        var arg = SetPublishCommandArgumentList(false, Platform.Apple, DeviceIdiom.Desktop, Architecture.Arm64);
        SetPublishCommandArgumentList(list, arg);
        return $"dotnet {string.Join(' ', list)}";
    }

    /// <summary>
    /// 根据枚举值设置发布命令行参数
    /// </summary>
    /// <param name="isDebug"></param>
    /// <param name="platform"></param>
    /// <param name="deviceIdiom"></param>
    /// <param name="architecture"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static PublishCommandArg SetPublishCommandArgumentList(
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
                        arg.UseAppHost = false;
                        arg.SingleFile = false;
                        arg.ReadyToRun = false;
                        arg.Trimmed = false;
                        arg.SelfContained = false;
                        arg.Architecture = architecture;
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
                        arg.CreatePackage = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceIdiom), deviceIdiom, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
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
        bool? CreatePackage = null,
        Architecture? Architecture = null
        )
    {
        string? _Configuration;

        public static string GetConfiguration(bool debug) => debug ? "Debug" : "Release";

        public string Configuration
        {
            get
            {
                _Configuration ??= GetConfiguration(IsDebug);
                return _Configuration;
            }
        }

        string? _PublishDir;

        public string GetPublishDirWithAssemblies()
        {
            var value = string.Join(Path.DirectorySeparatorChar, new[]
                {
                    "bin",
                    Configuration,
                    "Publish",
                    GetPublishFileName(IsDebug, RuntimeIdentifier),
                    "assemblies",
                });
            return value;
        }

        public string GetPublishDir()
        {
            var value = string.Join(Path.DirectorySeparatorChar, new[]
                {
                    "bin",
                    Configuration,
                    "Publish",
                    GetPublishFileName(IsDebug, RuntimeIdentifier),
                });
            return value;
        }

        public string PublishDir
        {
            get
            {
                _PublishDir ??= /*RuntimeIdentifier.StartsWith("linux") ? GetPublishDir() :*/ GetPublishDirWithAssemblies();
                return _PublishDir;
            }

            set
            {
                _PublishDir = value;
            }
        }
    }

    const string publish_apphost_winany_arg =
"""
publish -c {0} -p:OutputType={1} -p:PublishDir=bin\{0}\Publish\win-any -p:PublishReferencesDocumentationFiles=false  -p:PublishDocumentationFile=false -p:PublishDocumentationFiles=false -f {2} -p:DebugType=none -p:DebugSymbols=false --nologo -v q /property:WarningLevel=1
""";

    static void PublishAppHost(string publishDir, Platform platform, bool debug)
    {
        const string appconfigFileName = "Steam++.exe.config";

        var rootPublishDir = Path.Combine(publishDir, "..");
        //var cacheFilePath = Path.Combine(ProjectUtils.ProjPath,
        //    "res", "windows", "Steam++.apphost");
        //// 使用缓存文件
        //if (File.Exists(cacheFilePath))
        //{
        //    File.Copy(cacheFilePath, Path.Combine(rootPublishDir, "Steam++.exe"));
        //    var sourceFileName = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.AppHost", "App.config");
        //    var appconfigContent = File.ReadAllText(sourceFileName);

        //    var xmlDoc = new XmlDocument();
        //    xmlDoc.LoadXml(appconfigContent);
        //    appconfigContent = xmlDoc.InnerXml;
        //    File.WriteAllText(Path.Combine(rootPublishDir, appconfigFileName), appconfigContent);
        //    return;
        //}

        const string app_host_tfm = "net35";
        var configuration = PublishCommandArg.GetConfiguration(debug);
        string? arguments = null;
        bool isWindows = false;
        switch (platform)
        {
            case Platform.Windows:
            case Platform.UWP:
            case Platform.WinUI:
                isWindows = true;
                arguments = publish_apphost_winany_arg.Format(
                    configuration,
                    debug ? "Exe" : "WinExe",
                    app_host_tfm);
                break;
        }
        var projRootPath = ProjectPath_AppHost;
        CleanProjDir(projRootPath);
        StartProcessAndWaitForExit(projRootPath,
            arguments ?? // 多次相同的编译产生的文件不会变化
            throw new ArgumentOutOfRangeException(nameof(platform), platform, null));

        if (isWindows)
        {
            var appHostPublishDir = Path.Combine(projRootPath, "bin", configuration, "Publish", "win-any");
            var apphostfilenames = new[]
            {
               "Steam++.exe",
               appconfigFileName,
            };

            ObfuscarHelper.Start(appHostPublishDir);

            foreach (var item in apphostfilenames)
            {
                var sourceFileName = Path.Combine(appHostPublishDir, item);
                var destFileName = Path.Combine(rootPublishDir, item);
                if (item == appconfigFileName)
                {
                    var appconfigContent = File.ReadAllText(sourceFileName);
                    if (app_host_tfm.StartsWith("net4"))
                    {
                        // net4x 不能兼容 2.x~3.x
                        appconfigContent = appconfigContent.Replace(
"""
<supportedRuntime version="v2.0.50727" />
""", null);

                        if (app_host_tfm == "net40")
                        {
                            appconfigContent = appconfigContent.Replace(
"""
<supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0" />
""", null);
                        }
                    }
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(appconfigContent);
                    appconfigContent = xmlDoc.InnerXml;
                    File.WriteAllText(destFileName, appconfigContent);
                }
                else
                {
                    File.Copy(sourceFileName, destFileName, true);
                }
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

        if (!arg.Framework.StartsWith("osx"))
        {
            // PublishDir is used by the CLI to denote the Publish target.
            argumentList.Add($@"-p:PublishDir={arg.PublishDir}");
        }
        else
        {
            argumentList.Add("-o");
            argumentList.Add(arg.PublishDir);
        }

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

    static IEnumerable<string> GetPluginNames(Platform platform)
    {
        yield return AssemblyInfo.Accelerator;
        yield return AssemblyInfo.GameAccount;
        yield return AssemblyInfo.GameList;
        yield return AssemblyInfo.Authenticator;
        yield return AssemblyInfo.SteamIdleCard;
        if (platform == Platform.Windows)
        {
            yield return AssemblyInfo.ArchiSteamFarmPlus;
            yield return AssemblyInfo.GameTools;
        }
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
        foreach (var pluginName in GetPluginNames(platform))
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
                    PublishAcceleratorReverseProxy(pluginName, pluginDir, isDebug, platform, architecture);
                    break;
            }

            static void PublishAcceleratorReverseProxy(
                string pluginName,
                string destinationDir,
                bool isDebug,
                Platform platform,
                Architecture architecture)
            {
                var isWinArm64 = platform == Platform.Windows && architecture == Architecture.Arm64;

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
                arg.UseAppHost = true;
                arg.SingleFile = true;
                arg.ReadyToRun = false;
                arg.Trimmed = false;
                arg.SelfContained = false;
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
                            arg.UseAppHost = true;
                            arg.SingleFile = true;
                            arg.SelfContained = false;
                            arg.RuntimeIdentifier = $"linux-{ArchToString(architecture)}";
                            break;
                        case Platform.Apple:
                            arg.RuntimeIdentifier = $"osx-{ArchToString(architecture)}";
                            break;
                    }
                }
                var projRootPath = Path.Combine(ProjectUtils.ProjPath, "src", "BD.WTTS.Client.Plugins.Accelerator.ReverseProxy");

                if (isWinArm64)
                {
                    arg.SingleFile = true;
                    arg.SelfContained = true;
                }

                arg.PublishDir = arg.GetPublishDir();

                CleanProjDir(projRootPath);
                var psi = GetProcessStartInfo(projRootPath);
                SetPublishCommandArgumentList(psi.ArgumentList, arg);
                //if (!isWindows)
                //{
                //    psi.ArgumentList.Add("-p:\"DefineConstants=NOT_WINDOWS;$(DefineConstants)\"");
                //}

                var argument = string.Join(' ', psi.ArgumentList);
                Console.WriteLine(argument);

                ProcessHelper.StartAndWaitForExit(psi);

                var publishDir = Path.Combine(projRootPath, arg.PublishDir);

                var aspnetcorev2_inprocess = Path.Combine(publishDir, "aspnetcorev2_inprocess.dll");
                IOPath.FileTryDelete(aspnetcorev2_inprocess);
                if (isWindows)
                {
                    var e_sqlite3 = Path.Combine(publishDir, "e_sqlite3.dll");
                    IOPath.FileTryDelete(e_sqlite3);
                    CopyDirectory(publishDir, destinationDir, true);
                }
                else
                {
                    var startName = $"Steam++.{pluginName}";
                    File.Copy(Path.Combine(publishDir, startName), Path.Combine(destinationDir, startName));
                }
            }
        }
    }
}
