// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

/// <summary>Creates trimmed-safe open instance delegates once per settings property.</summary>
internal static class SettingsPropertyAccessors<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings, TValue>
{
    static readonly ConcurrentDictionary<string, (Func<TSettings, TValue> Getter, Action<TSettings, TValue> Setter)> cache = new(StringComparer.Ordinal);

    internal static (Func<TSettings, TValue> Getter, Action<TSettings, TValue> Setter) Get(string propertyName)
        => cache.GetOrAdd(propertyName, static name =>
        {
            var property = typeof(TSettings).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
                ?? throw new MissingMemberException(typeof(TSettings).FullName, name);
            var getter = property.GetMethod
                ?? throw new MissingMethodException(typeof(TSettings).FullName, $"get_{name}");
            var setter = property.SetMethod
                ?? throw new MissingMethodException(typeof(TSettings).FullName, $"set_{name}");
            return (getter.CreateDelegate<Func<TSettings, TValue>>(), setter.CreateDelegate<Action<TSettings, TValue>>());
        });
}
