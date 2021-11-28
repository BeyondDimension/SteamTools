using Avalonia.Media;
using System.Application.UI.Resx;

namespace System.Application.Services.Implementation
{
    partial class AvaloniaFontManagerImpl
    {
        static readonly Lazy<FontFamily> mDefault = new(() => new FontFamily(IPlatformService.Instance.GetDefaultFontFamily()));
        public static FontFamily Default => mDefault.Value;

        static readonly Lazy<FontFamily> mDefaultConsole = new(() =>
        {
            var fontName = R.DefaultConsoleFont;
            if (fontName == IFontManager.KEY_Default) return mDefault.Value;
            return new(fontName);
        });
        public static FontFamily DefaultConsole => mDefaultConsole.Value;
    }
}
