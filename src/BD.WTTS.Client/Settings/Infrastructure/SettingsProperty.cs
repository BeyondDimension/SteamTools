// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

/// <summary>
/// 引用类型的设置属性
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public class SettingsProperty<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsPropertyBase<TValue>, IDisposable, INotifyPropertyChanged
    where TValue : class
    where TSettings : new()
{
    readonly Action<TSettings, TValue?> setter;
    readonly Func<TSettings, TValue?> getter;
    IDisposable? disposable;
    readonly IOptionsMonitor<TSettings> monitor;
    bool disposedValue;

    protected sealed override TValue? ModelValue => getter(monitor.CurrentValue);

    public SettingsProperty(TValue? @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null)
    {
        var settingsType = typeof(TSettings);
        PropertyName = propertyName.ThrowIsNull();
        AutoSave = autoSave;
        Default = @default;
        ParameterExpression parameter = Expression.Parameter(settingsType, "obj");
        MemberExpression property = Expression.Property(parameter, PropertyName);
        ParameterExpression value = Expression.Parameter(typeof(TValue), "value");
        BinaryExpression assign = Expression.Assign(property, value);
        setter = Expression.Lambda<Action<TSettings, TValue?>>(assign, parameter, value).Compile();
        getter = Expression.Lambda<Func<TSettings, TValue?>>(property, parameter).Compile();
        monitor = Ioc.Get<IOptionsMonitor<TSettings>>();
#if DEBUG
        switch (propertyName)
        {
            case "WindowSizePositions":
                break;
        }
#endif
        this.value = getter(monitor.CurrentValue);
        SetProperties(settingsType, propertyName);
        disposable = monitor.OnChange(OnChange);
    }

    void OnChange(TSettings settings)
    {
        if (!CanOnChange(settings, PropertyName))
            return;

        SetValue(getter(settings), false);
    }

    public override string PropertyName { get; }

    protected virtual TValue? GetActualValue()
    {
        if (typeof(TValue) == typeof(string))
        {
            if (value == default || string.IsNullOrWhiteSpace(value?.ToString()))
                return Default;
            return value;
        }
        else
        {
            return value ?? Default;
        }
    }

    protected virtual void SetModelValue(TValue? value)
    {
        if (typeof(TValue) == typeof(string))
        {
            if (value != default && string.IsNullOrWhiteSpace(value?.ToString()))
                value = default;
        }
        setter(monitor.CurrentValue, value); // 赋值模型类属性
    }

    protected override TValue? ActualValue
    {
        get => GetActualValue();
        set
        {
            SetValue(value);
        }
    }

    public TValue? Value
    {
        get => ActualValue;
        set => ActualValue = value;
    }

    public override TValue? Default { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetValue(TValue? value, bool save = true)
    {
        if (Equals(value, this.value))
            return; // 值相同无变化

        var oldValue = this.value;
        this.value = value; // 赋值当前字段

        var setter_value = value;
        if (EqualityComparer<TValue?>.Default.Equals(value, Default))
        {
            setter_value = default;
        }
        SetModelValue(setter_value); // 赋值模型类属性

        OnValueChanged(oldValue, value); // 调用变更事件

        if (save && AutoSave) // 自动保存
        {
            Save();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Save() => ISettings.TrySave(typeof(TSettings), monitor, true);

    public override void RaiseValueChanged(bool notSave = false)
    {
        var setter_value = value;
        if (EqualityComparer<TValue?>.Default.Equals(value, Default))
        {
            setter_value = default;
        }
        SetModelValue(setter_value); // 赋值模型类属性
        OnValueChanged(value, value); // 调用变更事件
        if (!notSave && AutoSave) // 自动保存
        {
            Save();
        }
    }

    public override void Reset()
    {
        var oldValue = value;
        value = Default; // 赋值当前字段
        SetModelValue(default); // 赋值模型类属性

        OnValueChanged(oldValue, value); // 调用变更事件

        if (AutoSave) // 自动保存
        {
            Save();
        }
    }

    public override string ToString() => value?.ToString() ?? string.Empty;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // 释放托管状态(托管对象)
                disposable?.Dispose();
            }

            // 释放未托管的资源(未托管的对象)并重写终结器
            // 将大型字段设置为 null
            disposable = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// 数组或集合等可遍历集合的设置属性
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TEnumerable"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public class SettingsProperty<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TEnumerable, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsProperty<TEnumerable, TSettings>, ICollection<TValue>
    where TEnumerable : class, ICollection<TValue>, new()
    where TSettings : new()
{
    int ICollection<TValue>.Count => value?.Count ?? 0;

    bool ICollection<TValue>.IsReadOnly => value?.IsReadOnly ?? false;

    public SettingsProperty(TEnumerable? @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null) : base(@default, autoSave, propertyName)
    {

    }

    protected override bool Equals(TEnumerable? left, TEnumerable? right)
    {
        if (left == null)
        {
            return right == null;
        }
        else if (right == null)
        {
            return left == null;
        }

        if (EqualityComparer<TEnumerable>.Default.Equals(left, right))
        {
            return true;
        }

        return left.SequenceEqual(right);
    }

    static void AddRange(TEnumerable source, IEnumerable<TValue>? items)
    {
        if (items == null) return;

        switch (source)
        {
            case List<TValue> list:
                list.AddRange(items);
                break;
            case IExtendedList<TValue> extendedList:
                extendedList.AddRange(items);
                break;
            default:
                items.ForEach(source.Add);
                break;
        }
    }

    public virtual void Add(TValue item, bool raiseValueChanged = true, bool notSave = false)
    {
        if (value == null)
        {
            value = Activator.CreateInstance<TEnumerable>();
            AddRange(value, Default);
        }
        value.Add(item);
        if (raiseValueChanged)
            RaiseValueChanged(notSave);
    }

    public virtual void AddRange(IEnumerable<TValue> items, bool raiseValueChanged = true, bool notSave = false)
    {
        if (value == null)
        {
            value = Activator.CreateInstance<TEnumerable>();
            AddRange(value, Default);
        }
        AddRange(value, items);
        if (raiseValueChanged)
            RaiseValueChanged(notSave);
    }

    public virtual bool Remove(TValue item, bool raiseValueChanged = true, bool notSave = false)
    {
        bool result;
        if (value == null)
        {
            if (Default.Any_Nullable())
            {
                value = Activator.CreateInstance<TEnumerable>();
                AddRange(value, Default);

                result = value.Remove(item);
                if (raiseValueChanged)
                    RaiseValueChanged(notSave);

                return result;
            }
            return false;
        }

        result = value.Remove(item);
        if (raiseValueChanged)
            RaiseValueChanged(notSave);

        return result;
    }

    public virtual bool Contains(TValue item)
    {
        if (value == null)
        {
            if (Default.Any_Nullable())
            {
                return Default.Contains(item);
            }
            return false;
        }

        return value.Contains(item);
    }

    public virtual void Clear(bool raiseValueChanged = true, bool notSave = false)
    {
        if (value == null)
        {
            value = Activator.CreateInstance<TEnumerable>();
            if (raiseValueChanged)
                RaiseValueChanged(notSave);
            return;
        }

        value = null;
        if (raiseValueChanged)
            RaiseValueChanged(notSave);
    }

    void ICollection<TValue>.Add(TValue item) => Add(item);

    void ICollection<TValue>.Clear() => Clear();

    void ICollection<TValue>.CopyTo(TValue[] array, int arrayIndex)
    {
        if (value == null) return;

        value.CopyTo(array, arrayIndex);
    }

    bool ICollection<TValue>.Remove(TValue item) => Remove(item);

    IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
    {
        if (value == null)
            return (IEnumerator<TValue>)Array.Empty<TValue>().GetEnumerator();
        return value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (value == null)
            return Array.Empty<TValue>().GetEnumerator();
        return value.GetEnumerator();
    }
}

/// <summary>
/// 字典的设置属性
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TDictionary"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public class SettingsProperty<TKey, TValue,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TDictionary,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsProperty<KeyValuePair<TKey, TValue>, TDictionary, TSettings>, IDictionary<TKey, TValue>
    where TDictionary : class, IDictionary<TKey, TValue>, new()
    where TSettings : new()
{
    public SettingsProperty(TDictionary? @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null) : base(@default, autoSave, propertyName)
    {

    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get
        {
            if (value == null)
            {
                if (Default != null)
                {
                    if (Default.TryGetValue(key, out var value))
                    {
                        return value;
                    }
                }
                return default!;
            }
            try
            {
                return value[key];
            }
            catch
            {
                return default!;
            }
        }

        set
        {
            if (this.value == null)
            {
                this.value = Activator.CreateInstance<TDictionary>();
                if (Default != null)
                {
                    foreach (var item in Default)
                    {
                        this.value.Add(item.Key, item.Value);
                    }
                }
            }
            this.value.Add(key, value);
        }
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get
        {
            if (value == null)
            {
                if (Default != null)
                {
                    return Default.Keys;
                }
                else
                {
                    return Array.Empty<TKey>();
                }
            }
            return value.Keys;
        }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get
        {
            if (value == null)
            {
                if (Default != null)
                {
                    return Default.Values;
                }
                else
                {
                    return Array.Empty<TValue>();
                }
            }
            return value.Values;
        }
    }

    public virtual void Add(TKey key, TValue value, bool raiseValueChanged = true, bool notSave = false)
    {
        if (this.value == null)
        {
            this.value = Activator.CreateInstance<TDictionary>();
            if (Default != null)
            {
                foreach (var item in Default)
                {
                    this.value.Add(item.Key, item.Value);
                }
            }
        }
        if (!this.value.TryAdd(key, value))
        {
            this.value[key] = value;
        }

        if (raiseValueChanged)
            RaiseValueChanged(notSave);
    }

    public override void Add(KeyValuePair<TKey, TValue> pair, bool raiseValueChanged = true, bool notSave = false)
    {
        Add(pair.Key, pair.Value, raiseValueChanged, notSave);
    }

    static bool TryAdd(IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary is ConcurrentDictionary<TKey, TValue> cdict)
        {
            // https://github.com/dotnet/runtime/issues/30451
            return cdict.TryAdd(key, value);
        }
        return dictionary.TryAdd(key, value);
    }

    public override void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, bool raiseValueChanged = true, bool notSave = false)
    {
        if (value == null)
        {
            value = Activator.CreateInstance<TDictionary>();
            if (Default != null)
            {
                foreach (var item in Default)
                {
                    value.Add(item.Key, item.Value);
                }
            }
        }

        foreach (var item in items)
        {
            try
            {
                if (!TryAdd(value, item.Key, item.Value))
                {
                    value[item.Key] = item.Value;
                }
            }
            catch
            {
                value[item.Key] = item.Value;
            }
        }

        if (raiseValueChanged)
            RaiseValueChanged(notSave);
    }

    public virtual bool ContainsKey(TKey key)
    {
        if (value == null)
        {
            if (Default != null)
            {
                return Default.ContainsKey(key);
            }
            return false;
        }
        return value.ContainsKey(key);
    }

    public virtual bool Remove(TKey key, bool raiseValueChanged = true, bool notSave = false)
    {
        bool result;
        if (value == null)
        {
            if (Default.Any_Nullable())
            {
                value = Activator.CreateInstance<TDictionary>();
                foreach (var item in Default)
                {
                    value.Add(item.Key, item.Value);
                }

                result = value.Remove(key);
                if (raiseValueChanged)
                    RaiseValueChanged(notSave);

                return result;
            }
            return false;
        }

        result = value.Remove(key);
        if (raiseValueChanged)
            RaiseValueChanged(notSave);

        return result;
    }

    public virtual bool TryGetValue(TKey key, out TValue value)
    {
        if (this.value == null)
        {
            if (Default != null)
            {
                return Default.TryGetValue(key, out value!);
            }
            value = default!;
            return false;
        }
        return this.value.TryGetValue(key, out value!);
    }

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Add(key, value);

    bool IDictionary<TKey, TValue>.Remove(TKey key) => Remove(key);

    public virtual TValue GetOrAdd(TKey key, TValue value)
    {
        Add(key, value);
        return value;
    }
}
