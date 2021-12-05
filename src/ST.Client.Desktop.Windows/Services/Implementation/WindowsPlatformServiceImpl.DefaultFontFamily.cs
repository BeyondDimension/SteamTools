using System;
using System.Application.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl
    {
        static readonly IReadOnlyDictionary<FontWeight, Lazy<string?>> mDefaultFontFamily = Enum2.GetAll<FontWeight>().ToDictionary(k => k, v => new Lazy<string?>(() => GetDefaultFontFamily(v)));

        static string? GetDefaultFontFamily(FontWeight fontWeight)
        {
            if (OperatingSystem2.IsWindows)
            {
                // (版权、许可)不能在非 Windows 上使用 微软雅黑字体，不可将字体嵌入程序
                var fontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
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
                else if (!OperatingSystem2.IsWindowsVersionAtLeast(6, 3))
                {
                    var msyh_ttf = Path.Combine(fontsPath, fontWeight switch
                    {
                        // https://docs.microsoft.com/en-us/typography/fonts/windows_8_font_list
                        // https://docs.microsoft.com/en-us/typography/fonts/windows_7_font_list
                        FontWeight.Bold => "msyhbd.ttf",
                        _ => "msyh.ttf",
                    });
                    if (File.Exists(msyh_ttf))
                    {
                        if (OperatingSystem2.IsWindows7)
                        {
                            // Microsoft YaHei: A Simplified Chinese font developed by taking advantage of ClearType technology, and it provides excellent reading experience particularly onscreen. The font is very legible at small sizes.
                            return fontWeight switch
                            {
                                FontWeight.Bold => "Microsoft YaHei Bold",
                                _ => "Microsoft YaHei",
                            };
                        }
                        else
                        {
                            return fontWeight switch
                            {
                                FontWeight.Bold => "Microsoft YaHei UI Bold",
                                _ => "Microsoft YaHei UI",
                            };
                        }
                    }
                }
            }
            return null;
        }

        static string? GetDefaultFontFamilyCore(FontWeight fontWeight) => mDefaultFontFamily[fontWeight].Value;

        string? IPlatformService.GetDefaultFontFamily(FontWeight fontWeight)
        {
            var fontFamily = GetDefaultFontFamilyCore(fontWeight);
            if (fontFamily == null && fontWeight != FontWeight.Normal)
            {
                return GetDefaultFontFamilyCore(FontWeight.Normal);
            }
            return fontFamily;
        }
    }
}
