using ReactiveUI;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace System.Application.UI.Resx
{
    public sealed class R : ReactiveObject
    {
        static R? mCurrent;
        public static R Current => mCurrent ?? new();

        public R()
        {
            mCurrent = this;
        }

        public static readonly IReadOnlyDictionary<string, string> Languages;
        public static readonly IReadOnlyDictionary<string, string> SteamLanguages;
        static readonly Lazy<IReadOnlyCollection<KeyValuePair<string, string>>> mFonts = new(() => IFontManager.Instance.GetFonts());
        public static IReadOnlyCollection<KeyValuePair<string, string>> Fonts => mFonts.Value;

        //static readonly Lazy<string> mDefaultConsoleFont = new(() =>
        //{
        //    var consoleFonts = new[] {
        //        IFontManager.ConsoleFont_JetBrainsMono,
        //        IFontManager.ConsoleFont_CascadiaCode,
        //        IFontManager.ConsoleFont_SourceCodePro,
        //        IFontManager.ConsoleFont_Consolas,
        //    };
        //    var intersect = Fonts.Select(x => x.Value).Intersect(consoleFonts);
        //    return intersect.FirstOrDefault() ?? IFontManager.KEY_Default;
        //});
        //public static string DefaultConsoleFont => mDefaultConsoleFont.Value;

        public AppResources Res { get; private set; } = new();

        public static CultureInfo DefaultCurrentUICulture { get; private set; }

        public static bool IsChineseSimplified => IsMatch(Culture, zh_Hans);

        const string zh_Hans = "zh-Hans";

        static R()
        {
            DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
            Languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "", "Auto" },
                { zh_Hans, "Chinese (Simplified)" },
                { "zh-Hant", "Chinese (Traditional)" },
                { "en", "English" },
                { "ko", "Korean" },
                { "ja", "Japanese" },
                { "ru", "Russian" },
                { "es", "Spanish" },
                { "it", "Italian" },
            };

            SteamLanguages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "zh-CN", "schinese" },
                { zh_Hans, "schinese" },
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

        public static void ChangeAutoLanguage(CultureInfo? cultureInfo = null) => MainThread2.BeginInvokeOnMainThread(() => ChangeAutoLanguageCore(cultureInfo));

        static void ChangeAutoLanguageCore(CultureInfo? cultureInfo = null)
        {
            if (!string.IsNullOrWhiteSpace(SettingsPageViewModel.Instance.SelectLanguage.Key)) return;
            cultureInfo ??= CultureInfo.CurrentUICulture;
            DefaultCurrentUICulture = cultureInfo;
            ChangeLanguageCore(cultureInfo.Name);
        }

        /// <summary>
        /// 更改语言
        /// </summary>
        /// <param name="cultureName"></param>
        public static void ChangeLanguage(string? cultureName) => MainThread2.BeginInvokeOnMainThread(() => ChangeLanguageCore(cultureName));

        static CultureInfo TryGetCultureInfo(string? cultureName, CultureInfo defaultCultureInfo)
        {
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                return defaultCultureInfo;
            }
            try
            {
                return CultureInfo.GetCultureInfo(cultureName);
            }
            catch
            {
                return defaultCultureInfo;
            }
        }

        static void ChangeLanguageCore(string? cultureName)
        {
            if (cultureName == null || IsMatch(AppResources.Culture, cultureName)) return;
            AppResources.Culture = TryGetCultureInfo(cultureName, DefaultCurrentUICulture);
            Thread.CurrentThread.CurrentUICulture = AppResources.Culture;
            Thread.CurrentThread.CurrentCulture = AppResources.Culture;
            CultureInfo.DefaultThreadCurrentUICulture = AppResources.Culture;
            CultureInfo.DefaultThreadCurrentCulture = AppResources.Culture;
            CultureInfo.CurrentUICulture = AppResources.Culture;
            CultureInfo.CurrentCulture = AppResources.Culture;
            mAcceptLanguage = GetAcceptLanguageCore();
            mLanguage = GetLanguageCore();
            Current.Res = new();
            Current.RaisePropertyChanged(nameof(Res));
            ChangedHandler?.Invoke();
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
            return SteamLanguages[zh_Hans];
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
            foreach (var item in Languages)
            {
                if (!string.IsNullOrWhiteSpace(item.Key))
                {
                    if (IsMatch(culture, item.Key))
                    {
                        return item.Key;
                    }
                }
            }
            return "en";
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

        static event Action? ChangedHandler;

        sealed class ChangedEventListener : IDisposable
        {
            readonly Action _listener;
            public ChangedEventListener(Action listener)
            {
                _listener = listener;
                ChangedHandler += Handle;
            }

            void Handle() => _listener.Invoke();

            public void Dispose()
            {
                ChangedHandler -= Handle;
            }
        }

        public static IDisposable Subscribe(Action action, bool notifyOnInitialValue = true)
        {
            if (notifyOnInitialValue) action();
            return new ChangedEventListener(action);
        }
    }
}