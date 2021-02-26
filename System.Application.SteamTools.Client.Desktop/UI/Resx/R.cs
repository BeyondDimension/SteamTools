using ReactiveUI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Application.UI.Resx
{
    public sealed class R : ReactiveObject
    {
        //static readonly Lazy<R> @this = new Lazy<R>(new R());

        //public static R Current => @this.Value;

        public static R Current { get; }

        public static readonly IList<KeyValuePair<string, string>> Languages;
        public static readonly IList<KeyValuePair<string, string>> SteamLanguages;

        public static CultureInfo DefaultCurrentUICulture { get; }

        static R()
        {
            Current = new R();
            DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
            Languages = new Dictionary<string, string>()
            {
                { "", "Auto" },
                { "en", "English" },
                { "zh-Hans", "Chinese(Simplified)" },
                { "zh-Hant", "Chinese(Traditional)" },
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
            }.ToList();
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
                CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
                AppResources.Culture = CultureInfo.CurrentUICulture;
                Current.RaisePropertyChanged(nameof(Res));
            }
            MainThreadDesktop.BeginInvokeOnMainThread(ChangeLanguage);
        }

        R()
        {
            Res = new AppResources();
        }

        public AppResources Res { get; }
    }
}