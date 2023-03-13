namespace BD.WTTS.Services;

public interface IResourceService
{
    static IResourceService Instance => Ioc.Get<IResourceService>();

    CultureInfo Culture { get; set; }

    string? GetString(string name, CultureInfo? culture = null);

    CultureInfo DefaultCurrentUICulture { get; }

    bool IsChineseSimplified { get; }

    string Language { get; }

    string AcceptLanguage { get; }

    void ChangeAutoLanguage(CultureInfo? cultureInfo = null);

    void ChangeLanguage(string? cultureName);

    string GetCurrentCultureSteamLanguageName();

    IReadOnlyDictionary<string, string> Languages { get; }

    IReadOnlyDictionary<string, string> SteamLanguages { get; }

    IReadOnlyCollection<KeyValuePair<string, string>> Fonts { get; }
}
