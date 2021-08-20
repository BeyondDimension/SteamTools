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
            new Typeface("avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Fonts#WenQuanYi Micro Hei");

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

        [ThreadStatic] private static string[] t_languageTagBuffer;

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
            CultureInfo culture, out Typeface fontKey)
        {
            SKFontStyle skFontStyle;

            switch (fontWeight)
            {
                case FontWeight.Normal when fontStyle == FontStyle.Normal:
                    skFontStyle = SKFontStyle.Normal;
                    break;
                case FontWeight.Normal when fontStyle == FontStyle.Italic:
                    skFontStyle = SKFontStyle.Italic;
                    break;
                case FontWeight.Bold when fontStyle == FontStyle.Normal:
                    skFontStyle = SKFontStyle.Bold;
                    break;
                case FontWeight.Bold when fontStyle == FontStyle.Italic:
                    skFontStyle = SKFontStyle.BoldItalic;
                    break;
                default:
                    skFontStyle = new SKFontStyle((SKFontStyleWeight)fontWeight, SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle);
                    break;
            }

            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }

            if (t_languageTagBuffer == null)
            {
                t_languageTagBuffer = new string[2];
            }

            t_languageTagBuffer[0] = culture.TwoLetterISOLanguageName;
            t_languageTagBuffer[1] = culture.ThreeLetterISOLanguageName;

            if (fontFamily != null && fontFamily.FamilyNames.HasFallbacks)
            {
                var familyNames = fontFamily.FamilyNames;

                for (var i = 1; i < familyNames.Count; i++)
                {
                    var skTypeface =
                        _skFontManager.MatchCharacter(familyNames[i], skFontStyle, t_languageTagBuffer, codepoint);

                    if (skTypeface == null)
                    {
                        continue;
                    }

                    fontKey = new Typeface(skTypeface.FamilyName, fontStyle, fontWeight);

                    return true;
                }
            }
            else
            {
                var skTypeface = _skFontManager.MatchCharacter(null, skFontStyle, t_languageTagBuffer, codepoint);

                if (skTypeface != null)
                {
                    fontKey = new Typeface(skTypeface.FamilyName, fontStyle, fontWeight);

                    return true;
                }
            }

            fontKey = default;

            return false;
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
