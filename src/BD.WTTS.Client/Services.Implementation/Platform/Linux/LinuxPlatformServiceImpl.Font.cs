#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    string IPlatformService.GetDefaultFontFamily(FontWeight fontWeight)
    {
        // ubuntu-22.04.2-desktop-amd64.iso
        // Noto Sans CJK JP
        // Noto Sans CJK JP Black
        // Noto Sans CJK KR
        // Noto Sans CJK KR Black
        // Noto Sans CJK SC
        // Noto Sans CJK SC Black
        // Noto Sans CJK TC
        // Noto Sans CJK TC Black
        // Noto Sans CJK HK
        // Noto Sans CJK HK Black
        // Noto Sans CJK JP DemiLight
        // Noto Sans CJK KR DemiLight
        // Noto Sans CJK SC DemiLight
        // Noto Sans CJK TC DemiLight
        // Noto Sans CJK HK DemiLight
        // Noto Sans CJK JP Light
        // Noto Sans CJK KR Light
        // Noto Sans CJK SC Light
        // Noto Sans CJK TC Light
        // Noto Sans CJK HK Light
        // Noto Sans CJK JP Medium
        // Noto Sans CJK KR Medium
        // Noto Sans CJK SC Medium
        // Noto Sans CJK TC Medium
        // Noto Sans CJK HK Medium
        // Noto Sans CJK JP Thin
        // Noto Sans CJK KR Thin
        // Noto Sans CJK SC Thin
        // Noto Sans CJK TC Thin
        // Noto Sans CJK HK Thin

        // deepin-desktop-community-20.8-amd64.iso

        const ushort Thin = 100;
        const ushort ExtraLight = 200;
        const ushort Light = 300;
        const ushort Normal = 400;
        const ushort Medium = 500;
        const ushort Black = 900;

        var fontWeightValue = (ushort)fontWeight;
        if (fontWeightValue < 150)
            fontWeightValue = Thin;
        else if (fontWeightValue < 250)
            fontWeightValue = ExtraLight;
        else if (fontWeightValue < 350)
            fontWeightValue = Light;
        else if (fontWeightValue < 450)
            fontWeightValue = Normal;
        else if (fontWeightValue < 550)
            fontWeightValue = Medium;
        //else if (fontWeightValue < 650)
        //    fontWeightValue = SemiBold;
        //else if (fontWeightValue < 750)
        //    fontWeightValue = SemiBold;
        //else if (fontWeightValue < 850)
        //    fontWeightValue = ExtraBold;
        else /*if (fontWeightValue >= 950)*/
            fontWeightValue = Black;

        var culture = ResourceService.Culture;
        if (culture.IsMatch(AssemblyInfo.CultureName_SimplifiedChinese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK SC Thin",
                ExtraLight => "Noto Sans CJK SC DemiLight",
                Light => "Noto Sans CJK SC Light",
                Medium => "Noto Sans CJK SC Medium",
                Black => "Noto Sans CJK SC Black",
                _ or Normal => "Noto Sans CJK SC",
            };
        }
        else if (culture.IsMatch("zh-HK"))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK HK Thin",
                ExtraLight => "Noto Sans CJK HK DemiLight",
                Light => "Noto Sans CJK HK Light",
                Medium => "Noto Sans CJK HK Medium",
                Black => "Noto Sans CJK HK Black",
                _ or Normal => "Noto Sans CJK HK",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_TraditionalChinese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK TC Thin",
                ExtraLight => "Noto Sans CJK TC DemiLight",
                Light => "Noto Sans CJK TC Light",
                Medium => "Noto Sans CJK TC Medium",
                Black => "Noto Sans CJK TC Black",
                _ or Normal => "Noto Sans CJK TC",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_Japanese))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK JP Thin",
                ExtraLight => "Noto Sans CJK JP DemiLight",
                Light => "Noto Sans CJK JP Light",
                Medium => "Noto Sans CJK JP Medium",
                Black => "Noto Sans CJK JP Black",
                _ or Normal => "Noto Sans CJK JP",
            };
        }
        else if (culture.IsMatch(AssemblyInfo.CultureName_Korean))
        {
            return fontWeightValue switch
            {
                Thin => "Noto Sans CJK KR Thin",
                ExtraLight => "Noto Sans CJK KR DemiLight",
                Light => "Noto Sans CJK KR Light",
                Medium => "Noto Sans CJK KR Medium",
                Black => "Noto Sans CJK KR Black",
                _ or Normal => "Noto Sans CJK KR",
            };
        }
        return IPlatformService.DefaultGetDefaultFontFamily();
    }
}
#endif