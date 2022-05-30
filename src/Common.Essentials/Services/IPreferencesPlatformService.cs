namespace System.Application.Services;

public interface IPreferencesPlatformService
{
    static IPreferencesPlatformService Instance => DI.Get<IPreferencesPlatformService>();

    bool PlatformContainsKey(string key, string? sharedName);

    void PlatformRemove(string key, string? sharedName);

    void PlatformClear(string? sharedName);

    string? PlatformGet(string key, string? defaultValue, string? sharedName);

    bool PlatformGet(string key, bool defaultValue, string? sharedName);

    DateTime PlatformGet(string key, DateTime defaultValue, string? sharedName);

    double PlatformGet(string key, double defaultValue, string? sharedName);

    int PlatformGet(string key, int defaultValue, string? sharedName);

    long PlatformGet(string key, long defaultValue, string? sharedName);

    float PlatformGet(string key, float defaultValue, string? sharedName);

    void PlatformSet(string key, string? value, string? sharedName);

    void PlatformSet(string key, bool value, string? sharedName);

    void PlatformSet(string key, DateTime value, string? sharedName);

    void PlatformSet(string key, double value, string? sharedName);

    void PlatformSet(string key, int value, string? sharedName);

    void PlatformSet(string key, long value, string? sharedName);

    void PlatformSet(string key, float value, string? sharedName);
}

public interface IPreferencesGenericPlatformService : IPreferencesPlatformService
{
    T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible;

    void PlatformSet<T>(string key, T? value, string? sharedName) where T : notnull, IConvertible;

    string? IPreferencesPlatformService.PlatformGet(string key, string? defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    bool IPreferencesPlatformService.PlatformGet(string key, bool defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    DateTime IPreferencesPlatformService.PlatformGet(string key, DateTime defaultValue, string? sharedName) => DateTime.FromBinary(PlatformGet(key, defaultValue.ToBinary(), sharedName));

    double IPreferencesPlatformService.PlatformGet(string key, double defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    int IPreferencesPlatformService.PlatformGet(string key, int defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    long IPreferencesPlatformService.PlatformGet(string key, long defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    float IPreferencesPlatformService.PlatformGet(string key, float defaultValue, string? sharedName) => PlatformGet(key, defaultValue, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, string? value, string? sharedName) => PlatformSet(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, bool value, string? sharedName) => PlatformSet(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, DateTime value, string? sharedName) => PlatformSet(key, value.ToBinary(), sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, double value, string? sharedName) => PlatformSet(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, int value, string? sharedName) => PlatformSet(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, long value, string? sharedName) => PlatformSet(key, value, sharedName);

    void IPreferencesPlatformService.PlatformSet(string key, float value, string? sharedName) => PlatformSet(key, value, sharedName);
}