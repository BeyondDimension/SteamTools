// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Interop
{
#if MACCATALYST || MACOS || LINUX || IOS || ANDROID
    public static partial class Libc
    {
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [LibraryImport("libc", EntryPoint = "geteuid", SetLastError = true)]
        public static partial uint GetEUID();
    }
#endif
}