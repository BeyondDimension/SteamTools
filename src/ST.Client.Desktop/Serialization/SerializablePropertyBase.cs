using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Application.Serialization
{
    [DebuggerDisplay("Value={Value}, Key={Key}, Default={Default}")]
    public abstract class SerializablePropertyBase<T> : INotifyPropertyChanged where T : notnull
    {
        private T? _value;
        private bool _cached;

        public string Key { get; }

        public ISerializationProvider Provider { get; }

        public bool AutoSave { get; set; }

        public T? Default { get; }

        public virtual T? Value
        {
            get
            {
                if (_cached) return _value;

                if (!Provider.IsLoaded)
                {
                    Provider.Load();
                }

                if (Provider.TryGetValue(Key, out object? obj))
                {
                    _value = DeserializeCore(obj);
                    _cached = true;
                }
                else
                {
                    _value = Default;
                }

                return _cached ? _value : Default;
            }
            set
            {
                if (_cached && Equals(_value, value)) return;

                if (!Provider.IsLoaded)
                {
                    Provider.Load();
                }

                var old = _value;
                _value = value;
                _cached = true;
                Provider.SetValue(Key, value);
                OnValueChanged(old, value);

                if (AutoSave) Provider.Save();
            }
        }

        public virtual void RaiseValueChanged()
        {
            Provider.SetValue(Key, _value);
            OnValueChanged(_value, _value);
            if (AutoSave) Provider.Save();
        }

        protected SerializablePropertyBase(string key, ISerializationProvider provider) : this(key, provider, default)
        {
        }

        protected SerializablePropertyBase(string key, ISerializationProvider provider, T? defaultValue)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Default = defaultValue;

            Provider.Reloaded += (sender, args) =>
            {
                if (_cached)
                {
                    _cached = false;

                    var oldValue = _value;
                    var newValue = Value;
                    if (!Equals(oldValue, newValue))
                    {
                        OnValueChanged(oldValue, newValue);
                    }
                }
                else
                {
                    var oldValue = default(T);
                    var newValue = Value;
                    OnValueChanged(oldValue, newValue);
                }
            };
        }

        protected virtual T? DeserializeCore(object value)
        {
            if (typeof(T).IsValueType || typeof(T) == typeof(string))
            {
                return (T)value;
            }
            var temp = Serializable.SMP(value);
            return Serializable.DMP<T>(temp);
        }

        public virtual IDisposable Subscribe(Action<T?> listener)
        {
            listener(Value);
            return new ValueChangedEventListener(this, listener);
        }

        public virtual void Reset()
        {
            if (!Provider.IsLoaded)
            {
                Provider.Load();
            }

            if (Provider.TryGetValue(Key, out object? old))
            {
                if (Provider.RemoveValue(Key))
                {
                    _value = default;
                    _cached = false;
                    OnValueChanged(DeserializeCore(old), Default);

                    if (AutoSave) Provider.Save();
                }
            }
        }

        private class ValueChangedEventListener : IDisposable
        {
            private readonly Action<T?> _listener;
            private readonly SerializablePropertyBase<T> _source;

            public ValueChangedEventListener(SerializablePropertyBase<T> property, Action<T?> listener)
            {
                _listener = listener;
                _source = property;
                _source.ValueChanged += HandleValueChanged;
            }

            private void HandleValueChanged(object sender, ValueChangedEventArgs<T> args)
            {
                _listener(args.NewValue);
            }

            public void Dispose()
            {
                _source.ValueChanged -= HandleValueChanged;
            }
        }

        public static implicit operator T?(SerializablePropertyBase<T> property)
        {
            return property.Value;
        }

        #region events

        public event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;

        protected virtual void OnValueChanged(T? oldValue, T? newValue)
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, newValue));
        }

        private readonly Dictionary<PropertyChangedEventHandler, EventHandler<ValueChangedEventArgs<T>>> _handlers = new();

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { ValueChanged += (_handlers[value] = (sender, args) => value(sender, new PropertyChangedEventArgs(nameof(Value)))); }
            remove
            {
                if (_handlers.TryGetValue(value, out EventHandler<ValueChangedEventArgs<T>> handler))
                {
                    ValueChanged -= handler;
                    _handlers.Remove(value);
                }
            }
        }

        #endregion
    }
}