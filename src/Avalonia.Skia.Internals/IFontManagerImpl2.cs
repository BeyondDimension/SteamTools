// Replace font management to implement custom default font
extern alias AvaloniaSkia;

using Avalonia.Media;
using FontManagerImpl = AvaloniaSkia::Avalonia.Skia.FontManagerImpl;

// ReSharper disable once CheckNamespace
namespace Avalonia.Platform;

public interface IFontManagerImpl2 : IFontManagerImpl
{
    protected IFontManagerImpl Impl { get; }

    string DefaultFontFamilyName { get; }

    string OnCreateGlyphTypeface(string familyName);

    string IFontManagerImpl.GetDefaultFontFamilyName() => DefaultFontFamilyName;

    string[] IFontManagerImpl.GetInstalledFontFamilyNames(bool checkForUpdates)
    {
        return Impl.GetInstalledFontFamilyNames(checkForUpdates);
    }

    new string[] GetInstalledFontFamilyNames(bool checkForUpdates)
    {
        return Impl.GetInstalledFontFamilyNames(checkForUpdates);
    }

    bool IFontManagerImpl.TryMatchCharacter(int codepoint, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch, CultureInfo? culture, out Typeface typeface)
    {
        return Impl.TryMatchCharacter(codepoint, fontStyle, fontWeight, fontStretch, culture, out typeface);
    }

    bool IFontManagerImpl.TryCreateGlyphTypeface(string familyName, FontStyle style, FontWeight weight, FontStretch stretch, [NotNullWhen(returnValue: true)] out IGlyphTypeface? glyphTypeface)
    {
        familyName = OnCreateGlyphTypeface(familyName);
        return Impl.TryCreateGlyphTypeface(familyName, style, weight, stretch, out glyphTypeface);
    }

    bool IFontManagerImpl.TryCreateGlyphTypeface(Stream stream, FontSimulations fontSimulations, [NotNullWhen(returnValue: true)] out IGlyphTypeface? glyphTypeface)
    {
        return Impl.TryCreateGlyphTypeface(stream, fontSimulations, out glyphTypeface);
    }

    //IGlyphTypeface IFontManagerImpl.CreateGlyphTypeface(Typeface typeface)
    //{
    //    typeface = OnCreateGlyphTypeface(typeface);
    //    return Impl.CreateGlyphTypeface(typeface);
    //}

    protected static IFontManagerImpl CreateFontManager() => new FontManagerImpl();
}
