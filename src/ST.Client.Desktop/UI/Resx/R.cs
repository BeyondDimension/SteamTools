using ReactiveUI;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

namespace System.Application.UI.Resx
{
    public sealed class R : ReactiveObject
    {
        public static R Current { get; }

        public static readonly IList<KeyValuePair<string, string>> Languages;
        public static readonly Dictionary<string, string> SteamLanguages;
        public static readonly IList<KeyValuePair<string, FontFamily?>> Fonts;

        public AppResources Res { get; set; } = new AppResources();

        public static CultureInfo DefaultCurrentUICulture { get; }

        static R()
        {
            Current = new R();
            DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
            Languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "", "Auto" },
                { "zh-Hans", "Chinese(Simplified)" },
                { "zh-Hant", "Chinese(Traditional)" },
                { "en", "English" },
                { "ko", "Koreana" },
                { "ja", "Japanese" },
                { "ru", "Russian" },
            }.ToList();
            SteamLanguages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "zh-CN", "schinese" },
                { "zh-Hans", "schinese" },
                { "zh-Hant", "tchinese" },
                { "en","english"},
                { "ko", "koreana" },
                { "ja", "japanese" },
                { "ru", "russian" },
            };
            Fonts = GetFonts();
            AppResources.Culture = DefaultCurrentUICulture;
        }

        static IList<KeyValuePair<string, FontFamily?>> GetFonts()
        {
            var culture = Culture;
            InstalledFontCollection ifc = new();
            var list = ifc.Families.Select(x =>
                KeyValuePair.Create(x.GetName(culture.LCID), (FontFamily?)x)).ToList();
            list.Insert(0, KeyValuePair.Create("Default", (FontFamily?)null));
            return list;
        }

        static bool IsMatch(CultureInfo cultureInfo, string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureInfo.Name))
            {
                return false;
            }
            if (cultureInfo.Name == cultureName)
            {
                return true;
            }
            else
            {
                return IsMatch(cultureInfo.Parent, cultureName);
            }
        }

        /// <summary>
        /// 更改语言
        /// </summary>
        /// <param name="cultureName"></param>
        public static void ChangeLanguage(string cultureName)
        {
            if (IsMatch(AppResources.Culture, cultureName)) return;
            AppResources.Culture = string.IsNullOrWhiteSpace(cultureName) ?
                DefaultCurrentUICulture :
                CultureInfo.GetCultureInfo(Languages.SingleOrDefault(x => x.Key == cultureName).Key);
            CultureInfo.CurrentUICulture = AppResources.Culture;
            mAcceptLanguage = GetAcceptLanguageCore();
            mLanguage = GetLanguageCore();
            Current.Res = new AppResources();
            Current.RaisePropertyChanged(nameof(Res));
        }

        public static string GetCurrentCultureSteamLanguageName()
        {
            try
            {
                var culture = Culture;
                foreach (var item in SteamLanguages)
                {
                    if (IsMatch(culture, item.Key))
                    {
                        return item.Value;
                    }
                }
                //return AppResources.Culture == null ?
                //    Languages.First().Key :
                //    SteamLanguages[AppResources.Culture.Name];
            }
            catch (Exception ex)
            {
                Log.Error(nameof(R), ex, nameof(GetCurrentCultureSteamLanguageName));
            }
            return SteamLanguages["en"];
        }

        static string GetAcceptLanguageCore()
        {
            var culture = Culture;
            return culture.GetAcceptLanguage();
        }

        static string? mAcceptLanguage;

        public static string AcceptLanguage
        {
            get
            {
                if (mAcceptLanguage == null)
                {
                    mAcceptLanguage = GetAcceptLanguageCore();
                }
                return mAcceptLanguage;
            }
        }

        static string? mLanguage;

        static string GetLanguageCore()
        {
            var culture = Culture;
            if (IsMatch(culture, "zh-Hans"))
            {
                return "zh-Hans";
            }
            else if (IsMatch(culture, "zh-Hant"))
            {
                return "zh-Hant";
            }
            else if (IsMatch(culture, "ko"))
            {
                return "ko";
            }
            else if (IsMatch(culture, "ja"))
            {
                return "ja";
            }
            else if (IsMatch(culture, "ru"))
            {
                return "ru";
            }
            else
            {
                return "en";
            }
        }

        public static string Language
        {
            get
            {
                if (mLanguage == null)
                {
                    mLanguage = GetLanguageCore();
                }
                return mLanguage;
            }
        }

        public static CultureInfo Culture => AppResources.Culture ?? DefaultCurrentUICulture;
    }
}