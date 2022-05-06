using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip.Compression;
using SevenZip;
using System.Application.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Properties;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
#if !SERVER
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Packaging.Targets;
using Packaging.Targets.IO;
using ZstdNet;
#endif
using NCompressionMode = System.IO.Compression.CompressionMode;

namespace System.Application
{
    internal static partial class Utils
    {
        static Utils()
        {
            if (OperatingSystem.IsWindows())
            {
                //var sevenZipLibraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.dll");
                var sevenZipLibraryPath = Path.Combine(AppContext.BaseDirectory, "7z.dll");
                SevenZipBase.SetLibraryPath(sevenZipLibraryPath);
                SevenZipCompressor.LzmaDictionarySize = 805306368; // 768MB

                var zstdLibraryPath = Path.Combine(AppContext.BaseDirectory, "libzstd.dll");
                if (File.Exists(zstdLibraryPath))
                {
                    var zstdLibraryPathDest = Path.Combine(AppContext.BaseDirectory, "x64", "libzstd.dll");
                    if (File.Exists(zstdLibraryPathDest)) File.Delete(zstdLibraryPathDest);
                    File.Move(zstdLibraryPath, zstdLibraryPathDest);
                }
            }
        }

        //public static void AddDeploymentModeOption(Command command, string alias = "-d")
        //{
        //    var o = new Option<DeploymentMode>(alias, () => DeploymentMode.SCD, DeploymentModeDesc);
        //    command.AddOption(o);
        //}

        public const string InputPubDirNameDesc = "(必填)输入发布文件夹名";
        public const string InputPubDirNameError = "错误：必须输入发布文件夹名";
        public const string DevDesc = "是否为开发环境，默认值否";
        //const string DeploymentModeDesc = "应用部署模式，默认为SCD";
        //public const string DevTablePrefix = "";
        public const string PublishJsonFileName = "Publish.json";

        static readonly Lazy<string> mPublishJsonFilePath = new(() => Path.Combine(IOPath.AppDataDirectory, PublishJsonFileName));

        public static string PublishJsonFilePath => mPublishJsonFilePath.Value;

        public static readonly string[] ignoreDirNames = new[]
        {
            IOPath.DirName_AppData,
            IOPath.DirName_Cache,
            "CEF"
        };

        /// <summary>
        /// 检查版本号字符串是否符合要求
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CheckVersion(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("错误：必须输入一个版本号！");
                return false;
            }
            else if (!System.Version.TryParse(value, out var _))
            {
                Console.WriteLine("错误：输入的版本号格式不正确！");
                return false;
            }
            return true;
        }

        static readonly Lazy<bool> mIsAigioPC = new(() =>
        {
            return Hashs.String.Crc32(Environment.MachineName, false) == "88DF9AB0" ||
            Hashs.String.Crc32(Environment.UserName, false) == "8AA383BC";
        });

        public static bool IsAigioPC => mIsAigioPC.Value;

