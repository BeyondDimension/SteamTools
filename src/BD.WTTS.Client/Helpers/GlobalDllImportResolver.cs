#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
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
    public static readonly HashSet<KeyValuePair<string, Assembly>> Pairs = new();

    public static string DebugInfo => string.Join(Environment.NewLine, Pairs.Select(x => $"{x.Key} {x.Value}"));
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
    public static string GetLibraryPath(string? libraryName = null)
        => Path.Combine(BaseDirectory, "native", RID, libraryName ?? string.Empty);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string GetLibraryFileName(string libraryName)
    {
        const string fileExtension =
#if WINDOWS
                ".dll";
#elif MACCATALYST || MACOS
                ".dylib";
#elif LINUX
                ".so";
#else
#endif
        if (!libraryName.EndsWith(fileExtension, StringComparison.OrdinalIgnoreCase))
            libraryName += fileExtension;
        return libraryName;
    }

    public static nint Delegate(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
#if DEBUG
        Console.WriteLine($"path: {searchPath}, name: {libraryName}, asm: {assembly}");
#endif

        if (libraryNames.Contains(libraryName))
        {
            var libraryPath = GetLibraryPath(GetLibraryFileName(libraryName));
            if (File.Exists(libraryPath) && NativeLibrary.TryLoad(libraryPath, out var handle))
                return handle;
        }

#if DEBUG
        Pairs.Add(new(libraryName, assembly));
#endif

        // Otherwise, fallback to default import resolver.

        return NativeLibrary.Load(libraryName, assembly, searchPath);
    }

#if DEBUG
    public static void MoveFiles()
    {
        foreach (var libraryName_ in libraryNames)
        {
            var libraryName = GetLibraryFileName(libraryName_);
            var rootLibraryPath = Path.Combine(BaseDirectory, libraryName);
            var rootLibraryFileExists = File.Exists(rootLibraryPath);
            if (rootLibraryFileExists)
            {
                var destLibraryPath = GetLibraryPath(libraryName);
                IOPath.FileTryDelete(destLibraryPath);
                var libraryDirPath = Path.GetDirectoryName(destLibraryPath);
                if (libraryDirPath != null)
                    IOPath.DirCreateByNotExists(libraryDirPath);
                File.Move(rootLibraryPath, destLibraryPath, true);
            }
        }
    }
#endif
}
#endif