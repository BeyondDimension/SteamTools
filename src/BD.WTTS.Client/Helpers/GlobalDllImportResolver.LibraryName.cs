#if WINDOWS || TOOL_PUBLISH
//#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class GlobalDllImportResolver
{
    /// <summary>
    /// e_sqlite3 SQLitePCLRaw.provider.e_sqlite3
    /// </summary>
    internal const string e_sqlite3 = "e_sqlite3";

    /// <summary>
    /// libSkiaSharp SkiaSharp
    /// </summary>
    internal const string libSkiaSharp = "libSkiaSharp";

    /// <summary>
    /// libHarfBuzzSharp
    /// </summary>
    internal const string libHarfBuzzSharp = "libHarfBuzzSharp";

    /// <summary>
    /// av_libGLESv2.dll Avalonia.OpenGL
    /// </summary>
    internal const string av_libGLESv2 = "av_libGLESv2";

#if WINDOWS || TOOL_PUBLISH
    /// <summary>
    /// WebView2Loader
    /// </summary>
    internal const string WebView2Loader = "WebView2Loader";

    /// <summary>
    /// WinDivert
    /// </summary>
    internal const string WinDivert = "WinDivert";

    /// <summary>
    /// WinDivert
    /// </summary>
    internal const string WinDivert32 = "WinDivert32";

    /// <summary>
    /// WinDivert
    /// </summary>
    internal const string WinDivert64 = "WinDivert64";

    /// <summary>
    /// 7z
    /// </summary>
    internal const string _7z = "7z";
#endif

    static IEnumerable<string> GetLibraryNames()
    {
        yield return e_sqlite3;
        yield return libSkiaSharp;
        yield return libHarfBuzzSharp;
        yield return av_libGLESv2;
#if WINDOWS || TOOL_PUBLISH
        yield return WebView2Loader;
        yield return WinDivert;
        yield return _7z;
#endif
    }

#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    internal static readonly HashSet<string> libraryNames = new(GetLibraryNames());
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter
}
#endif