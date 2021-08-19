using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Application.UI
{
    public class CustomFontManagerImpl : IFontManagerImpl
    {
        //private readonly Typeface[] _customTypefaces;
        private SKFontManager _skFontManager = SKFontManager.Default;
        private readonly string _defaultFamilyName;

        //Load font resources in the project, you can load multiple font resources
        private readonly Typeface _defaultTypeface =
            new Typeface("resm:ST.Client.Desktop.Avalonia.Assets.Fonts.WenQuanYiMicroHei-01#WenQuanYi Micro Hei");

        public CustomFontManagerImpl()
        {
            //_customTypefaces = new[] { _defaultTypeface };
            _defaultFamilyName = _defaultTypeface.FontFamily.FamilyNames.PrimaryFamilyName;
        }

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false)
        {
            if (checkForUpdates)
            {
                _skFontManager = SKFontManager.CreateDefault();
            }

            return _skFontManager.FontFamilies;
            //return _customTypefaces.Select(x => x.FontFamily.Name);
        }

        private readonly string[] _bcp47 = { CultureInfo.CurrentCulture.ThreeLetterISOLanguageName, CultureInfo.CurrentCulture.TwoLetterISOLanguageName };

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
            CultureInfo culture, out Typeface typeface)
        {
            //foreach (var customTypeface in _customTypefaces)
            //{
            //    if (customTypeface.GlyphTypeface.GetGlyph((uint)codepoint) == 0)
            //    {
            //        continue;
            //    }

            //    typeface = new Typeface(customTypeface.FontFamily.Name, fontStyle, fontWeight);

            //    return true;
            //}

            var fallback = SKFontManager.Default.MatchCharacter(fontFamily?.Name, (SKFontStyleWeight)fontWeight,
                SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle, _bcp47, codepoint);

            typeface = new Typeface(fallback?.FamilyName ?? _defaultFamilyName, fontStyle, fontWeight);

            return true;
        }

        private SKTypeface GetSkTypefaceByFontFamily(Typeface typeface)
        {
            SKTypeface? skTypeface = SKTypeface.FromFamilyName(typeface.FontFamily.Name,
                          (SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);

            if (skTypeface.FamilyName != typeface.FontFamily.Name)
            {
                try
                {
                    Assembly asm = Assembly.Load("Avalonia.Skia");
                    var t = asm.GetType("Avalonia.Skia.SKTypefaceCollectionCache");
                    var fontCollection = t?.InvokeMember("GetOrAddTypefaceCollection", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { typeface.FontFamily });
                    var type = fontCollection?.GetType();
                    var oMethod = type?.GetMethod("Get");
                    var r = oMethod?.Invoke(fontCollection, new object[] { typeface });
                    skTypeface = r as SKTypeface;
                }
                catch
                {
                    //Log.Error(nameof(CustomFontManagerImpl), ex, nameof(GetSkTypefaceByFontFamily));
                    skTypeface = null;
                }
            }

            if (skTypeface == null)
            {
                skTypeface = SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name);
            }

            return skTypeface;
        }

        public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface)
        {
            SKTypeface? skTypeface = null;

            switch (typeface.FontFamily.Name)
            {
                case FontFamily.DefaultFontFamilyName:
                case "WenQuanYi Micro Hei":  //font family name
                    skTypeface = GetSkTypefaceByFontFamily(_defaultTypeface); break;
                default:
                    skTypeface = GetSkTypefaceByFontFamily(typeface);
                    break;
            }

            return new GlyphTypefaceImpl(skTypeface);
        }
    }
}
