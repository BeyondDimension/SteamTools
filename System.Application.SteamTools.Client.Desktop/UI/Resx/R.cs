using ReactiveUI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

namespace System.Application.UI.Resx
{
    public sealed class R : ReactiveObject
    {
        //static readonly Lazy<R> @this = new Lazy<R>(new R());

        //public static R Current => @this.Value;

        public static R Current { get; }

        public static readonly IList<KeyValuePair<string, string>> Languages;
        public static readonly Dictionary<string, string> SteamLanguages;

        public static CultureInfo DefaultCurrentUICulture { get; }

        static R()
        {
            Current = new R();
            DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
            Languages = new Dictionary<string, string>()
            {
                { "", "Auto" },
                { "zh-Hans", "Chinese(Simplified)" },
                { "zh-Hant", "Chinese(Traditional)" },
                { "en", "English" },
                { "ko", "Koreana" },
                { "ja", "Japanese" },
                { "ru", "Russian" },
            }.ToList();
            SteamLanguages = new Dictionary<string, string>()
            {
                { "zh-Hans", "schinese" },
                { "zh-Hant", "tchinese" },
                { "en","english"},
                { "ko", "koreana" },
                { "ja", "japanese" },
                { "ru", "russian" },
            };
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
            void ChangeLanguage()
            {
                if (IsMatch(CultureInfo.CurrentUICulture, cultureName)) return;
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                AppResources.Culture = CultureInfo.GetCultureInfo(cultureName);
                Current.Res = new AppResources();
                Current.RaisePropertyChanged(nameof(Res));
            }
            MainThreadDesktop.BeginInvokeOnMainThread(ChangeLanguage);
        }

        public static string GetCurrentCultureSteamLanguageName()
        {
            return AppResources.Culture == null ? Languages.First().Value : SteamLanguages[AppResources.Culture.Name];
        }

        //public AppResources Res => new AppResources();
        public AppResources Res { get; set; } = new AppResources();
    }
}