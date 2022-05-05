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

        static readonly Typeface _defaultTypeface =
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
            if (string.Equals(name, IFontManager.KEY_Default, StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(name, IFontManager.KEY_WinUI, StringComparison.OrdinalIgnoreCase))
                return true;
            if (_defaultTypeface.FontFamily.FamilyNames.Contains(name))
                return true;
            return name switch
            {
                FontFamily.DefaultFontFamilyName => true,
                _ => false,
            };
        }

        Typeface IFontManagerImpl2.OnCreateGlyphTypeface(Typeface typeface)
        {
            var name = typeface.FontFamily.Name;
            if (IsDefaultFontFamilyName(name)) return _defaultTypeface;
            return typeface;
        }
    }
}