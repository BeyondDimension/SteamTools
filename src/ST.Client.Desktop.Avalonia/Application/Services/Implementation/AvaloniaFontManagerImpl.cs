using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Skia;
using SkiaSharp;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Application.Services.Implementation
{
    public class AvaloniaFontManagerImpl : FontManagerImpl, IFontManagerImpl
    {
        readonly string _defaultFamilyName;
        readonly IDesktopAppService appService;

        readonly Typeface _defaultTypeface =
           new("avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Fonts#WenQuanYi Micro Hei");

        public static bool UseGdiPlusFirst { get; set; }

        public AvaloniaFontManagerImpl(IDesktopPlatformService platformService, IDesktopAppService appService) : base(platformService)
        {
            this.appService = appService;
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

        public string GetDefaultFontFamilyName()
        {
            return _defaultFamilyName;
        }

        SKFontManager? _skFontManager;
        readonly Lazy<SKFontManager> lazy_skFontManager = new(() => SKFontManager.Default);

        public SKFontManager SKFontManager
        {
            get
            {
                if (_skFontManager == null)
                {
                    _skFontManager = lazy_skFontManager.Value;
                }
                return _skFontManager;
            }
            set
            {
                _skFontManager = value;
            }
        }

        protected const string Skia = "Skia";
        protected const string Direct2D1 = "Direct2D1";

        IEnumerable<string> GetInstalledFontFamilyNamesBySkia(bool checkForUpdates)
        {
            if (checkForUpdates)
            {
                SKFontManager = SKFontManager.CreateDefault();
            }

            return SKFontManager.FontFamilies;
        }

        protected virtual IEnumerable<string> GetInstalledFontFamilyNamesByDirect2D1(bool checkForUpdates) => throw new NotSupportedException();

        public IEnumerable<string> GetInstalledFontFamilyNames(bool checkForUpdates = false) => appService.RenderingSubsystemName switch
        {
            Skia => GetInstalledFontFamilyNamesBySkia(checkForUpdates),
            Direct2D1 => GetInstalledFontFamilyNamesByDirect2D1(checkForUpdates),
            _ => throw new NotSupportedException(),
        };

        [ThreadStatic] private static string[]? t_languageTagBuffer;

        bool TryMatchCharacterBySkia(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
         CultureInfo culture, out Typeface fontKey)
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
                        SKFontManager.MatchCharacter(familyNames[i], skFontStyle, t_languageTagBuffer, codepoint);

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
                var skTypeface = SKFontManager.MatchCharacter(null, skFontStyle, t_languageTagBuffer, codepoint);

                if (skTypeface != null)
                {
                    fontKey = new Typeface(skTypeface.FamilyName, fontStyle, fontWeight);

                    return true;
                }
            }

            fontKey = default;

            return false;
        }

        protected virtual bool TryMatchCharacterByDirect2D1(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
        CultureInfo culture, out Typeface fontKey) => throw new NotSupportedException();

        public bool TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily,
          CultureInfo culture, out Typeface fontKey) => appService.RenderingSubsystemName switch
          {
              Skia => TryMatchCharacterBySkia(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
              Direct2D1 => TryMatchCharacterByDirect2D1(codepoint, fontStyle, fontWeight, fontFamily, culture, out fontKey),
              _ => throw new NotSupportedException(),
          };

        SKTypeface GetSkTypefaceByFontFamily(Typeface typeface)
        {
            SKTypeface? skTypeface = null;

            if (typeface.FontFamily.Key == null)
            {
                var defaultName = SKTypeface.Default.FamilyName;
                var fontStyle = new SKFontStyle((SKFontStyleWeight)typeface.Weight, SKFontStyleWidth.Normal, (SKFontStyleSlant)typeface.Style);

                foreach (var familyName in typeface.FontFamily.FamilyNames)
                {
                    skTypeface = SKFontManager.MatchFamily(familyName, fontStyle);

                    if (skTypeface is null
                        || (!skTypeface.FamilyName.Equals(familyName, StringComparison.Ordinal)
                            && defaultName.Equals(skTypeface.FamilyName, StringComparison.Ordinal)))
                    {
                        continue;
                    }

                    break;
                }

                skTypeface ??= SKFontManager.MatchTypeface(SKTypeface.Default, fontStyle);
            }
            else
            {
                try
                {
                    Assembly asm = Assembly.Load("Avalonia.Skia");
                    var t = asm?.GetType("Avalonia.Skia.SKTypefaceCollectionCache");
                    var fontCollection = t?.InvokeMember("GetOrAddTypefaceCollection", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { typeface.FontFamily });
                    var type = fontCollection?.GetType();
                    var oMethod = type?.GetMethod("Get");
                    var r = oMethod?.Invoke(fontCollection, new object[] { typeface });
                    skTypeface = r as SKTypeface;
                }
                catch (Exception ex)
                {
                    Log.Error(nameof(AvaloniaFontManagerImpl), ex, nameof(GetSkTypefaceByFontFamily));
                    skTypeface = null;
                }
            }

            if (skTypeface == null)
            {
                skTypeface = SKTypeface.FromFamilyName(_defaultTypeface.FontFamily.Name);
            }

            return skTypeface;
        }

        IGlyphTypefaceImpl CreateGlyphTypefaceBySkia(Typeface typeface)
        {
            var skTypeface = typeface.FontFamily.Name switch
            {
                FontFamily.DefaultFontFamilyName or "WenQuanYi Micro Hei" => GetSkTypefaceByFontFamily(_defaultTypeface),
                _ => GetSkTypefaceByFontFamily(typeface),
            };
            if (skTypeface == null)
            {
                skTypeface = SKTypeface.Default;
            }

            return new GlyphTypefaceImpl(skTypeface);
        }

        protected virtual IGlyphTypefaceImpl CreateGlyphTypefaceByDirect2D1(Typeface typeface) => throw new NotSupportedException();

        public IGlyphTypefaceImpl CreateGlyphTypeface(Typeface typeface) => appService.RenderingSubsystemName switch
        {
            Skia => CreateGlyphTypefaceBySkia(typeface),
            Direct2D1 => CreateGlyphTypefaceByDirect2D1(typeface),
            _ => throw new NotSupportedException(),
        };
    }
}