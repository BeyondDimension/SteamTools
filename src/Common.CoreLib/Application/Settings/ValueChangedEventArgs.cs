namespace System.Application.Settings;

public sealed class ValueChangedEventArgs<T> : EventArgs where T : notnull
{
    public T? OldValue { get; }

    public T? NewValue { get; }

    public ValueChangedEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}