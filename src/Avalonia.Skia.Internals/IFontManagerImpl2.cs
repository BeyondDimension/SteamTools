// Replace font management to implement custom default font
extern alias AvaloniaSkia;

using Avalonia.Media;
using System.Collections.Generic;
using System.Globalization;
using FontManagerImpl = AvaloniaSkia::Avalonia.Skia.FontManagerImpl;

// ReSharper disable once CheckNamespace
namespace Avalonia.Platform
{
    public interface IFontManagerImpl2 : IFontManagerImpl
    {
        protected IFontManagerImpl Impl { get; }

        string DefaultFontFamilyName { get; }

        Typeface OnCreateGlyphTypeface(Typeface typeface);

        string IFontManagerImpl.GetDefaultFontFamilyName() => DefaultFontFamilyName;

        IEnumerable<string> IFontManagerImpl.GetInstalledFontFamilyNames(bool checkForUpdates)
        {
            return Impl.GetInstalledFontFamilyNames(checkForUpdates);
        }

        bool IFontManagerImpl.TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontFamily fontFamily, CultureInfo culture, out Typeface typeface)
        {
            return Impl.TryMatchCharacter(codepoint, fontStyle, fontWeight, fontFamily, culture, out typeface);
        }

        IGlyphTypefaceImpl IFontManagerImpl.CreateGlyphTypeface(Typeface typeface)
        {
            typeface = OnCreateGlyphTypeface(typeface);
            return Impl.CreateGlyphTypeface(typeface);
        }

        protected static IFontManagerImpl CreateFontManager() => new FontManagerImpl();
    }
}
