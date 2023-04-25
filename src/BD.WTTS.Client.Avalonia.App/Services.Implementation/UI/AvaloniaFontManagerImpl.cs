namespace BD.WTTS.Services.Implementation;

sealed class AvaloniaFontManagerImpl : FontManagerImpl, IFontManagerImpl2
{
    //readonly string _defaultFamilyName;
    readonly IFontManagerImpl impl;

    //static readonly Typeface _defaultTypeface =
    //    new("avares://BD.WTTS.Client.Avalonia.App/Application/UI/Assets/Fonts#WenQuanYi Micro Hei");

    public static bool UseGdiPlusFirst { get; set; }

    public AvaloniaFontManagerImpl(IPlatformService platformService) : base(platformService)
    {
        //_defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
        impl = IFontManagerImpl2.CreateFontManager();
    }

    public sealed override IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
    {
        if (UseGdiPlusFirst)
        {
            try
            {
                var fonts = platformService.GetFontsByGdiPlus();
                if (fonts.Any()) return fonts;
            }
            catch
            {
            }
            UseGdiPlusFirst = false;
            return GetFonts();
        }
        else
        {
            return GetFontsByAvalonia();
        }
    }

    IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByAvalonia()
    {
        var fonts = Impl.GetInstalledFontFamilyNames(true);
        var list = fonts.Select(x => new KeyValuePair<string, string>(x, x)).ToList();
        list.Insert(0, IFontManager.Default);
        return list;
    }

    IFontManagerImpl IFontManagerImpl2.Impl => impl;

    IFontManagerImpl2 Impl => (IFontManagerImpl2)impl;

    string IFontManagerImpl2.DefaultFontFamilyName => platformService.GetDefaultFontFamily();

    internal static bool IsDefaultFontFamilyName(string name)
        => IsDefaultFontFamilyName(IPlatformService.Instance, name);

    internal static bool IsDefaultFontFamilyName(IPlatformService platformService, string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        if (string.Equals(name, IFontManager.KEY_Default, StringComparison.OrdinalIgnoreCase))
            return true;
        if (string.Equals(name, IFontManager.KEY_WinUI, StringComparison.OrdinalIgnoreCase))
            return true;
        if (new FontFamily(platformService.GetDefaultFontFamily()).FamilyNames.Contains(name))
            return true;
        return name switch
        {
            FontFamily.DefaultFontFamilyName => true,
            _ => false,
        };
    }

    string IFontManagerImpl2.OnCreateGlyphTypeface(string fontFamilyName)
    {
        if (IsDefaultFontFamilyName(platformService, fontFamilyName))
            return platformService.GetDefaultFontFamily();
        return fontFamilyName;
    }

    static readonly Lazy<FontFamily> mDefault = new(() =>
    {
        var name = IPlatformService.Instance.GetDefaultFontFamily();
        if (string.IsNullOrEmpty(name))
            return FontFamily.Default;
        return new FontFamily(name);
    });

    public static FontFamily Default => mDefault.Value;
}