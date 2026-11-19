#if WTTS_NATIVE_AOT
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BD.WTTS;

// Linked into the accelerator executable as well as the main client. Keep SQLite
// imports dynamic so both native executables can load the same shared library.
internal static class SharedSqliteNativeLibrary
{
    static readonly string executableDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

#if WINDOWS
    static string GetNativeDirectory(string directory) => Path.Combine(directory, "native", windowsRid);

    static readonly string windowsRid = $"win-{RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()}";
    static readonly string packageLibraries = GetNativeDirectory(GetPackageDirectory(executableDirectory));

    internal static string GetPackageLibraryPath(string filename) => Path.Combine(packageLibraries, filename);

    const uint LoadLibrarySearchDefaultDirs = 0x00001000;
    const uint LoadLibrarySearchDllLoadDir = 0x00000100;
    const uint LoadLibrarySearchSystem32 = 0x00000800;

    [DllImport("kernel32.dll", EntryPoint = "AddDllDirectory", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern nint AddDllDirectory(string directory);

    [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetDefaultDllDirectories(uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern nint LoadLibraryExW(string path, nint file, uint flags);

    static nint LoadWindowsLibrary(string path)
    {
        var handle = LoadLibraryExW(path, 0, LoadLibrarySearchDllLoadDir | LoadLibrarySearchSystem32);
        if (handle == 0)
            throw new DllNotFoundException(path, new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()));
        return handle;
    }

    const string sqliteFilename = "e_sqlite3.dll";
#else
    static readonly string sqliteFilename = OperatingSystem.IsMacOS() ? "libe_sqlite3.dylib" : "libe_sqlite3.so";
#endif

    static readonly string sqlitePath = GetSqlitePath();

    [ModuleInitializer]
    internal static void Register()
    {
#if WINDOWS
        // Graphics loaders can look up DLLs by name independently of a managed
        // import resolver. Add only our executable-relative RID directory and
        // exclude CWD and PATH from the default Windows loader search.
        if (File.Exists(GetPackageLibraryPath(sqliteFilename)) &&
            (AddDllDirectory(packageLibraries) == 0 || !SetDefaultDllDirectories(LoadLibrarySearchDefaultDirs)))
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
#if !APP_REVERSE_PROXY
        NativeLibrary.SetDllImportResolver(typeof(global::Steamworks.SteamClient).Assembly, ResolveSteamApi);
#endif
#endif
        NativeLibrary.SetDllImportResolver(typeof(global::SQLitePCL.SQLite3Provider_e_sqlite3).Assembly, Resolve);
    }

    static nint Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (name != "e_sqlite3" && name != sqliteFilename)
            return nint.Zero;

        // Give the runtime its own loader reference: a losing fixup race may free it.
#if WINDOWS
        return LoadWindowsLibrary(sqlitePath);
#else
        return NativeLibrary.Load(sqlitePath);
#endif
    }

#if WINDOWS && !APP_REVERSE_PROXY
    static nint ResolveSteamApi(string name, Assembly assembly, DllImportSearchPath? searchPath)
    {
        const string filename = "steam_api64.dll";
        if (!name.Equals("steam_api64", StringComparison.OrdinalIgnoreCase) &&
            !name.Equals(filename, StringComparison.OrdinalIgnoreCase))
            return nint.Zero;

        var path = GetPackageLibraryPath(filename);
        if (!File.Exists(path))
            path = Path.Combine(executableDirectory, filename);
        return LoadWindowsLibrary(path);
    }
#endif

    static string GetSqlitePath()
    {
#if WINDOWS
        var path = GetPackageLibraryPath(sqliteFilename);
        if (File.Exists(path))
            return path;
#endif
        // Preserve raw/older publish directories and the Unix layout, without using CWD or PATH.
        return Path.Combine(GetPackageDirectory(executableDirectory), sqliteFilename);
    }

    internal static string GetPackageDirectory(string directory)
    {
#if MACOS
        if (Path.GetFileName(directory) == "MacOS" &&
            Directory.GetParent(directory) is { Name: "Contents" } contentsDirectory)
        {
            var bundleLibraries = Path.Combine(contentsDirectory.FullName, "MonoBundle");
            if (Directory.Exists(bundleLibraries))
                return bundleLibraries;
        }
#endif
#if APP_REVERSE_PROXY
        // Standalone publish has its own libraries. Installed helpers share the parent package.
#if WINDOWS
        if (File.Exists(Path.Combine(GetNativeDirectory(directory), sqliteFilename)))
            return directory;
        // A packaged Windows helper must prefer the shared RID directory over
        // any leftover legacy copy beside the helper.
        var modules = Directory.GetParent(directory);
        if (modules != null && modules.Name.Equals("modules", StringComparison.OrdinalIgnoreCase) &&
            modules.Parent is { } package && File.Exists(Path.Combine(GetNativeDirectory(package.FullName), sqliteFilename)))
            return package.FullName;
#endif
        if (File.Exists(Path.Combine(directory, sqliteFilename)))
            return directory;
        var parent = Directory.GetParent(directory);
        if (parent != null && parent.Name.Equals("modules", StringComparison.OrdinalIgnoreCase))
            directory = parent.Parent?.FullName ?? directory;
#endif
        return directory;
    }
}
#endif
