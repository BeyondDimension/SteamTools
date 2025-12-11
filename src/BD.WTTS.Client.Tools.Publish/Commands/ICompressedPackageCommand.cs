using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip.Compression;
using Packaging.Targets.IO;
using SevenZip;
using ZstdNet;
using NCompressionMode = System.IO.Compression.CompressionMode;

namespace BD.WTTS.Client.Tools.Publish.Commands;

/// <summary>
/// 创建压缩包命令
/// </summary>
interface ICompressedPackageCommand : ICommand
{
    const string commandName = "compressed";

    static Command ICommand.GetCommand()
    {
        var sevenzip = new Option<bool>("--7z", "Create 7z compressed");
        var zip = new Option<bool>("--zip", "Create Zip compressed");
        var tgz = new Option<bool>("--gz", "Create tgz compressed");
        var zstd = new Option<bool>("--zstd", "Create tar.zst compressed");
        var rids = new Option<string[]>("--rids", "RID is short for runtime identifier");
        var command = new Command(commandName, "Create compressed package")
        {
            sevenzip, zip, tgz, zstd, rids,
        };
        command.SetHandler(Handler, sevenzip, zip, tgz, zstd, rids);
        return command;
    }

    internal static void Handler(bool sevenzip, bool zip, bool tgz, bool zstd, string[] rids)
    {
        var tasks = AppPublishInfo.Instance.
            Where(x => rids.Contains(x.RuntimeIdentifier)).
            Select(x =>
            {
                return GetTasks();
                IEnumerable<ThreadTask> GetTasks()
                {
                    if (sevenzip)
                        yield return InBackground(() =>
                        {
                            GenerateCompressedPackage(x, CloudFileType.SevenZip);
                        });
                    if (tgz)
                        yield return InBackground(() =>
                        {
                            GenerateCompressedPackage(x, CloudFileType.TarGzip);
                        });
                    if (zstd)
                        yield return InBackground(() =>
                        {
                            GenerateCompressedPackage(x, CloudFileType.TarZstd);
                        });
                }
            }).SelectMany(x => x).ToArray();
        ThreadTask.WaitAll(tasks);
        Console.WriteLine($"{commandName} OK");
    }

    static void GenerateCompressedPackage(AppPublishInfo item, CloudFileType type)
    {
        var fileEx = GetFileExByCloudFileType(type);
        var packPath = GetPackPath(item, fileEx);
        Console.WriteLine($"正在生成压缩包：{packPath}");
        IOPath.FileIfExistsItDelete(packPath);

        GetCreatePackByCloudFileType(type)(packPath, item.Files);

        using var fileStream = File.OpenRead(packPath);
        var sha384 = Hashs.String.SHA384(fileStream);
        var sha256 = Hashs.String.SHA256(fileStream);
        AppPublishFileInfo info = new()
        {
            FileEx = fileEx,
            FilePath = packPath,
            Length = fileStream.Length,
            SHA384 = sha384,
            SHA256 = sha256,
        };
        if (item.SingleFile.ContainsKey(type))
            item.SingleFile[type] = info;
        else
            item.SingleFile.Add(type, info);
        Console.WriteLine($"压缩包已生成：{packPath}");
    }

    /// <summary>
    /// 根据文件类型获取文件扩展名
    /// </summary>
    /// <param name="compressedType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static string GetFileExByCloudFileType(CloudFileType cloudFileType) => cloudFileType switch
    {
        CloudFileType.TarGzip => FileEx.TAR_GZ,
        CloudFileType.TarBrotli => FileEx.TAR_BR_LONG,
        CloudFileType.SevenZip => FileEx._7Z,
        CloudFileType.TarZstd => FileEx.TAR_ZST,
        CloudFileType.TarXz => FileEx.TAR_XZ,
        CloudFileType.Json => FileEx.JSON,
        CloudFileType.Dll => ".dll",
        CloudFileType.Xml => ".xml",
        CloudFileType.So => ".so",
        CloudFileType.Dylib => ".dylib",
        CloudFileType.None => "",
        CloudFileType.Js => ".js",
        CloudFileType.Xaml => ".xaml",
        CloudFileType.AXaml => ".axaml",
        CloudFileType.CSharp => ".cs",
        _ => throw new ArgumentOutOfRangeException(nameof(cloudFileType), cloudFileType, null),
    };

