#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    string IPlatformService.GetDefaultFontFamily(FontWeight fontWeight)
    {
        const ushort Thin = 100;
        const ushort ExtraLight = 200;
        const ushort Light = 300;
        const ushort Normal = 400;
        const ushort Medium = 500;
        const ushort SemiBold = 600;
        const ushort ExtraBold = 800;
        const ushort Black = 900;

        var fontWeightValue = (ushort)fontWeight;
        if (fontWeightValue < 50)
            fontWeightValue = Thin;
        if (fontWeightValue < 250)
            fontWeightValue = ExtraLight;
        if (fontWeightValue < 350)
            fontWeightValue = Light;
        if (fontWeightValue < 450)
            fontWeightValue = Normal;
        if (fontWeightValue < 550)
            fontWeightValue = Medium;
        if (fontWeightValue < 650)
            fontWeightValue = SemiBold;
        if (fontWeightValue < 850)
            fontWeightValue = ExtraBold;
        if (fontWeightValue >= 950)
            fontWeightValue = Black;

        var culture = ResourceService.Culture;
        if (culture.IsMatch(AssemblyInfo.CultureName_SimplifiedChinese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK SC Thin",
                ExtraLight => "Noto Sans CJK SC ExtraLight",
                Light => "Noto Sans CJK SC Light",
                Medium => "Noto Sans CJK SC Medium",
                SemiBold => "Noto Sans CJK SC SemiBold",
                Black => "Noto Sans CJK SC Black",
                _ or Normal => "Noto Sans CJK SC",
            };
        }
        else if (culture.IsMatch("zh-HK"))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK HK Thin",
                ExtraLight => "Noto Sans CJK HK ExtraLight",
                Light => "Noto Sans CJK HK Light",
                Medium => "Noto Sans CJK HK Medium",
                SemiBold => "Noto Sans CJK HK SemiBold",
                Black => "Noto Sans CJK HK Black",
                _ or Normal => "Noto Sans CJK HK",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_TraditionalChinese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK TC Thin",
                ExtraLight => "Noto Sans CJK TC ExtraLight",
                Light => "Noto Sans CJK TC Light",
                Medium => "Noto Sans CJK TC Medium",
                SemiBold => "Noto Sans CJK TC SemiBold",
                Black => "Noto Sans CJK TC Black",
                _ or Normal => "Noto Sans CJK TC",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_Japanese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK JP Thin",
                ExtraLight => "Noto Sans CJK JP ExtraLight",
                Light => "Noto Sans CJK JP Light",
                Medium => "Noto Sans CJK JP Medium",
                SemiBold => "Noto Sans CJK JP SemiBold",
                Black => "Noto Sans CJK JP Black",
                _ or Normal => "Noto Sans CJK JP",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_Korean))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK KR Thin",
                ExtraLight => "Noto Sans CJK KR ExtraLight",
                Light => "Noto Sans CJK KR Light",
                Medium => "Noto Sans CJK KR Medium",
                SemiBold => "Noto Sans CJK KR SemiBold",
                Black => "Noto Sans CJK KR Black",
                _ or Normal => "Noto Sans CJK KR",
            };
        }
        return IPlatformService.DefaultGetDefaultFontFamily();
    }
}
#endif