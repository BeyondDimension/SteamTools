#if WINDOWS || LINUX
//#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// 解析从程序集进行的本机库导入
/// <para>https://learn.microsoft.com/zh-cn/dotnet/standard/native-interop/cross-platform#custom-import-resolver</para>
/// <para>本机库放入 .\native\win-x64\xxx.dll</para>
/// <para>参考：https://learn.microsoft.com/zh-cn/nuget/create-packages/supporting-multiple-target-frameworks#architecture-specific-folders</para>
/// </summary>
public static partial class GlobalDllImportResolver
{
#if DEBUG
    //public static readonly ConcurrentBag<KeyValuePair<string, Assembly>> Pairs = new();

    //public static string DebugInfo => string.Join(Environment.NewLine, Pairs.Select(x => $"{x.Key} {x.Value}"));
#endif

    /// <inheritdoc cref="RID"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetRID()
    {
#if WINDOWS
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "win-x86",
            Architecture.X64 => "win-x64",
            Architecture.Arm64 => "win-arm64",
            _ => throw new PlatformNotSupportedException(),
        };
#elif MACCATALYST || MACOS
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "osx-x64", // 最低 OS 版本为 macOS 10.12 Sierra
            Architecture.Arm64 => "osx.11.0-arm64",
            _ => throw new PlatformNotSupportedException(),
        };
#elif LINUX
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "linux-x64", // 大多数桌面发行版，如 CentOS、Debian、Fedora、Ubuntu 及派生版本
            Architecture.Arm64 => "linux-arm64", // 在 64 位 ARM 上运行的 Linux 发行版本，如 Raspberry Pi Model 3 及更高版本上的 Ubuntu 服务器 64 位
            Architecture.Arm => "linux-arm", // 在 ARM 上运行的 Linux 发行版本，如 Raspberry Pi Model 2 及更高版本上的 Raspbian
            Architecture.Armv6 => "linux-armv6",
            _ => throw new PlatformNotSupportedException(),
        };
#else
        throw new PlatformNotSupportedException();
