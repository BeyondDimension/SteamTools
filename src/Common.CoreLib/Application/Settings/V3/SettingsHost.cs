using System.Runtime.CompilerServices;

namespace System.Application.Settings;

public static class SettingsHost
{
    public static void Load() => SettingsProviderV3.Load();

    [Obsolete]
    public static void Save()
    {

    }
}

public abstract class SettingsHost2<TSettings> where TSettings : SettingsHost2<TSettings>, new()
{
    static readonly string CategoryName = typeof(TSettings).Name;

    static string GetKey(string propertyName) => $"{CategoryName}.{propertyName}";

    public static SerializableProperty<T> GetProperty<T>(T? defaultValue, [CallerMemberName] string propertyName = "") => new(GetKey(propertyName), SettingsProviderV3.Provider, defaultValue);

    [Obsolete("autoSave N/A")]
    public static SerializableProperty<T> GetProperty<T>(T? defaultValue, bool autoSave = true, [CallerMemberName] string propertyName = "") => new(GetKey(propertyName), SettingsProviderV3.Provider, defaultValue);
}
