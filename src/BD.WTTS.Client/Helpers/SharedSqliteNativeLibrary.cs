#if WTTS_NATIVE_AOT
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BD.WTTS;

// Linked into the accelerator executable as well as the main client. Keep provider
// imports dynamic so both native executables map the same package-root library.
internal static class SharedSqliteNativeLibrary
{
    static readonly Lazy<nint> library = new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    [ModuleInitializer]
    internal static void Register() => NativeLibrary.SetDllImportResolver(
        typeof(global::SQLitePCL.SQLite3Provider_e_sqlite3).Assembly, Resolve);

    static nint Resolve(string name, Assembly assembly, DllImportSearchPath? searchPath) =>
        name == "e_sqlite3" ? library.Value : nint.Zero;

    static nint Load()
    {
        var directory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;
#if APP_REVERSE_PROXY
        // Standalone dotnet publish remains usable; only the documented modules/
        // layout shares its native library with the parent package.
        var parent = Directory.GetParent(directory);
        if (parent != null && parent.Name.Equals("modules", StringComparison.OrdinalIgnoreCase))
            directory = parent.Parent?.FullName ?? directory;
#endif
        var filename = OperatingSystem.IsWindows() ? "e_sqlite3.dll"
            : OperatingSystem.IsMacOS() ? "libe_sqlite3.dylib" : "libe_sqlite3.so";
        // An absolute path is deliberate: a missing shared dependency must not
        // silently load an unrelated library from CWD or the machine search path.
        return NativeLibrary.Load(Path.Combine(directory, filename));
    }
}
#endif
