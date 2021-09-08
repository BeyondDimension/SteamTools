using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaFontManagerImpl : FontManagerImpl
    {
        bool useGdiPlusFirst;
        public AvaloniaFontManagerImpl(bool useGdiPlusFirst)
        {
            this.useGdiPlusFirst = useGdiPlusFirst;
        }

        public sealed override IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
        {
            IReadOnlyCollection<KeyValuePair<string, string>> fonts;

            if (useGdiPlusFirst)
            {
                try
                {
                    fonts = IFontManager.GetFontsByGdiPlus();
                }
                catch
                {
                    useGdiPlusFirst = false;
                    return GetFonts();
                }
            }
            else
            {
                fonts = GetFontsByAvalonia();
            }

            return fonts;
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