//using Avalonia.Direct2D1;
//using Avalonia.Direct2D1.Media;
//using Avalonia.Media;
//using Avalonia.Platform;
//using SharpDX.DirectWrite;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;
//using FontFamily = Avalonia.Media.FontFamily;
//using FontStyle = Avalonia.Media.FontStyle;
//using FontWeight = Avalonia.Media.FontWeight;

//// https://github.com/AvaloniaUI/Avalonia/blob/master/src/Windows/Avalonia.Direct2D1/Media/FontManagerImpl.cs

//namespace System.Application.Services.Implementation
//{
//    internal sealed class WindowsAvaloniaFontManagerImpl : AvaloniaFontManagerImpl
//    {
//        static readonly Lazy<FontCollection> _InstalledFontCollection = new(() =>
//        {
//            var type = typeof(GlyphTypefaceImpl).Assembly.GetType("Avalonia.Direct2D1.Media.Direct2D1FontCollectionCache");
//            if (type != null)
//            {
//                var field = type.GetField("InstalledFontCollection", BindingFlags.NonPublic | BindingFlags.Static);
//                if (field != null && field.FieldType == typeof(FontCollection))
//                {
//                    return (field.GetValue(null) as FontCollection)!;
//                }
//            }
//            return Direct2D1Platform.DirectWriteFactory.GetSystemFontCollection(false);
//        });
//        public static FontCollection InstalledFontCollection => _InstalledFontCollection.Value;

//        public WindowsAvaloniaFontManagerImpl(IDesktopPlatformService platformService, IDesktopAppService appService) : base(platformService, appService)
//        {
//        }

//        protected override IEnumerable<string> GetInstalledFontFamilyNamesByDirect2D1(bool checkForUpdates)
//        {
//            var familyCount = InstalledFontCollection.FontFamilyCount;

//            var fontFamilies = new string[familyCount];

//            for (var i = 0; i < familyCount; i++)
//            {
//                fontFamilies[i] = InstalledFontCollection.GetFontFamily(i).FamilyNames.GetString(0);
//            }

//            return fontFamilies;
//        }

//        protected override bool TryMatchCharacterByDirect2D1(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily, CultureInfo culture, out Typeface typeface)
//        {
//            var familyCount = InstalledFontCollection.FontFamilyCount;

//            for (var i = 0; i < familyCount; i++)
//            {
//                var font = InstalledFontCollection.GetFontFamily(i)
//                    .GetMatchingFonts((SharpDX.DirectWrite.FontWeight)fontWeight, FontStretch.Normal,
//                        (SharpDX.DirectWrite.FontStyle)fontStyle).GetFont(0);

//                if (!font.HasCharacter(codepoint))
//                {
//                    continue;
//                }

//                var fontFamilyName = font.FontFamily.FamilyNames.GetString(0);

//                typeface = new Typeface(fontFamilyName, fontStyle, fontWeight);

//                return true;
//            }

//            typeface = default;

//            return false;
//        }

//        protected override IGlyphTypefaceImpl CreateGlyphTypefaceByDirect2D1(Typeface typeface)
//        {
//            return new GlyphTypefaceImpl(typeface);
//        }
//    }
//}
