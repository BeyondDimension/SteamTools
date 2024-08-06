#if WINDOWS || LINUX || TOOL_PUBLISH
//#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class GlobalDllImportResolver
{
    /// <summary>
    /// e_sqlite3 SQLitePCLRaw.provider.e_sqlite3
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string e_sqlite3 = "e_sqlite3";

    /// <summary>
    /// libSkiaSharp SkiaSharp
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string libSkiaSharp = "libSkiaSharp";

    /// <summary>
    /// libHarfBuzzSharp
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string libHarfBuzzSharp = "libHarfBuzzSharp";

#if WINDOWS || TOOL_PUBLISH

    /// <summary>
    /// av_libGLESv2.dll Avalonia.OpenGL
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string av_libGLESv2 = "av_libGLESv2";

    /// <summary>
    /// WebView2Loader
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string WebView2Loader = "WebView2Loader";

    /// <summary>
    /// WinDivert
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string WinDivert = "WinDivert";

    /// <summary>
    /// WinDivert
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string WinDivert32 = "WinDivert32";

    /// <summary>
    /// WinDivert
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string WinDivert64 = "WinDivert64";

    /// <summary>
    /// 7z
    /// </summary>
    [Mobius(Obsolete = true)]
    internal const string _7z = "7z";

    [Mobius(Obsolete = true)]
    internal const string xunyoucall64 = "xunyoucall64";
#endif

    [Mobius(Obsolete = true)]
    static IEnumerable<string> GetLibraryNames()
    {
        yield return e_sqlite3;
        yield return libSkiaSharp;
        yield return libHarfBuzzSharp;
#if WINDOWS || TOOL_PUBLISH
        yield return av_libGLESv2;
        yield return WebView2Loader;
        yield return WinDivert;
        yield return _7z;
        yield return xunyoucall64;
#endif
    }

#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
    [Mobius(Obsolete = true)]
    internal static readonly HashSet<string> libraryNames = new(GetLibraryNames());
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter
}
#endif