#if WINDOWS
//#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class GlobalDllImportResolver
{
    /// <summary>
    /// e_sqlite3 SQLitePCLRaw.provider.e_sqlite3
    /// </summary>
    const string e_sqlite3 = "e_sqlite3";

    /// <summary>
    /// libSkiaSharp SkiaSharp
    /// </summary>
    const string libSkiaSharp = "libSkiaSharp";

    /// <summary>
    /// libHarfBuzzSharp
    /// </summary>
    const string libHarfBuzzSharp = "libHarfBuzzSharp";

    /// <summary>
    /// av_libGLESv2.dll Avalonia.OpenGL
    /// </summary>
    const string av_libGLESv2 = "av_libGLESv2";

#if WINDOWS
    /// <summary>
    /// WebView2Loader
    /// </summary>
    const string WebView2Loader = "WebView2Loader";

    /// <summary>
    /// WinDivert
    /// </summary>
    const string WinDivert = "WinDivert";

    /// <summary>
    /// WinDivert
    /// </summary>
    const string WinDivert32 = "WinDivert32";

    /// <summary>
    /// WinDivert
    /// </summary>
    const string WinDivert64 = "WinDivert64";
#endif

    static IEnumerable<string> GetLibraryNames()
    {
        yield return e_sqlite3;
        yield return libSkiaSharp;
        yield return libHarfBuzzSharp;
        yield return av_libGLESv2;
#if WINDOWS
        yield return WebView2Loader;
        yield return WinDivert;
#endif
    }

    static readonly HashSet<string> libraryNames = new(GetLibraryNames());
}
#endif