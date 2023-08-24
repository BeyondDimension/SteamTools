// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings;

/// <summary>
/// 值类型的设置属性
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TSettings"></typeparam>
public class SettingsStructProperty<TValue, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TSettings> : SettingsStructPropertyBase<TValue, TSettings>
    where TValue : struct
    where TSettings : new()
{
    public SettingsStructProperty(TValue @default = default, bool autoSave = true, [CallerMemberName] string? propertyName = null) : base(@default, autoSave, propertyName)
    {
    }

    public TValue Value
    {
        get
        {
            var value = ActualValue;
            return value;
            //if (value.HasValue)
            //    return value.Value;
            //return default;
        }

        set => ActualValue = value;
    }
}