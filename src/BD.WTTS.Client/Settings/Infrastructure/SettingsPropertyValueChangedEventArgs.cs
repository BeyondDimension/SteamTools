// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

public sealed class SettingsPropertyValueChangedEventArgs<TValue> : EventArgs
{
    public TValue? OldValue { get; }

    public TValue? NewValue { get; }

    public SettingsPropertyValueChangedEventArgs(TValue? oldValue, TValue? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}