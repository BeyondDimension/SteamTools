// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

[DebuggerDisplay("Value={Value}, PropertyName={PropertyName}, AutoSave={AutoSave}")]
public sealed class SettingsProperty<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : IDisposable, INotifyPropertyChanged
{
    readonly Action<TSettings, TValue?> setter;
    readonly Func<TSettings, TValue?> getter;
    readonly IDisposable? disposable;
    readonly IOptionsMonitor<TSettings> monitor;
    TValue? value;

    void IDisposable.Dispose() => disposable?.Dispose();

    public SettingsProperty([CallerMemberName] string? propertyName = null)
    {
        PropertyName = propertyName.ThrowIsNull();
        ParameterExpression parameter = Expression.Parameter(typeof(TSettings), "obj");
        MemberExpression property = Expression.Property(parameter, PropertyName);
        ParameterExpression value = Expression.Parameter(typeof(TValue?), "value");
        BinaryExpression assign = Expression.Assign(property, value);
        setter = Expression.Lambda<Action<TSettings, TValue?>>(assign, parameter, value).Compile();
        getter = Expression.Lambda<Func<TSettings, TValue?>>(property, parameter).Compile();
        monitor = Ioc.Get<IOptionsMonitor<TSettings>>();
        this.value = getter(monitor.CurrentValue);
        disposable = monitor.OnChange(OnChange);
    }

    void OnChange(TSettings settings) => Value = getter(settings);

    public string PropertyName { get; }

    /// <summary>
    /// 自动保存，默认为 <see langword="true" />
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// 值
    /// </summary>
    public TValue? Value
    {
        get => value;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(value, this.value))
                return; // 值相同无变化

            var oldValue = this.value;
            this.value = value; // 赋值当前字段
            setter(monitor.CurrentValue, value); // 赋值模型类属性

            OnValueChanged(oldValue, value); // 调用变更事件

            if (AutoSave) // 自动保存
            {
                Save();
            }
        }
    }

    static readonly object lockSave = new();

    void Save()
    {
        lock (lockSave)
        {
            ISettings.TrySave(typeof(TSettings), monitor, true);
        }
    }

    public void RaiseValueChanged(bool notSave = false)
    {
        setter(monitor.CurrentValue, value); // 赋值模型类属性
        OnValueChanged(value, value); // 调用变更事件
        if (!notSave && AutoSave) // 自动保存
        {
            Save();
        }
    }

    public override string ToString() => value?.ToString() ?? string.Empty;

    public IDisposable Subscribe(Action<TValue?> listener, bool notifyOnInitialValue = true)
    {
        if (notifyOnInitialValue)
            listener(Value);
        return new ValueChangedEventListener(this, listener);
    }

    sealed class ValueChangedEventListener : IDisposable
    {
        private readonly Action<TValue?> _listener;
        private readonly SettingsProperty<TValue, TSettings> _source;

        public ValueChangedEventListener(SettingsProperty<TValue, TSettings> property, Action<TValue?> listener)
        {
            _listener = listener;
            _source = property;
            _source.ValueChanged += HandleValueChanged;
        }

        private void HandleValueChanged(object? sender, ValueChangedEventArgs<TValue> args)
        {
            _listener(args.NewValue);
        }

        public void Dispose()
        {
            _source.ValueChanged -= HandleValueChanged;
        }
    }

    public static implicit operator TValue?(SettingsProperty<TValue, TSettings> property)
        => property.Value;

    #region events

    public event EventHandler<ValueChangedEventArgs<TValue>>? ValueChanged;

    async void OnValueChanged(TValue? oldValue, TValue? newValue)
    {
        await MainThread2.InvokeOnMainThreadAsync(() =>
        {
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<TValue>(oldValue, newValue));
        });
    }

    readonly Dictionary<PropertyChangedEventHandler, EventHandler<ValueChangedEventArgs<TValue>>>
        _handlers = new();

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
