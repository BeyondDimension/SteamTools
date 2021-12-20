using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;

namespace System.Application.Services.Implementation
{
    partial class AvaloniaFontManagerImpl : IFontManagerImpl
    {
        // https://github.com/AvaloniaUI/Avalonia/blob/0.10.10/src/Skia/Avalonia.Skia/FontManagerImpl.cs

        protected const string Skia = "Skia";

        private SKFontManager _skFontManager = SKFontManager.Default;

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        IEnumerable<string> GetInstalledFontFamilyNamesBySkia(bool checkForUpdates)
        {
            if (checkForUpdates)
            {
                _skFontManager = SKFontManager.CreateDefault();
            }

            return _skFontManager.FontFamilies;
        }

        [ThreadStatic] private static string[]? t_languageTagBuffer;

        bool TryMatchCharacterBySkia(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily, CultureInfo culture, out Typeface fontKey)
        {
            var skFontStyle = fontWeight switch
            {
                FontWeight.Normal when fontStyle == FontStyle.Normal => SKFontStyle.Normal,
                FontWeight.Normal when fontStyle == FontStyle.Italic => SKFontStyle.Italic,
                FontWeight.Bold when fontStyle == FontStyle.Normal => SKFontStyle.Bold,
                FontWeight.Bold when fontStyle == FontStyle.Italic => SKFontStyle.BoldItalic,
                _ => new SKFontStyle((SKFontStyleWeight)fontWeight, SKFontStyleWidth.Normal, (SKFontStyleSlant)fontStyle),
            };
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

        IGlyphTypefaceImpl CreateGlyphTypefaceBySkia(Typeface typeface)
        {
            switch (typeface.FontFamily.Name)
            {
                case FontFamily.DefaultFontFamilyName or "WenQuanYi Micro Hei" or IFontManager.KEY_Default:
                    typeface = _defaultTypeface;
                    break;
            }

            SKTypeface? skTypeface = null;

            if (typeface.FontFamily.Key == null)
            {
                var defaultName = SKTypeface.Default.FamilyName;
                var fontStyle = new SKFontStyle((SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);

                foreach (var familyName in typeface.FontFamily.FamilyNames)
                {
                    skTypeface = _skFontManager.MatchFamily(familyName, fontStyle);

                    if (skTypeface is null
                        || (!skTypeface.FamilyName.Equals(familyName, StringComparison.Ordinal)
                            && defaultName.Equals(skTypeface.FamilyName, StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    break;
                }

                skTypeface ??= _skFontManager.MatchTypeface(SKTypeface.Default, fontStyle);
            }
            else
            {
                var fontCollection = SKTypefaceCollectionCache.GetOrAddTypefaceCollection(typeface.FontFamily);

                skTypeface = fontCollection.Get(typeface);
            }

            if (skTypeface == null)
            {
                throw new InvalidOperationException(
                    $"Could not create glyph typeface for: {typeface.FontFamily.Name}.");
            }

            var isFakeBold = (int)typeface.Weight >= 600 && !skTypeface.IsBold;

            var isFakeItalic = typeface.Style == FontStyle.Italic && !skTypeface.IsItalic;

            return new GlyphTypefaceImpl(skTypeface, isFakeBold, isFakeItalic);
        }
    }
}
