using BD.WTTS.Client.Tools.Publish.Helpers;
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

    static string GetPublishFileName(bool debug, string rid, string fileEx = "", bool aot = false)
    {
        var value = $"[{(debug ? "Debug" : "Release")}] {EntryPointAssemblyName}{(aot ? "(Native AOT)" : null)}_v{AssemblyInfo.InformationalVersion}_{rid.Replace('-', '_')}_{releaseTimestamp}{fileEx}";
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
        var debug = CommandCompat.GetOption<bool>("--debug", "Defines the build configuration");
        var rids = CommandCompat.GetOption<string[]>("--rids", "RID is short for runtime identifier");
        var force_sign = CommandCompat.GetOption<bool>("--force-sign", GetDefForceSign, "Mandatory verification must be digitally signed");
        var hsm_sign = CommandCompat.GetOption<bool>("--hsm-sign", "");
        var sha256 = CommandCompat.GetOption<bool>("--sha256", () => true, "Calculate file hash value");
        var sha384 = CommandCompat.GetOption<bool>("--sha384", () => true, "Calculate file hash value");
        var stm_upload = CommandCompat.GetOption<bool>("--stm-upload", "Create a Steam upload ZIP for Windows or Linux Native AOT");
        var aot = CommandCompat.GetOption<bool>("--aot", () => true, "Build Native AOT packages alongside JIT packages");
        var command = new Command(commandName, "DotNet publish app")
        {
           debug, rids, force_sign, sha256, sha384, stm_upload, hsm_sign, aot,
        };
        command.SetHandler(Handler, debug, rids, force_sign, sha256, sha384, stm_upload, hsm_sign, aot);
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


    internal static void Handler(bool debug, string[] rids, bool force_sign, bool sha256, bool sha384, bool stm_upload, bool hsm_sign, bool aot)
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

                var projRootPath = ProjectPath_AvaloniaApp;
                var arg = SetPublishCommandArgumentList(debug, info.Platform, info.DeviceIdiom, info.Architecture);
                if (info.Platform == Platform.Apple)
                {
                    var directory = Path.Combine(projRootPath, arg.GetPublishDir());
                    PublishMacJit(arg, info.Architecture, directory);
                    WriteMacPublishManifest(directory, rid, DeploymentMode.SCD, sha256, sha384);
                    if (aot)
                    {
                        var nativeDirectory = Path.Combine(projRootPath, arg.GetPublishDir(aot: true));
                        NativeAotPublisher.RunPublishScript(rid, nativeDirectory);
                        WriteMacPublishManifest(nativeDirectory, rid, DeploymentMode.AOT, sha256, sha384);
                    }
                    continue;
                }

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

                var psi = GetProcessStartInfo(projRootPath);
                if (isWinArm64) // win-arm64
                {
                    // steamclient.dll 缺少 Arm64，以及 Arm64EC 在 dotnet 中目前不可用
                    // 更改为 x64 兼容运行主进程
                    arg.RuntimeIdentifier = "win-x64";
                    arg.PublishDir = arg.PublishDir.Replace("win-x64", "win-arm64");
                }
                SetPublishCommandArgumentList(psi.ArgumentList, arg);
                psi.ArgumentList.Add($"-p:WttsDesktopTargetFrameworks={arg.Framework}");

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.Write("[");
                Console.Write(rid);
                Console.Write("] dotnet ");
                Console.WriteLine(string.Join(' ', psi.ArgumentList));
                ResetConsoleColor();

                var publishDir = Path.Combine(projRootPath, arg.PublishDir);
                Console.WriteLine(publishDir);
                var rootPublishDir = Path.GetFullPath(Path.Combine(publishDir, ".."));
                var rootPublishDir_Aot = aot ? Path.Combine(projRootPath, arg.GetPublishDir(aot: true)) : null!;
                //switch (info.Platform)
                //{
                //    case Platform.Linux:
                //        rootPublishDir = Path.GetFullPath(publishDir);
                //        break;
                //}
                DirTryDelete(rootPublishDir);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.Write(ToolStrings.Get("DirectoryDeleted"));
                Console.WriteLine(rootPublishDir);
                ResetConsoleColor();

                // 发布主体
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("PublishingMain"));
                ResetConsoleColor();
                ProcessHelper.StartAndWaitForExit(psi);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("PublishedMain"));
                ResetConsoleColor();

                // 验证 Avalonia.Base.dll 版本号必须为 11+
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("ValidatingAvalonia"));
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
                    Console.WriteLine(ToolStrings.Get("LinuxIcon"));
                    var ico_path = Path.Combine(ProjectUtils.ProjPath, "res", "icons", "app", "v3", "Logo_512.png");
                    var save_dir_path = Path.Combine(rootPublishDir, "Icons");
                    IOPath.DirCreateByNotExists(save_dir_path);
                    // 不能使用下划线
                    File.Copy(ico_path, Path.Combine(save_dir_path, "Watt-Toolkit.png"), true);
                    Console.WriteLine(ToolStrings.Get("LinuxScripts"));
                    var legacyScripts = Path.Combine(rootPublishDir, "..", "ShellScript", "Linux");
                    var localizedScripts = Path.Combine(ProjectUtils.ProjPath, "build", "linux");
                    var destinationScripts = Path.Combine(rootPublishDir, "script");
                    if (Directory.Exists(legacyScripts))
                        CopyDirectory(legacyScripts, destinationScripts, true);
                    CopyDirectory(localizedScripts, destinationScripts, true);
                    if (OperatingSystem.IsLinux())
                    {
                        const UnixFileMode executable = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
                        foreach (var script in Directory.EnumerateFiles(destinationScripts)
                            .Where(static path => Path.GetExtension(path) is ".sh" or ".desktop"))
                            File.SetUnixFileMode(script, File.GetUnixFileMode(script) | executable);
                    }
                    File.Move(Path.Combine(destinationScripts, "Steam++.sh"), Path.Combine(rootPublishDir, "Steam++.sh"));
                }

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("ValidatedAvalonia"));
                ResetConsoleColor();

                // 删除 CreateDump
                RemoveCreateDump(publishDir);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("RemovedCreateDump"));
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
                Console.WriteLine(ToolStrings.Get("MoveNativeLibraries"));
                ResetConsoleColor();

                // 处理 json 文件
                var runtimeconfigjsonpath = Path.Combine(publishDir, runtimeconfigjsonfilename);
                ILaunchAppTestCommand.HandlerJsonFiles(runtimeconfigjsonpath);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("ProcessedJson"));
                ResetConsoleColor();

                if (isWindows)
                {
                    // 发布 apphost
                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                    Console.WriteLine(ToolStrings.Get("PublishingAppHost"));
                    ResetConsoleColor();
                    PublishAppHost(publishDir, info.Platform, debug);
                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                    Console.WriteLine(ToolStrings.Get("PublishedAppHost"));
                    ResetConsoleColor();

                }

                // 发布插件
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("PublishingPlugins"));
                ResetConsoleColor();
                PublishPlugins(debug, info.Platform, info.Architecture, Path.Combine(rootPublishDir, "modules"), arg.Configuration, arg.Framework);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("PublishedPlugins"));
                ResetConsoleColor();

                // 复制运行时
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("CopyingRuntime"));
                ResetConsoleColor();
                CopyRuntime(rootPublishDir, info.Platform, isCopyRuntime, info.Architecture);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("CopiedRuntime"));
                ResetConsoleColor();

                // 发布 AOT 版本
                if (rootPublishDir_Aot != null)
                    NativeAotPublisher.RunPublishScript(rid, rootPublishDir_Aot);

                var appPublish = new AppPublishInfo()
                {
                    DeploymentMode = DeploymentMode.SCD,
                    RuntimeIdentifier = arg.RuntimeIdentifier,
                    DirectoryPath = rootPublishDir,
                };

                var appPublish_Aot = rootPublishDir_Aot != null ? new AppPublishInfo()
                {
                    DeploymentMode = DeploymentMode.AOT,
                    RuntimeIdentifier = arg.RuntimeIdentifier,
                    DirectoryPath = rootPublishDir_Aot,
                } : null!;

                IOPath.DirTryDelete(Path.Combine(rootPublishDir, IOPath.DirName_AppData));
                IOPath.DirTryDelete(Path.Combine(rootPublishDir, IOPath.DirName_Cache));
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("RemovedCache"));
                ResetConsoleColor();

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("ScanningFiles"));
                ResetConsoleColor();
                IScanPublicDirectoryCommand.ScanPathCore(appPublish.DirectoryPath,
                    appPublish.Files,
                    ignoreRootDirNames: ignoreDirNames);
                if (appPublish_Aot != null)
                {
                    IScanPublicDirectoryCommand.ScanPathCore(appPublish_Aot.DirectoryPath,
                        appPublish_Aot.Files,
                        ignoreRootDirNames: ignoreDirNames);
                }
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("ScannedFiles"));
                ResetConsoleColor();

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("HashingFiles"));
                ResetConsoleColor();
                HashFiles(appPublish, sha256, sha384);
                if (appPublish_Aot != null)
                    HashFiles(appPublish_Aot, sha256, sha384);
                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.WriteLine(ToolStrings.Get("HashedFiles"));
                ResetConsoleColor();

                if (OperatingSystem.IsWindows() && isWindows)
                {
                    // 数字签名
                    List<AppPublishFileInfo> toBeSignedFiles = new();
                    HashSet<string> toBeSignedFilePaths = new();

                    List<AppPublishFileInfo> appPublishAllFiles = new();
                    appPublishAllFiles.AddRange(appPublish.Files);
                    if (appPublish_Aot != null)
                    {
                        appPublishAllFiles.AddRange(appPublish_Aot.Files);
                    }

                    foreach (var item in appPublishAllFiles)
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
                        Console.Write(ToolStrings.Get("SigningCount"));
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
                                //Console.WriteLine("数字签名失败，输入回车使用自签继续");
                                //Console.ReadLine();
                                //if (debug)
                                //    throw;
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
                    Console.WriteLine(ToolStrings.Get("GeneratingMsix"));
                    ResetConsoleColor();

                    void PublishMSIX(AppPublishInfo appPublish, AppPublishFileInfo msixInfo, string rootPublishDir, out string msixFilePath)
                    {
                        // 生成清单文件
                        MSIXHelper.MakeAppx.GenerateAppxManifestXml(rootPublishDir, AppVersion4, info.Architecture);

                        // 打包资源 images
                        MSIXHelper.MakePri.Start(rootPublishDir);

                        var msixDir = $"{rootPublishDir}_MSIX";
                        IOPath.DirCreateByNotExists(msixDir);
                        msixFilePath = Path.Combine(msixDir, GetPublishFileName(debug, rid, ".msix"));

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

                        msixInfo.FilePath = msixBundleFilePath;
                        msixInfo.Length = msixFileStream.Length;
                        msixInfo.SignatureSHA384 = Hashs.String.SHA384(msixFileStream);
                        appPublish.SingleFile.Add(CloudFileType.MsixBundle, msixInfo);
                    }

                    var msixInfo = new AppPublishFileInfo
                    {
                        FileEx = ".msixbundle",
                    };
                    AppPublishFileInfo msixInfo_Aot = null!;
                    PublishMSIX(appPublish, msixInfo, rootPublishDir, out var msixFilePath);
                    if (aot && rootPublishDir_Aot != null && appPublish_Aot != null)
                    {
                        msixInfo_Aot = new AppPublishFileInfo
                        {
                            FileEx = msixInfo.FileEx,
                        };
                        PublishMSIX(appPublish_Aot, msixInfo_Aot, rootPublishDir_Aot, out _);
                    }

                    SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                    Console.Write(ToolStrings.Get("GeneratedMsixSize"));
                    Console.Write(IOPath.GetDisplayFileSizeString(msixInfo.Length));
                    Console.Write(ToolStrings.Get("PathSuffix"));
                    Console.WriteLine(msixFilePath);
                    ResetConsoleColor();
                }

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkMagenta);
                Console.WriteLine(ToolStrings.Get("CreatingArchive"));
                ResetConsoleColor();
                string? packPath = null;
                string GetPackPathWithTryDelete(string fileEx, bool aot = false)
                {
                    var packPath = $"{(aot ? rootPublishDir_Aot : rootPublishDir)}{fileEx}";
                    IOPath.FileTryDelete(packPath);
                    return packPath;
                }
                switch (info.Platform)
                {
                    case Platform.Windows:
                    case Platform.UWP:
                    case Platform.WinUI:
                        {
                            ICompressedPackageCommand.CreateSevenZipPack(packPath = GetPackPathWithTryDelete(FileEx._7Z), appPublish.Files);
                            if (appPublish_Aot != null)
                            {
                                ICompressedPackageCommand.CreateSevenZipPack(packPath = GetPackPathWithTryDelete(FileEx._7Z, aot: true), appPublish_Aot.Files);
                            }
                            break;
                        }
                    case Platform.Linux:
                        {
                            ICompressedPackageCommand.CreateGZipPack(packPath = GetPackPathWithTryDelete(FileEx.TAR_GZ), appPublish.Files);
                            if (appPublish_Aot != null)
                            {
                                ICompressedPackageCommand.CreateGZipPack(packPath = GetPackPathWithTryDelete(FileEx.TAR_GZ, aot: true), appPublish_Aot.Files);
                            }
                        }
                        break;
                }

                // A. old jit version dlc
                //if (stm_upload)
                //{
                //    List<AppPublishFileInfo> mainFiles = new();
                //    List<AppPublishFileInfo> modulesFiles = new();
                //    List<AppPublishFileInfo> stmUploads = new();
                //    foreach (var item in appPublish.Files)
                //    {
                //        if (item.RelativePath.Contains("modules"))
                //        {
                //            modulesFiles.Add(item);
                //        }
                //        else
                //        {
                //            mainFiles.Add(item);
                //        }
                //    }
                //    var stmupload_main_zip_path = GetPackPathWithTryDelete("_Main.stmupload.zip");
                //    stmUploads.Add(new()
                //    {
                //        FilePath = stmupload_main_zip_path,
                //        RelativePath = Path.GetFileName(stmupload_main_zip_path),
                //    });
                //    ICompressedPackageCommand.CreateZipPack(stmupload_main_zip_path, mainFiles);

                //    var query = from module in modulesFiles
                //                let split = module.RelativePath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries)
                //                let name = split.Length >= 2 ? split[1] : null
                //                where name != null
                //                group module by name;
                //    var items = query.ToArray();
                //    foreach (var item in items)
                //    {
                //        if (string.IsNullOrWhiteSpace(item.Key))
                //            continue;
                //        var files = item.ToArray();
                //        var stmupload_item_zip_path = GetPackPathWithTryDelete($"_{item.Key}.stmupload.zip");
                //        stmUploads.Add(new()
                //        {
                //            FilePath = stmupload_item_zip_path,
                //            RelativePath = Path.GetFileName(stmupload_item_zip_path),
                //        });
                //        ICompressedPackageCommand.CreateZipPack(stmupload_item_zip_path, files);
                //    }
                //    ICompressedPackageCommand.CreateZipPack(GetPackPathWithTryDelete(".stmupload.zip"), stmUploads);
                //}

                // B. aot main only
                if (stm_upload && aot && appPublish_Aot != null)
                {
                    List<AppPublishFileInfo> mainFiles = appPublish_Aot.Files;
                    List<AppPublishFileInfo> stmUploads = new();
                    var stmupload_main_zip_path = GetPackPathWithTryDelete("_Main.stmupload.zip", aot: true);
                    stmUploads.Add(new()
                    {
                        FilePath = stmupload_main_zip_path,
                        RelativePath = Path.GetFileName(stmupload_main_zip_path),
                    });
                    ICompressedPackageCommand.CreateZipPack(stmupload_main_zip_path, mainFiles);
                }

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.Write(ToolStrings.Get("CreatedArchiveSize"));
                if (packPath != null)
                {
                    Console.Write(IOPath.GetDisplayFileSizeString(new FileInfo(packPath).Length));
                }
                else
                {
                    Console.Write(0);
                }
                Console.Write(ToolStrings.Get("PathSuffix"));
                Console.WriteLine(packPath);
                ResetConsoleColor();

                var jsonFilePath = WritePublishManifest(appPublish);
                if (appPublish_Aot != null)
                    WritePublishManifest(appPublish_Aot);

                SetConsoleColor(ConsoleColor.White, ConsoleColor.DarkGreen);
                Console.Write(ToolStrings.Get("GeneratedManifestSize"));
                Console.Write(IOPath.GetDisplayFileSizeString(appPublish.Files.Sum(x => x.Length)));
                Console.Write(ToolStrings.Get("PathSuffix"));
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

    static void PublishMacJit(PublishCommandArg arg, Architecture architecture, string directory)
    {
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException("macOS DMG publishing requires macOS.");

        var parent = Path.GetDirectoryName(directory)!;
        var work = Directory.CreateDirectory(Path.Combine(parent, $".macos-jit-{Guid.NewGuid():N}")).FullName;
        try
        {
            var payload = Directory.CreateDirectory(Path.Combine(work, "payload")).FullName;
            var app = Path.Combine(payload, "Steam++.app");
            var managedDirectory = Path.Combine(app, "Contents", "MonoBundle");
            var modules = Path.Combine(managedDirectory, "modules");
            arg.PublishDir = Path.Combine(work, "publish");
            arg.CreatePackage = false;
            var publish = GetProcessStartInfo(ProjectPath_AvaloniaApp);
            SetPublishCommandArgumentList(publish.ArgumentList, arg);
            publish.ArgumentList.Add($"-p:WttsDesktopTargetFrameworks={arg.Framework}");
            publish.ArgumentList.Add($"-p:AppBundleDir={app}");
            publish.ArgumentList.Add("-p:CopyDSYMToPublishDirectory=false");
            ProcessHelper.StartAndWaitForExit(publish);

            var dtls = Path.Combine(managedDirectory, "libdtls_native.dylib");
            if (architecture == Architecture.X64 && !File.Exists(dtls))
            {
                var intelLibrary = Path.Combine(ProjectUtils.ProjPath, "build", "artifacts",
                    "native-dependencies", "dtls", "osx-x64", "native", "libdtls_native.dylib");
                if (!File.Exists(intelLibrary))
                    throw new FileNotFoundException("Run build/native-dtls/build-macos-x64.sh before publishing the Intel macOS DMG.", intelLibrary);
                NativeAotPublisher.Run("lipo", work, "-verify_arch", "x86_64", intelLibrary);
                File.Copy(intelLibrary, dtls);
                NativeAotPublisher.Run("codesign", work, "--force", "--sign", "-", dtls);
            }

            PublishPlugins(arg.IsDebug, Platform.Apple, architecture, modules, arg.Configuration, arg.Framework);
            var accelerator = Path.Combine(modules, "Accelerator");
            foreach (var library in Directory.EnumerateFiles(accelerator, "*.dylib", SearchOption.AllDirectories))
                NativeAotPublisher.Run("codesign", work, "--force", "--sign", "-", library);
            NativeAotPublisher.Run("codesign", work, "--force", "--sign", "-", Path.Combine(accelerator, "Steam++.Accelerator"));
            NativeAotPublisher.Run("codesign", work, "--force", "--sign", "-", "--preserve-metadata=entitlements", app);

            var packaging = Path.Combine(ProjectUtils.ProjPath, "build", "macos");
            var stagedImage = Path.Combine(work, "package.dmg");
            NativeAotPublisher.Run("/bin/bash", work, Path.Combine(packaging, "create-dmg.sh"),
                Path.Combine(packaging, "installer-template.dmg"), app, stagedImage);
            Directory.Move(payload, directory);
            try
            {
                File.Move(stagedImage, directory + ".dmg");
            }
            catch
            {
                Directory.Move(directory, payload);
                throw;
            }
            Console.WriteLine($"macOS disk image: {directory}.dmg");
        }
        finally
        {
            try
            {
                Directory.Delete(work, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"Could not remove macOS packaging directory {work}: {ex.Message}");
            }
        }
    }

    static void WriteMacPublishManifest(string directory, string rid, DeploymentMode mode, bool sha256, bool sha384)
    {
        var appPublish = new AppPublishInfo
        {
            DeploymentMode = mode,
            RuntimeIdentifier = rid,
            DirectoryPath = directory,
        };
        IScanPublicDirectoryCommand.ScanPathCore(directory, appPublish.Files, ignoreRootDirNames: ignoreDirNames);
        HashFiles(appPublish, sha256, sha384);
        WritePublishManifest(appPublish);
    }

    static void HashFiles(AppPublishInfo publish, bool sha256, bool sha384)
    {
        foreach (var item in publish.Files)
        {
            if (sha256)
            {
                using var file = File.OpenRead(item.FilePath);
                item.SHA256 = Hashs.String.SHA256(file);
            }
            if (sha384)
            {
                using var file = File.OpenRead(item.FilePath);
                item.SHA384 = Hashs.String.SHA384(file);
            }
        }
    }

    static string WritePublishManifest(AppPublishInfo publish)
    {
        var path = $"{publish.DirectoryPath}.json";
        using var stream = File.Open(path, FileMode.OpenOrCreate);
        JsonSerializer.Serialize(stream, publish, new AppPublishInfoContext(new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        }).AppPublishInfo);
        stream.Flush();
        stream.SetLength(stream.Position);
        return path;
    }

    private static readonly Lazy<string> _AppVersion4 = new(() =>
    {
        var v = new Version(AssemblyInfo.FileVersion);
        static int GetInt32(int value) => value < 0 ? 0 : value;
        return $"{GetInt32(v.Major)}.{GetInt32(v.Minor)}.{GetInt32(v.Build)}.{GetInt32(v.Revision)}";
    });

    static string AppVersion4 => _AppVersion4.Value;

    /// <summary>
    /// 按生成的 runtimeconfig 依赖闭包复制共享运行时，不裁剪框架内部程序集。
    /// </summary>
    static void CopyRuntime(string rootPublishDir, Platform platform, bool isCopyRuntime, Architecture architecture)
    {
        if (isCopyRuntime)
            BundledRuntimeHelper.Copy(rootPublishDir, platform, architecture);
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
            if (libFileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var pdbFileName = $"{libFileName[..^4]}.pdb";
                MoveNativeLibrary(pdbFileName);
            }
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
                        arg.ReadyToRun = !isDebug;
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

        public string GetPublishDirWithAssemblies(bool aot = false)
        {
            var value = string.Join(Path.DirectorySeparatorChar, new[]
                {
                    "bin",
                    Configuration,
                    "Publish",
                    GetPublishFileName(IsDebug, RuntimeIdentifier,aot:aot),
                    "assemblies",
                });
            return value;
        }

        public string GetPublishDir(bool aot = false)
        {
            var value = string.Join(Path.DirectorySeparatorChar, new[]
                {
                    "bin",
                    Configuration,
                    "Publish",
                    GetPublishFileName(IsDebug, RuntimeIdentifier,aot:aot),
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
    /// <param name="modulesDirectory"></param>
    /// <param name="configuration"></param>
    /// <param name="framework"></param>
    /// <exception cref="FileNotFoundException"></exception>
    static void PublishPlugins(
                bool isDebug,
                Platform platform,
                Architecture architecture,
                string modulesDirectory,
                string configuration,
                string framework)
    {
        foreach (var pluginName in GetPluginNames(platform))
        {
            var projRootPath = Path.Combine(ProjectUtils.ProjPath, "src", $"BD.WTTS.Client.Plugins.{pluginName}");
            StartProcessAndWaitForExit(projRootPath,
                $"build -c {configuration} -f {framework} -p:WttsDesktopTargetFrameworks={framework} --nologo -v q /property:WarningLevel=1");

            var dllFileName = $"BD.WTTS.Client.Plugins.{pluginName}.dll";
            var pluginBuildDir = Path.Combine(projRootPath, "bin", configuration);
            var dllPath = Path.Combine(pluginBuildDir, framework, dllFileName);
            if (!File.Exists(dllPath))
                throw new FileNotFoundException(null, dllPath);
            var pluginDir = Path.Combine(modulesDirectory, pluginName);
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
                bool isMacOS = false;
                switch (platform)
                {
                    case Platform.Windows:
                    case Platform.UWP:
                    case Platform.WinUI:
                        isWindows = true;
                        break;
                    case Platform.Apple:
                        isMacOS = true;
                        break;
                }

                arg.Framework = $"net{Environment.Version.Major}.{Environment.Version.Minor}";
                arg.ReadyToRun = false;
                arg.Trimmed = false;
                if (isMacOS)
                {
                    arg.SelfContained = true;
                }
                else
                {
                    arg.SelfContained = false;
                }
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
                    arg.SelfContained = true;
                }

                arg.PublishDir = arg.GetPublishDir();

                CleanProjDir(projRootPath);
                var psi = GetProcessStartInfo(projRootPath);
                SetPublishCommandArgumentList(psi.ArgumentList, arg);
                psi.ArgumentList.Add("-p:WttsPublishReverseProxySingleFile=true");
                //if (!isWindows)
                //{
                //    psi.ArgumentList.Add("-p:\"DefineConstants=NOT_WINDOWS;$(DefineConstants)\"");
                //}

                var argument = string.Join(' ', psi.ArgumentList);
                Console.WriteLine(argument);

                ProcessHelper.StartAndWaitForExit(psi);

                var publishDir = Path.Combine(projRootPath, arg.PublishDir);

                // Single-file embeds this file; retain the generated declaration for shared-runtime packaging.
                if (arg.SelfContained == false)
                {
                    var runtimeConfigName = $"Steam++.{pluginName}.runtimeconfig.json";
                    var runtimeConfigPath = Path.Combine(projRootPath, "bin", arg.Configuration,
                        arg.Framework, arg.RuntimeIdentifier, runtimeConfigName);
                    File.Copy(runtimeConfigPath, Path.Combine(destinationDir, runtimeConfigName), true);
                }

                var aspnetcorev2_inprocess = Path.Combine(publishDir, "aspnetcorev2_inprocess.dll");
                IOPath.FileTryDelete(aspnetcorev2_inprocess);
                if (isWindows)
                {
                    var e_sqlite3 = Path.Combine(publishDir, "e_sqlite3.dll");
                    IOPath.FileTryDelete(e_sqlite3);
                    CopyDirectory(publishDir, destinationDir, true);
                }
                else if (isMacOS)
                {
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
