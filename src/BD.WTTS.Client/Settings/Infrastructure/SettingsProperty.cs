// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

/// <summary>
/// 引用类型的设置属性
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public class SettingsProperty<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsPropertyBase<TValue>, IDisposable, INotifyPropertyChanged
    where TValue : class
{
    readonly Action<TSettings, TValue?> setter;
    readonly Func<TSettings, TValue?> getter;
    IDisposable? disposable;
    readonly IOptionsMonitor<TSettings> monitor;
    TValue? value;
    bool disposedValue;

    public SettingsProperty(TValue? @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null)
    {
        PropertyName = propertyName.ThrowIsNull();
        AutoSave = autoSave;
        Default = @default;
        ParameterExpression parameter = Expression.Parameter(typeof(TSettings), "obj");
        MemberExpression property = Expression.Property(parameter, PropertyName);
        ParameterExpression value = Expression.Parameter(typeof(TValue), "value");
        BinaryExpression assign = Expression.Assign(property, value);
        setter = Expression.Lambda<Action<TSettings, TValue?>>(assign, parameter, value).Compile();
        getter = Expression.Lambda<Func<TSettings, TValue?>>(property, parameter).Compile();
        monitor = Ioc.Get<IOptionsMonitor<TSettings>>();
        this.value = getter(monitor.CurrentValue);
        disposable = monitor.OnChange(OnChange);
    }

    void OnChange(TSettings settings)
    {
        if (ISettings.SaveStatus[typeof(TSettings)])
            return;

        SetValue(getter(settings), false);
    }

    public override string PropertyName { get; }

    public override TValue? ActualValue
    {
        get => value ?? Default;
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

    public override TValue? Default { get; }

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
        setter(monitor.CurrentValue, setter_value); // 赋值模型类属性

        OnValueChanged(oldValue, value); // 调用变更事件

        if (save && AutoSave) // 自动保存
        {
            Save();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void Save() => ISettings.TrySave(typeof(TSettings), monitor, true);

    public override void RaiseValueChanged(bool notSave = false)
    {
        var setter_value = value;
        if (EqualityComparer<TValue?>.Default.Equals(value, Default))
        {
            setter_value = default;
        }
        setter(monitor.CurrentValue, setter_value); // 赋值模型类属性
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
        setter(monitor.CurrentValue, default); // 赋值模型类属性

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
public class SettingsProperty<TValue, TEnumerable, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsProperty<TEnumerable, TSettings>
    where TEnumerable : class, IEnumerable<TValue>
{
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
}