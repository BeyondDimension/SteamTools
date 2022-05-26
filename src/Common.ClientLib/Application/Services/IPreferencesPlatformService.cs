namespace System.Application.Services;

public interface IPreferencesPlatformService
{
    static IPreferencesPlatformService Instance => DI.Get<IPreferencesPlatformService>();

    bool PlatformContainsKey(string key, string? sharedName);

    void PlatformRemove(string key, string? sharedName);

    void PlatformClear(string? sharedName);

    T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible;

    void PlatformSet<T>(string key, T? value, string? sharedName) where T : notnull, IConvertible;
}
