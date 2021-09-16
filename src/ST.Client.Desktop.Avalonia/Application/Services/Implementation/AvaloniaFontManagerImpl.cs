using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaFontManagerImpl : FontManagerImpl
    {
        public static bool UseGdiPlusFirst { get; set; }

        public AvaloniaFontManagerImpl(IDesktopPlatformService platformService) : base(platformService)
        {
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

        static IReadOnlyCollection<KeyValuePair<string, string>> GetFontsByAvalonia()
        {
            var fonts = FontManager.Current.GetInstalledFontFamilyNames();
            var list = fonts.Select(x => KeyValuePair.Create(x, x)).ToList();
            list.Insert(0, IFontManager.Default);
            return list;
        }
    }
}