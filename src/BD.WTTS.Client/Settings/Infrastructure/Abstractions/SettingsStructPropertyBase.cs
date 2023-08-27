// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public class SettingsStructPropertyBase<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsPropertyBase<TValue>, IDisposable, INotifyPropertyChanged
    where TValue : struct
    where TSettings : new()
{
    readonly Action<TSettings, TValue> setter;
    readonly Func<TSettings, TValue> getter;
    IDisposable? disposable;
    readonly IOptionsMonitor<TSettings> monitor;
    bool disposedValue;

    protected sealed override TValue ModelValue => getter(monitor.CurrentValue);

    public SettingsStructPropertyBase(TValue @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null)
    {
        var settingsType = typeof(TSettings);
        PropertyName = propertyName.ThrowIsNull();

        AutoSave = autoSave;
        Default = @default;
        ParameterExpression parameter = Expression.Parameter(settingsType, "obj");
        MemberExpression property = Expression.Property(parameter, PropertyName);
        ParameterExpression value = Expression.Parameter(typeof(TValue), "value");
        BinaryExpression assign = Expression.Assign(property, value);
        setter = Expression.Lambda<Action<TSettings, TValue>>(assign, parameter, value).Compile();
        getter = Expression.Lambda<Func<TSettings, TValue>>(property, parameter).Compile();
        monitor = Ioc.Get<IOptionsMonitor<TSettings>>();
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

    /// <summary>
    /// 值
    /// </summary>
    protected override TValue ActualValue
    {
        get => value;
        set
        {
            SetValue(value);
        }
    }

    public override TValue Default { get; set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetValue(TValue value, bool save = true)
    {
        if (Equals(value, this.value))
            return; // 值相同无变化

        var oldValue = this.value;
        this.value = value; // 赋值当前字段

        var setter_value = value;
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

    public override string ToString() => value.ToString() ?? string.Empty;

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