        public static string GetInfoVersion(string assemblyFile)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyFile);
            return fileVersionInfo?.FileVersion ?? string.Empty;
            //var a = Assembly.LoadFrom(assemblyFile);
            //var attr = a.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            //return attr?.InformationalVersion ?? string.Empty;
        }

        public static void ScanPath(string dirPath, List<PublishFileInfo>? list = null, string? relativeTo = null, string[]? ignoreRootDirNames = null)
        {
            list ??= new List<PublishFileInfo>();
            relativeTo ??= dirPath;
            var files = Directory.GetFiles(dirPath).Where(x =>
                Path.GetExtension(x) != ".pdb" &&
                (Path.GetExtension(x) != ".xml" || x.EndsWith(".VisualElementsManifest.xml", StringComparison.OrdinalIgnoreCase)) &&
                !x.EndsWith(".runtimeconfig.dev.json", StringComparison.OrdinalIgnoreCase));
            if (files.Any())
            {
                var files2 = files.Select(x => new PublishFileInfo(x, relativeTo));
                list.AddRange(files2);
            }
            var dirs = Directory.GetDirectories(dirPath);
            foreach (var dir in dirs)
            {
                if (ignoreRootDirNames != null && ignoreRootDirNames.Contains(Path.GetDirectoryName(dir)))
                {
                    // 忽略顶级文件夹
                    continue;
                }
                ScanPath(dir, list, relativeTo);
            }
        }

        public static void CreatePack(string packPath, IEnumerable<PublishFileInfo> files)
        {
            using var fs = File.Create(packPath);
            using var s = new GZipOutputStream(fs);
            s.SetLevel(Deflater.BEST_COMPRESSION);
            using var archive = TarArchive.CreateOutputTarArchive(s,
                TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
            foreach (var file in files)
            {
                Console.WriteLine($"正在压缩：{file.Path}");
                var entry = TarEntry.CreateEntryFromFile(file.Path);
                entry.Name = file.RelativePath;
                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                archive.WriteEntry(entry, false);
            }
        }

#if !SERVER
        sealed class XZOutputStream2 : XZOutputStream
        {
            public XZOutputStream2(Stream s) : base(s)
            {
            }

            public XZOutputStream2(Stream s, int threads) : base(s, threads)
            {
            }

            public XZOutputStream2(Stream s, int threads, uint preset) : base(s, threads, preset)
            {
            }

            public XZOutputStream2(Stream s, int threads, uint preset, bool leaveOpen) : base(s, threads, preset, leaveOpen)
            {
            }

            public override void Flush()
            {
            }
        }

        public static void CreateXZPack(string packPath, IEnumerable<PublishFileInfo> files)
        {
            using var fs = File.Create(packPath);
            using var s = new XZOutputStream2(fs);
            using var archive = TarArchive.CreateOutputTarArchive(s,
                TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
            foreach (var file in files)
            {
                Console.WriteLine($"正在压缩：{file.Path}");
                var entry = TarEntry.CreateEntryFromFile(file.Path);
                entry.Name = file.RelativePath;
                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                archive.WriteEntry(entry, false);
            }
        }
#endif

        public static void CreateBrotliPack(string packPath, IEnumerable<PublishFileInfo> files)
        {
            using var fs = File.Create(packPath);
            using var s = new BrotliStream(fs, NCompressionMode.Compress);
            using var archive = TarArchive.CreateOutputTarArchive(s,
                TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
            foreach (var file in files)
            {
                Console.WriteLine($"正在压缩：{file.Path}");
                var entry = TarEntry.CreateEntryFromFile(file.Path);
                entry.Name = file.RelativePath;
                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                archive.WriteEntry(entry, false);
            }
        }

        public static void CreateSevenZipPack(string packPath, IEnumerable<PublishFileInfo> files)
        {
            SevenZipCompressor compressor = new();
            compressor.ArchiveFormat = OutArchiveFormat.SevenZip;
            compressor.CompressionLevel = SevenZip.CompressionLevel.Ultra;
            compressor.CompressionMethod = CompressionMethod.Lzma2;
            compressor.FastCompression = false;
            compressor.ScanOnlyWritable = true;
            compressor.DirectoryStructure = true;
            compressor.FileCompressionStarted += (_, e) =>
            {
                Console.WriteLine($"正在压缩：{e.FileName}");
            };
            var dict = files.ToDictionary(x => x.RelativePath, x => x.Path);
            compressor.CompressFileDictionary(dict, packPath);
        }

#if !SERVER
        public static void CreateZstdPack(string packPath, IEnumerable<PublishFileInfo> files)
        {
            using var fs = File.Create(packPath);
            var maxCompressionLevel = CompressionOptions.MaxCompressionLevel;
            Console.WriteLine($"MaxCompressionLevel: {maxCompressionLevel}");
            using var s = new CompressionStream(fs, new CompressionOptions(maxCompressionLevel));
            using var archive = TarArchive.CreateOutputTarArchive(s,
                TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
            foreach (var file in files)
            {
                Console.WriteLine($"正在压缩：{file.Path}");
                var entry = TarEntry.CreateEntryFromFile(file.Path);
                entry.Name = file.RelativePath;
                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                archive.WriteEntry(entry, false);
            }
        }
#endif

        public static void SavePublishJson(IEnumerable<PublishDirInfo> dirNames, bool removeFiles = false)
        {
            if (removeFiles)
            {
                foreach (var item in dirNames)
                {
                    item.Files.Clear();
                }
            }
            var publish_json_str = Serializable.SJSON(dirNames);
            IOPath.FileIfExistsItDelete(PublishJsonFilePath);
            File.WriteAllText(PublishJsonFilePath, publish_json_str);
        }

        static string? mVersion;

        public static bool IsUseCustomVersion => mVersion != null;

        public static string Version
        {
            [Obsolete("use GetVersion(bool)")]
            get
            {
                if (mVersion != null) return mVersion;
                var mainDllPath = ProjectPathUtil.projPath + ProjectPathUtil.MainDllPath;
                var version = (File.Exists(mainDllPath) ? FileVersionInfo.GetVersionInfo(mainDllPath).FileVersion : null) ?? ThisAssembly.Version;
                var versionArray = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
                switch (versionArray.Length)
                {
                    case 1:
                        version = $"{versionArray[0]}.0.0";
                        break;
                    case 2:
                        version = $"{versionArray[0]}.{versionArray[1]}.0";
                        break;
                    case 3:
                    case 4:
                        version = string.Join('.', versionArray);
                        break;
                }
                return version;
            }

            set
            {
                mVersion = value;
            }
        }

        public static string GetVersion(bool dev)
        {
            if (mVersion != null) return mVersion;
            var configuration = GetConfiguration(dev, isLower: false);
            var mainDllPath = ProjectPathUtil.projPath + string.Format(ProjectPathUtil.MainDllPath_, configuration);
            var version = (File.Exists(mainDllPath) ? FileVersionInfo.GetVersionInfo(mainDllPath).FileVersion : null) ?? ThisAssembly.Version;
            var versionArray = version.Split('.', StringSplitOptions.RemoveEmptyEntries);
            switch (versionArray.Length)
            {
                case 1:
                    version = $"{versionArray[0]}.0.0";
                    break;
                case 2:
                    version = $"{versionArray[0]}.{versionArray[1]}.0";
                    break;
                case 3:
                case 4:
                    version = string.Join('.', versionArray);
                    break;
            }
            return version;
        }

        public static string GetFileName(bool dev, PublishDirInfo item, string fileEx)
        {
            var name = item.Name.Replace("-", "_");
            if (name.Contains("osx_")) name = name.Replace("osx_", "macos_");
            var version = GetVersion(dev);
            var fileName = item.DeploymentMode switch
            {
                DeploymentMode.SCD =>
                    $"Watt Toolkit_{name}_{(IsUseCustomVersion ? "" : "v")}{version}{fileEx}",
                DeploymentMode.FDE =>
                    $"Watt Toolkit_{name}_fde_{(IsUseCustomVersion ? "" : "v")}{version}{fileEx}",
                _ => throw new ArgumentOutOfRangeException(nameof(item.DeploymentMode), item.DeploymentMode, null),
            };
            return fileName;
        }

        public static string GetPackPath(bool dev, PublishDirInfo item, string fileEx)
        {
            var fileName = GetFileName(dev, item, fileEx);
            var packPath = item.DeploymentMode switch
            {
                DeploymentMode.SCD => Path.Combine(item.Path, "..", fileName),
                DeploymentMode.FDE => Path.Combine(item.Path, "..", "..", fileName),
                _ => throw new ArgumentOutOfRangeException(nameof(item.DeploymentMode), item.DeploymentMode, null),
            };
            return packPath;
        }

        public static string[] GetAllRids(string[] rids, Platform platform) => !rids.Any()
                ? platform switch
                {
                    Platform.Unknown => new[] { "win-x64", "win-arm64", "linux-x64", "linux-arm64", "linux-arm", "osx-x64", "osx-arm64", },
                    Platform.Windows => new[] { "win-x64", "win-arm64", },
                    Platform.Linux => new[] { "linux-x64", "linux-arm64", "linux-arm", },
                    Platform.Apple => new[] { "osx-x64", "osx-arm64", },
                    _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null),
                }
                : rids;

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
            public const string RpmVendor = ThisAssembly.AssemblyCompany;
            public const string Description = ThisAssembly.AssemblyDescription;
            public const string Url = "https://steampp.net";
            public const string PreInstallScript = null!;
            public const string PostInstallScript = null!;
            public const string PreRemoveScript = null!;
            public const string PostRemoveScript = null!;
            public const string FileNameDesktop = Constants.HARDCODED_APP_NAME + ".desktop";
            public const string DebMaintainer = ThisAssembly.AssemblyCompany;
            public const string DebSection = "misc";
            public const string DebPriority = "extra";
            public const string DebHomepage = Url;
            public const string dotnet_runtime_6_0 = "dotnet-runtime-6.0";
            public const string aspnetcore_runtime_6_0 = "aspnetcore-runtime-6.0";

#if !SERVER
            public static void AddFileNameDesktop(ArchiveBuilder2 archiveBuilder2, List<ArchiveEntry> archiveEntries)
            {
                var metadata = new Dictionary<string, string>()
                {
                    { "CopyToPublishDirectory", "Always" },
                    { "LinuxPath", "/usr/share/applications/" + FileNameDesktop },
                    { "Link", FileNameDesktop },
                };

                var taskItem = new TaskItem(FileNameDesktop, metadata);
                var taskItems = new ITaskItem[] { taskItem };
                archiveBuilder2.AddFile(
                    Path.Combine(AppContext.BaseDirectory, FileNameDesktop),
                    FileNameDesktop,
                    Prefix,
                    archiveEntries,
                    taskItems);
            }
#endif
        }

        public static string GetConfiguration(bool dev, bool isLower)
            => dev ? (isLower ? "debug" : "Debug") : (isLower ? "release" : "Release");
    }
}