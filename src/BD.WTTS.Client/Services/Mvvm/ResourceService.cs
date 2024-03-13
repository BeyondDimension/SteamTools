using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public sealed class ResourceService : ReactiveObject
{
    static readonly Lazy<ResourceService> mCurrent = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);

    public static ResourceService Current => mCurrent.Value;

    public static readonly IReadOnlyDictionary<string, string> Languages;
    public static readonly IReadOnlyDictionary<string, string> SteamLanguages;
    static readonly Lazy<IReadOnlyCollection<KeyValuePair<string, string>>> mFonts = new(GetFonts);

    public static KeyValuePair<string, string> GetSelectLanguage()
    {
        var value = UISettings.Language.Value;
        foreach (var item in Languages)
        {
            if (item.Value == value)
            {
                return item;
            }
        }
        return Languages.FirstOrDefault();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static IReadOnlyCollection<KeyValuePair<string, string>> GetFonts()
    {
        var fonts = IFontManager.Instance.GetFonts();
        return fonts;
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> Fonts => mFonts.Value;

    public static KeyValuePair<string, string> GetSelectFont()
    {
        var value = UISettings.FontName.Value;
        foreach (var item in mFonts.Value)
        {
            if (item.Value == value)
            {
                return item;
            }
        }
        return mFonts.Value.FirstOrDefault();
    }

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

    public static bool IsChineseSimplified => Culture.IsMatch(AssemblyInfo.CultureName_SimplifiedChinese);

    static ResourceService()
    {
        DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
        Languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "", "Auto" },
            { AssemblyInfo.CultureName_SimplifiedChinese, "Chinese (Simplified)" },
            { AssemblyInfo.CultureName_TraditionalChinese, "Chinese (Traditional)" },
            { AssemblyInfo.CultureName_English, "English" },
            { AssemblyInfo.CultureName_Korean, "Korean" },
            { AssemblyInfo.CultureName_Japanese, "Japanese" },
            { AssemblyInfo.CultureName_Russian, "Russian" },
            { AssemblyInfo.CultureName_Spanish, "Spanish" },
            { AssemblyInfo.CultureName_Italian, "Italian" },
        };

        SteamLanguages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { AssemblyInfo.CultureName_PRC, "schinese" },
            { AssemblyInfo.CultureName_SimplifiedChinese, "schinese" },
            { AssemblyInfo.CultureName_TraditionalChinese, "tchinese" },
            { AssemblyInfo.CultureName_English, "english" },
            { AssemblyInfo.CultureName_Korean, "koreana" },
            { AssemblyInfo.CultureName_Japanese, "japanese" },
            { AssemblyInfo.CultureName_Russian, "russian" },
            { AssemblyInfo.CultureName_Spanish, "spanish" },
            { AssemblyInfo.CultureName_Italian, "italian" },
        };
        AppResources.Culture = DefaultCurrentUICulture;
    }

    public static void ChangeAutoLanguage(CultureInfo? cultureInfo = null) => MainThread2.BeginInvokeOnMainThread(() => ChangeAutoLanguageCore(cultureInfo));

    static void ChangeAutoLanguageCore(CultureInfo? cultureInfo = null)
    {
        if (!string.IsNullOrWhiteSpace(SettingsPageViewModel.SelectLanguageKey))
            return;
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
        if (cultureName == null || AppResources.Culture.IsMatch(cultureName)) return;
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
                if (culture.IsMatch(item.Key))
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
            Log.Error(nameof(ResourceService), ex, nameof(GetCurrentCultureSteamLanguageName));
        }
        return SteamLanguages[AssemblyInfo.CultureName_SimplifiedChinese];
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
            mAcceptLanguage ??= GetAcceptLanguageCore();
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
                if (culture.IsMatch(item.Key))
                {
                    return item.Key;
                }
            }
        }
        return AssemblyInfo.CultureName_English;
    }

    public static string Language
    {
        get
        {
            mLanguage ??= GetLanguageCore();
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
