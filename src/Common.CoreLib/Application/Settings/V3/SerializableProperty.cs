using DBreeze;
using System.ComponentModel;
using System.Diagnostics;
using static System.Application.Settings.ISerializableProperty;

namespace System.Application.Settings;

public interface ISerializableProperty : INotifyPropertyChanged
{
    public const string tableName = "SerializableProperty";
}

[DebuggerDisplay("Value={Value}, Key={Key}, Default={Default}")]
public sealed class SerializableProperty<T> : ISerializableProperty, INotifyPropertyChanged
{
    T? _value;
    bool _cached;
    DBreezeEngine? _Provider;

    public string Key { get; }

    public DBreezeEngine Provider { get => _Provider ?? throw new ArgumentNullException(nameof(_Provider)); set => _Provider = value; }

    public T? Default { get; }

    public T? Value
    {
        get
        {
            if (!_cached)
            {
                {
                    using var t = Provider.GetTransaction();
                    var row = t.Select<string, byte[]>(tableName, Key);
                    if (row?.Value == null)
                    {
                        _value = Default;
                    }
                    else
                    {
                        try
                        {
                            _value = Serializable.DMP<T>(row.Value);
                        }
                        catch
                        {
                            _value = Default;
                        }
                    }
                }
                _cached = true;
            }

            return _value;
        }

        set
        {
            var comparer = EqualityComparer<T?>.Default;
            if (_cached && comparer.Equals(_value, value)) return;
            var old = _value;
            _value = value;
            _cached = true;
            SetValue(comparer, old, value);
        }
    }

    void SetValue(EqualityComparer<T?> comparer, T? oldValue, T? newValue)
    {
        {
            using var t = Provider.GetTransaction();
            if (comparer.Equals(newValue, Default))
                t.RemoveKey(tableName, Key);
            else
                t.Insert(tableName, Key, Serializable.SMP(newValue));
            t.Commit();
        }
        OnValueChanged(oldValue, newValue);
    }

    public override string ToString() => Value?.ToString() ?? string.Empty;

    public void RaiseValueChanged()
    {
        var comparer = EqualityComparer<T?>.Default;
        SetValue(comparer, _value, _value);
    }

    public SerializableProperty(string key, DBreezeEngine provider) : this(key, provider, default)
    {
    }

    public SerializableProperty(string key, DBreezeEngine provider, T? defaultValue)
    {
        Key = key;
        Provider = provider;
        Default = defaultValue;
    }

    public IDisposable Subscribe(Action<T?> listener, bool notifyOnInitialValue = true)
    {
        if (notifyOnInitialValue)
            listener(Value);
        return new ValueChangedEventListener(this, listener);
    }

    public void Reset()
    {
        var comparer = EqualityComparer<T?>.Default;
        SetValue(comparer, _value, Default);
    }

    sealed class ValueChangedEventListener : IDisposable
    {
        private readonly Action<T?> _listener;
        private readonly SerializableProperty<T> _source;

        public ValueChangedEventListener(SerializableProperty<T> property, Action<T?> listener)
        {
            _listener = listener;
            _source = property;
            _source.ValueChanged += HandleValueChanged;
        }

        private void HandleValueChanged(object? sender, ValueChangedEventArgs<T?> args)
        {
            _listener(args.NewValue);
        }

        public void Dispose()
        {
            _source.ValueChanged -= HandleValueChanged;
        }
    }

    public static implicit operator T?(SerializableProperty<T> property) => property.Value;

    #region events

    public event EventHandler<ValueChangedEventArgs<T?>>? ValueChanged;

    void OnValueChanged(T? oldValue, T? newValue) => ValueChanged?.Invoke(this, new ValueChangedEventArgs<T?>(oldValue, newValue));

    readonly Dictionary<PropertyChangedEventHandler, EventHandler<ValueChangedEventArgs<T>>> _handlers = new();

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            if (value == null) return;
            ValueChanged += _handlers[value] = (sender, args) => value(sender, new PropertyChangedEventArgs(nameof(Value)));
        }

        remove
        {
            if (value == null) return;
            if (_handlers.TryGetValue(value, out var handler))
            {
                ValueChanged -= handler;
                _handlers.Remove(value);
            }
        }
    }

    #endregion
}