#endif
    }

    /// <summary>
    /// https://learn.microsoft.com/zh-cn/dotnet/core/rid-catalog
    /// </summary>
    public static readonly string RID = GetRID();

    static string? _BaseDirectory;

    /// <inheritdoc cref="AppContext.BaseDirectory"/>
    public static string BaseDirectory
    {
        get => _BaseDirectory ?? AppContext.BaseDirectory;
        set => _BaseDirectory = value;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //static string GetDllDirectory() => GetLibraryPath();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetLibraryPath(string? libraryName = null, string? baseDirectory = null)
    {
        var libraryPath = Path.Combine(baseDirectory ?? BaseDirectory, "native", RID, libraryName ?? string.Empty);
        if (libraryName != null &&
            libraryName.StartsWith("WinDivert"))
        {
            libraryPath = Path.Combine(IOPath.AppDataDirectory, "native", RID, libraryName ?? string.Empty);
        }
        return libraryPath;
    }

    const string fileExtension_ =
#if WINDOWS
            ".dll";
#elif MACCATALYST || MACOS
            ".dylib";
#elif LINUX
            ".so";
#else
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetLibraryFileName(string libraryName, string? fileExtension = null)
    {
        fileExtension ??= fileExtension_;
        if (!libraryName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
            libraryName += fileExtension;
        return libraryName;
    }

    static readonly ConcurrentDictionary<string, string> pairs = new();
    static readonly ConcurrentDictionary<string, nint> loadNativeLibrarys = new();

    static bool TryLoad(string libraryPath, out nint handle)
    {
        if (loadNativeLibrarys.TryGetValue(libraryPath, out handle))
        {
            return true;
        }
        if (NativeLibrary.TryLoad(libraryPath, out handle))
        {
            loadNativeLibrarys.TryAdd(libraryPath, handle);
            return true;
        }
        return false;
    }

    static bool TryLoad(string libraryPath, Assembly assembly, DllImportSearchPath? searchPath, out nint handle)
    {
        if (loadNativeLibrarys.TryGetValue(libraryPath, out handle))
        {
            return true;
        }
        if (NativeLibrary.TryLoad(libraryPath, assembly, searchPath, out handle))
        {
            loadNativeLibrarys.TryAdd(libraryPath, handle);
            return true;
        }
        return false;
    }

    public static nint Delegate(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        libraryName = libraryName.TrimEnd(fileExtension_, StringComparison.OrdinalIgnoreCase);

        if (pairs.TryGetValue(libraryName, out var libraryPath) &&
            TryLoad(libraryPath, out var handle))
        {
            return handle;
        }

        //#if DEBUG
        //        try
        //        {
        //            if (Pairs.All(x => x.Key != libraryName))
        //                Console.WriteLine($"path: {searchPath}, name: {libraryName}, asm: {assembly}");
        //        }
        //        catch
        //        {

        //        }
        //#endif

        if (libraryNames.Contains(libraryName))
        {
            var libraryFileName = GetLibraryFileName(libraryName);
            libraryPath = GetLibraryPath(libraryFileName);
            if (File.Exists(libraryPath) && TryLoad(libraryPath, out handle))
            {
                pairs[libraryName] = libraryPath;
                return handle;
            }
#if LINUX || MACCATALYST || MACOS
            if (!libraryFileName.StartsWith("lib"))
            {
                libraryFileName = $"lib{libraryFileName}";
                libraryPath = GetLibraryPath(libraryFileName);
                if (File.Exists(libraryPath) && TryLoad(libraryPath, out handle))
                {
                    pairs[libraryName] = libraryPath;
                    return handle;
                }
            }
#endif
        }

        //#if DEBUG
        //        try
        //        {
        //            Pairs.Add(new(libraryName, assembly));
        //        }
        //        catch
        //        {

        //        }
        //#endif

        // Otherwise, fallback to default import resolver.

        return TryLoad(libraryName, assembly, searchPath, out var handle2) ? handle2 : default;
    }

#if DEBUG
    /// <summary>
    /// 调试时加载本机库之前将本机库位置移动到预设文件夹中
    /// </summary>
    public static void MoveFiles()
    {
        //Console.WriteLine("MoveFiles");
        foreach (var libraryName in libraryNames)
        {
            var libraryFileName = GetLibraryFileName(libraryName);
            MoveFiles(libraryFileName);
#if LINUX || MACCATALYST || MACOS
            if (!libraryFileName.StartsWith("lib"))
            {
                libraryFileName = $"lib{libraryFileName}";
                MoveFiles(libraryFileName);
            }
#endif
        }

#if WINDOWS
        MoveFiles(GetLibraryFileName(WinDivert32, ".sys"));
        MoveFiles(GetLibraryFileName(WinDivert64, ".sys"));
#endif

        static void MoveFiles(string libraryName)
        {
            var rootLibraryPath = Path.Combine(BaseDirectory, libraryName);
            //Console.WriteLine($"rootLibraryPath: {rootLibraryPath}");
            var rootLibraryFileExists = File.Exists(rootLibraryPath);
            if (!rootLibraryFileExists)
            {
                rootLibraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!, libraryName);
                //Console.WriteLine($"rootLibraryPath: {rootLibraryPath}");
                rootLibraryFileExists = File.Exists(rootLibraryPath);
            }
            if (!rootLibraryFileExists)
            {
                rootLibraryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location!)!, "runtimes", RID, "native", libraryName);
                rootLibraryFileExists = File.Exists(rootLibraryPath);
            }
            if (rootLibraryFileExists)
            {
                var destLibraryPath = GetLibraryPath(libraryName);
                IOPath.FileTryDelete(destLibraryPath);
                var libraryDirPath = Path.GetDirectoryName(destLibraryPath);
                if (libraryDirPath != null)
                    IOPath.DirCreateByNotExists(libraryDirPath);

                Console.WriteLine($"MoveFiles rootLibraryPath: {rootLibraryPath}, destLibraryPath: {destLibraryPath}");
                File.Move(rootLibraryPath, destLibraryPath, true);
            }
        }
    }
#endif
}
#endif