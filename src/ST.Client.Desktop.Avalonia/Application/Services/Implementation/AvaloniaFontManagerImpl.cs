using Avalonia.Media;
using Avalonia.Platform;
using System.Application.UI;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services.Implementation
{
    public partial class AvaloniaFontManagerImpl : FontManagerImpl, IFontManagerImpl2
    {
        readonly string _defaultFamilyName;
        //readonly IAvaloniaApplication application;
        readonly IFontManagerImpl impl;

        readonly Typeface _defaultTypeface =
           new("avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/Fonts#WenQuanYi Micro Hei");

        public static bool UseGdiPlusFirst { get; set; }

        public AvaloniaFontManagerImpl(IPlatformService platformService/*, IAvaloniaApplication application*/) : base(platformService)
        {
            //this.application = application;
            _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
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
            var fonts = impl.GetInstalledFontFamilyNames();
            var list = fonts.Select(x => KeyValuePair.Create(x, x)).ToList();
            list.Insert(0, IFontManager.Default);
            return list;
        }

        IFontManagerImpl IFontManagerImpl2.Impl => impl;

        string IFontManagerImpl2.DefaultFontFamilyName => _defaultFamilyName;

        internal static bool IsDefaultFontFamilyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            if (string.Equals(name, IFontManager.KEY_Default, StringComparison.OrdinalIgnoreCase)) return true;
            return name switch
            {
                FontFamily.DefaultFontFamilyName or "WenQuanYi Micro Hei" => true,
                _ => false,
            };
        }

        Typeface IFontManagerImpl2.OnCreateGlyphTypeface(Typeface typeface)
        {
            var name = typeface.FontFamily.Name;
            if (IsDefaultFontFamilyName(name)) return _defaultTypeface;
            return typeface;
        }

        //public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false) => application.RenderingSubsystemName switch
        //{
        //    Skia => GetInstalledFontFamilyNamesBySkia(checkForUpdates),
        //    Direct2D1 => GetInstalledFontFamilyNamesByDirect2D1(checkForUpdates),
        //    _ => throw new NotSupportedException(),
        //};

        //public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
        //  CultureInfo culture, out Typeface fontKey) => application.RenderingSubsystemName switch
        //  {
        //      Skia => TryMatchCharacterBySkia(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
        //      Direct2D1 => TryMatchCharacterByDirect2D1(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
        //      _ => throw new NotSupportedException(),
        //  };

        //public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface) => application.RenderingSubsystemName switch
        //{
        //    Skia => CreateGlyphTypefaceBySkia(typeface),
        //    Direct2D1 => CreateGlyphTypefaceByDirect2D1(typeface),
        //    _ => throw new NotSupportedException(),
        //};
    }
}