using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 字体管理
/// </summary>
public interface IFontManager
{
    static IFontManager Instance => Ioc.Get<IFontManager>();

    static KeyValuePair<string, string> Default { get; } = new(AppResources.Default, KEY_Default);

    IReadOnlyCollection<KeyValuePair<string, string>> GetFonts();

    const string KEY_Default = "Default";
    //const string KEY_DefaultConsole = "DefaultConsole";
    //const string ConsoleFont_CascadiaCode = "Cascadia Code";
    //const string ConsoleFont_Consolas = "Consolas";
    //const string ConsoleFont_SourceCodePro = "Source Code Pro";
    //const string ConsoleFont_JetBrainsMono = "JetBrains Mono";

    /// <summary>
    /// https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.media.fontfamily.xamlautofontfamily?view=winrt-22000
    /// </summary>
    public const string KEY_WinUI = "XamlAutoFontFamily";
}

partial interface IPlatformService
{
    /// <summary>
    /// 由 GDI+ 实现的获取当前系统字体数组，仅在 Windows 平台上实现，其他平台将返回空数组
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByGdiPlus()
    {
        // Common7\IDE\ReferenceAssemblies\Microsoft\Framework\MonoAndroid\v1.0\Facades\System.Drawing.Common.dll
        // System.Drawing.Text.InstalledFontCollection
        // throw new PlatformNotSupportedException();
        return Array.Empty<KeyValuePair<string, string>>();
    }
}