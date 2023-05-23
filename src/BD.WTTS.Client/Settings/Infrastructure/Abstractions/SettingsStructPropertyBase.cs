// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public class SettingsStructPropertyBase<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsPropertyBase<TValue?>, IDisposable, INotifyPropertyChanged
    where TValue : struct
{
    readonly Action<TSettings, TValue?> setter;
    readonly Func<TSettings, TValue?> getter;
    IDisposable? disposable;
    readonly IOptionsMonitor<TSettings> monitor;
    TValue? value;
    bool disposedValue;

    public SettingsStructPropertyBase(TValue? @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null)
    {
        PropertyName = propertyName.ThrowIsNull();
        AutoSave = autoSave;
        Default = @default;
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

    void OnChange(TSettings settings)
    {
        if (ISettings.SaveStatus[typeof(TSettings)])
            return;

        SetValue(getter(settings), false);
    }

    public override string PropertyName { get; }

    /// <summary>
    /// 值
    /// </summary>
    public override TValue? ActualValue
    {
        get => value ?? Default;
        set
        {
            SetValue(value);
        }
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
        setter(monitor.CurrentValue, value); // 赋值模型类属性
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

    public IDisposable Subscribe(Action<TValue> listener, bool notifyOnInitialValue = true)
    {
        void listener_(TValue? value)
        {
            TValue value_;
            if (value.HasValue)
            {
                value_ = value.Value;
            }
            else if (Default.HasValue)
            {
                value_ = Default.Value;
            }
            else
            {
                value_ = default;
            }
            listener?.Invoke(value_);
        }
        return Subscribe((Action<TValue?>)listener_, notifyOnInitialValue);
    }

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