    /// <summary>
    /// 根据压缩包类型获取创建压缩包委托
    /// </summary>
    /// <param name="compressedType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static Action<string, IEnumerable<AppPublishFileInfo>> GetCreatePackByCloudFileType(CloudFileType cloudFileType) => cloudFileType switch
    {
        CloudFileType.TarGzip => CreateGZipPack,
        CloudFileType.TarBrotli => CreateBrotliPack,
        CloudFileType.SevenZip => CreateSevenZipPack,
        CloudFileType.TarZstd => CreateZstdPack,
        CloudFileType.TarXz => CreateXZPack,
        _ => throw new ArgumentOutOfRangeException(nameof(cloudFileType), cloudFileType, null),
    };

    static void CreateGZipPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        using var fs = File.Create(packPath);
        using var s = new GZipOutputStream(fs);
        s.SetLevel(Deflater.BEST_COMPRESSION);
        using var archive = TarArchive.CreateOutputTarArchive(s,
            TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
        foreach (var file in files)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{file.FilePath}");
#endif
            var entry = TarEntry.CreateEntryFromFile(file.FilePath);
            entry.Name = file.RelativePath;
            if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
            archive.WriteEntry(entry, false);
        }
    }

    static void CreateBrotliPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        using var fs = File.Create(packPath);
        using var s = new BrotliStream(fs, NCompressionMode.Compress);
        using var archive = TarArchive.CreateOutputTarArchive(s,
            TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
        foreach (var file in files)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{file.FilePath}");
#endif
            var entry = TarEntry.CreateEntryFromFile(file.FilePath);
            entry.Name = file.RelativePath;
            if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
            archive.WriteEntry(entry, false);
        }
    }

    static readonly Lazy<object?> SetSevenZipLibraryPath = new(() =>
    {
        var libPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "7-Zip", "7z.dll");
        SevenZipBase.SetLibraryPath(libPath);
        return null;
    });

    static void CreateSevenZipPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        _ = SetSevenZipLibraryPath.Value;
        SevenZipCompressor? compressor = new()
        {
            ArchiveFormat = OutArchiveFormat.SevenZip,
            CompressionLevel = SevenZip.CompressionLevel.Ultra,
            CompressionMethod = CompressionMethod.Lzma2,
            FastCompression = false,
            ScanOnlyWritable = true,
            DirectoryStructure = true,
        };
        compressor.FileCompressionStarted += FileCompressionStarted;
        static void FileCompressionStarted(object? s, FileNameEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{e.FileName}");
#endif
        }
        var dict = files.ToDictionary(x => x.RelativePath, x => x.FilePath);
        compressor.CompressFileDictionary(dict, packPath);
        compressor.FileCompressionStarted -= FileCompressionStarted;
        compressor = null;
    }

    static void CreateZstdPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        using var fs = File.Create(packPath);
        var maxCompressionLevel = CompressionOptions.MaxCompressionLevel;
        Console.WriteLine($"MaxCompressionLevel: {maxCompressionLevel}");
        using var s = new CompressionStream(fs, new CompressionOptions(maxCompressionLevel));
        using var archive = TarArchive.CreateOutputTarArchive(s,
            TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
        foreach (var file in files)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{file.FilePath}");
#endif
            var entry = TarEntry.CreateEntryFromFile(file.FilePath);
            entry.Name = file.RelativePath;
            if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
            archive.WriteEntry(entry, false);
        }
    }

    static void CreateXZPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        using var fs = File.Create(packPath);
        using var s = new XZOutputStream2(fs);
        using var archive = TarArchive.CreateOutputTarArchive(s,
            TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
        foreach (var file in files)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{file.FilePath}");
#endif
            var entry = TarEntry.CreateEntryFromFile(file.FilePath);
            entry.Name = file.RelativePath;
            if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
            archive.WriteEntry(entry, false);
        }
    }

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

    static void CreateZipPack(string packPath, IEnumerable<AppPublishFileInfo> files)
    {
        using var archive = ZipFile.Open(packPath, ZipArchiveMode.Create);
        foreach (var file in files)
        {
#if DEBUG
            Console.WriteLine($"正在压缩：{file.FilePath}");
#endif
            var name = file.RelativePath;
            if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                name = name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
            archive.CreateEntryFromFile(file.FilePath, name);
        }
    }
}
