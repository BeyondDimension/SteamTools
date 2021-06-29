using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
#if __MOBILE__
using R_Res_TYPE = System.Byte;
#else
using R_Res_TYPE = System.Application.UI.Resx.AppResources;
#endif

namespace System.Application.UI.Resx
{
    public sealed class R : ReactiveObject
    {
        public static R Current { get; } = new();

        public static readonly IReadOnlyCollection<KeyValuePair<string, string>> Languages;
        public static readonly Dictionary<string, string> SteamLanguages;
        static readonly Lazy<IReadOnlyCollection<KeyValuePair<string, string>>> mFonts = new(() => IFontManager.Instance.GetFonts());
        public static IReadOnlyCollection<KeyValuePair<string, string>> Fonts => mFonts.Value;

        public R_Res_TYPE Res { get; set; } = new();

        public static CultureInfo DefaultCurrentUICulture { get; }

        static R()
        {
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
                { "es", "Spanish" },
                { "it", "Italian" },
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
                { "es", "spanish" },
                { "it", "italian" },
            };
            AppResources.Culture = DefaultCurrentUICulture;
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
        public static void ChangeLanguage(string? cultureName)
        {
            if (cultureName == null || IsMatch(AppResources.Culture, cultureName)) return;
            AppResources.Culture = string.IsNullOrWhiteSpace(cultureName) ?
                DefaultCurrentUICulture :
                CultureInfo.GetCultureInfo(Languages.SingleOrDefault(x => x.Key == cultureName).Key);
            CultureInfo.CurrentUICulture = AppResources.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = AppResources.Culture;
            CultureInfo.CurrentCulture = AppResources.Culture;
            CultureInfo.DefaultThreadCurrentCulture = AppResources.Culture;
            mAcceptLanguage = GetAcceptLanguageCore();
            mLanguage = GetLanguageCore();
            Current.Res =
#if __MOBILE__
                ++Current.Res;
#else
                new AppResources();
#endif
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
            return "english";
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