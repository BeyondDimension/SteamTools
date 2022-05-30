namespace System.Application.Services.Implementation;

sealed class PreferencesPlatformServiceImpl : IPreferencesPlatformService
{
    void IPreferencesPlatformService.PlatformClear(string? sharedName)
        => Preferences.Clear(sharedName);

    bool IPreferencesPlatformService.PlatformContainsKey(string key, string? sharedName)
        => Preferences.ContainsKey(key, sharedName);

    string? IPreferencesPlatformService.PlatformGet(string key, string? defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    bool IPreferencesPlatformService.PlatformGet(string key, bool defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    DateTime IPreferencesPlatformService.PlatformGet(string key, DateTime defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    double IPreferencesPlatformService.PlatformGet(string key, double defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    int IPreferencesPlatformService.PlatformGet(string key, int defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    long IPreferencesPlatformService.PlatformGet(string key, long defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    float IPreferencesPlatformService.PlatformGet(string key, float defaultValue, string? sharedName)
        => Preferences.Get(key, defaultValue, sharedName);

    void IPreferencesPlatformService.PlatformRemove(string key, string? sharedName)
        => Preferences.Remove(key, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, string? value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, bool value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, DateTime value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, double value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, int value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, long value, string? sharedName)
        => Preferences.Set(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, float value, string? sharedName)
        => Preferences.Set(key, value, sharedName);
}
