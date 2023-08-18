#if WINDOWS
using Nito.Comparers.Linq;
using System.Drawing;
using System.Drawing.Text;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
    {
        // https://docs.microsoft.com/zh-cn/typography/font-list
        var culture = ResourceService.Culture;
        InstalledFontCollection ifc = new();
        var list = ifc.Families
            .Where(x => x.IsStyleAvailable(FontStyle.Regular))
            .Select(x => new KeyValuePair<string, string>(x.GetName(culture.LCID), x.GetName(1033)))
            .ToList();
        list.Insert(0, IFontManager.Default);
        return list;
    }

    static readonly IReadOnlyDictionary<FontWeight, Lazy<string?>> mDefaultFontFamily = Enum2.GetAll<FontWeight>().Distinct().ToDictionary(k => k, v => new Lazy<string?>(() => GetDefaultFontFamily(v)));

    static string? GetDefaultFontFamily(FontWeight fontWeight)
    {
        // (版权、许可)不能在非 Windows 上使用 微软雅黑字体，不可将字体嵌入程序
        var fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        var fontWeightValue = (ushort)fontWeight;
        if (fontWeightValue < 350)
            fontWeight = FontWeight.Light;
        else if (fontWeightValue >= 650)
            fontWeight = FontWeight.Bold;
        else
            fontWeight = FontWeight.Normal;

        var msyh_ttc = Path.Combine(fontsPath, fontWeight switch
        {
            // Microsoft YaHei UI: The font glyphs are certified compliant with China standard GB18030-2000 with the font name Founder Lan Ting Hei. Microsoft Licensed the font glyph from Beijing Founder Electronics Co. Ltd. GB18030-2000
            // https://docs.microsoft.com/en-us/typography/font-list/microsoft-yahei
            // https://docs.microsoft.com/en-us/typography/fonts/windows_81_font_list
            FontWeight.Light => "msyhl.ttc",
            FontWeight.Bold => "msyhbd.ttc",
            _ => "msyh.ttc",
        });
        if (File.Exists(msyh_ttc))
        {
            return fontWeight switch
            {
                FontWeight.Light => "Microsoft YaHei UI Light",
                FontWeight.Bold => "Microsoft YaHei UI Bold",
                _ => "Microsoft YaHei UI",
            };
        }
        //else if (!OperatingSystem.IsWindowsVersionAtLeast(6, 3))
        //{
        //    var msyh_ttf = Path.Combine(fontsPath, fontWeight switch
        //    {
        //        // https://docs.microsoft.com/en-us/typography/fonts/windows_8_font_list
        //        // https://docs.microsoft.com/en-us/typography/fonts/windows_7_font_list
        //        FontWeight.Bold => "msyhbd.ttf",
        //        _ => "msyh.ttf",
        //    });
        //    if (File.Exists(msyh_ttf))
        //    {
        //        //if (OperatingSystem2.IsWindows7())
        //        //{
        //        //    // Microsoft YaHei: A Simplified Chinese font developed by taking advantage of ClearType technology, and it provides excellent reading experience particularly onscreen. The font is very legible at small sizes.
        //        //    return fontWeight switch
        //        //    {
        //        //        FontWeight.Bold => "Microsoft YaHei Bold",
        //        //        _ => "Microsoft YaHei",
        //        //    };
        //        //}
        //        //else
        //        //{
        //        return fontWeight switch
        //        {
        //            FontWeight.Bold => "Microsoft YaHei UI Bold",
        //            _ => "Microsoft YaHei UI",
        //        };
        //        //}
        //    }
        //}
        return null;
    }

    static string? GetDefaultFontFamilyCore(FontWeight fontWeight) => mDefaultFontFamily[fontWeight].Value;

    string IPlatformService.GetDefaultFontFamily(FontWeight fontWeight)
    {
        var fontFamily = GetDefaultFontFamilyCore(fontWeight);
        if (string.IsNullOrWhiteSpace(fontFamily))
        {
            if (fontWeight != FontWeight.Normal)
            {
                var defaultFontFamily = GetDefaultFontFamilyCore(FontWeight.Normal);
                if (!string.IsNullOrWhiteSpace(defaultFontFamily))
                    return defaultFontFamily;
            }
            return IPlatformService.DefaultGetDefaultFontFamily();
        }
        else
        {
            return fontFamily;
        }
    }
}
#endif