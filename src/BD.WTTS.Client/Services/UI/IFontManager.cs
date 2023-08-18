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
    /// Windows GDI+ 实现其他平台 SkiaSharp 实现
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
    {
        // https://docs.microsoft.com/zh-cn/typography/font-list
        var list = SkiaSharp.SKFontManager.Default.GetFontFamilies()
            .Select(x => new KeyValuePair<string, string>(x, x))
            .ToList();
        list.Insert(0, IFontManager.Default);
        return list;
    }
}