using Avalonia.Media;
using Avalonia.Platform;
using System.Application.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Application.Services.Implementation
{
    public partial class AvaloniaFontManagerImpl : FontManagerImpl
    {
        readonly string _defaultFamilyName;
        readonly IAvaloniaApplication application;

        readonly Typeface _defaultTypeface =
           new("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/Fonts#WenQuanYi Micro Hei");

        public static bool UseGdiPlusFirst { get; set; }

        public AvaloniaFontManagerImpl(IPlatformService platformService, IAvaloniaApplication application) : base(platformService)
        {
            this.application = application;
            _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
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
            var fonts = GetInstalledFontFamilyNames();
            var list = fonts.Select(x => KeyValuePair.Create(x, x)).ToList();
            list.Insert(0, IFontManager.Default);
            return list;
        }

        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false) => application.RenderingSubsystemName switch
        {
            Skia => GetInstalledFontFamilyNamesBySkia(checkForUpdates),
            Direct2D1 => GetInstalledFontFamilyNamesByDirect2D1(checkForUpdates),
            _ => throw new NotSupportedException(),
        };

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
          CultureInfo culture, out Typeface fontKey) => application.RenderingSubsystemName switch
          {
              Skia => TryMatchCharacterBySkia(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
              Direct2D1 => TryMatchCharacterByDirect2D1(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
              _ => throw new NotSupportedException(),
          };

        public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface) => application.RenderingSubsystemName switch
        {
            Skia => CreateGlyphTypefaceBySkia(typeface),
            Direct2D1 => CreateGlyphTypefaceByDirect2D1(typeface),
            _ => throw new NotSupportedException(),
        };
    }
}