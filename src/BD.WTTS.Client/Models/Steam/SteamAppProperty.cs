#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public class SteamAppProperty
{
    private static readonly Type?[] _types = new Type?[9]
    {
        typeof(SteamAppPropertyTable),
        typeof(string),
        typeof(int),
        typeof(float),
        null,
        typeof(string),
        typeof(System.Drawing.Color),
        typeof(ulong),
        null
    };

    public readonly string Name;

    private SteamAppPropertyType _propType;

    private object? _value;

    public SteamAppPropertyType PropertyType
    {
        get
        {
            return _propType;
        }

        internal set
        {
            if (_propType != value)
            {
                _propType = value;
                _value = null;
            }
        }
    }

    public Type? ValueType => _types[(int)_propType];

    internal object? Value
    {
        get
        {
            return _value;
        }

        set
        {
            _value = null;
            if (ValueType.IsAssignableFrom(value.GetType()))
            {
                _value = value;
            }
            else if (value is string text)
            {
                switch (_propType)
                {
                    case SteamAppPropertyType.String:
                        _value = text;
                        break;
                    case SteamAppPropertyType.Int32:
                        _value = int.Parse(text);
                        break;
                    case SteamAppPropertyType.Float:
                        _value = float.Parse(text);
                        break;
                    case SteamAppPropertyType.WString:
                        _value = text;
                        break;
                    case SteamAppPropertyType.Uint64:
                        _value = ulong.Parse(text);
                        break;
                    case (SteamAppPropertyType)4:
                    case SteamAppPropertyType.Color:
                        break;
                }
            }
        }
    }

    public T? GetValue<T>()
    {
        TryGetValue<T>(out var result);
        return result;
    }

    public bool TryGetValue<T>(out T? result)
    {
        result = default;
        bool result2 = false;
        int propType = (int)_propType;
        if (propType >= 0 && propType < _types.Length)
        {
            var type = _types[propType];
            if (type != null && typeof(T).IsAssignableFrom(type))
            {
                result = _value == null ? default : (T)_value;
                result2 = true;
            }
        }
        return result2;
    }

    public SteamAppProperty(string name)
    {
        Name = name;
        _propType = SteamAppPropertyType._Invalid_;
        _value = null;
    }

    public SteamAppProperty(string name, SteamAppPropertyType type, object value)
    {
        Name = name;
        _propType = type;
        _value = value;
    }

    public SteamAppProperty(SteamAppProperty other)
    {
        Name = other.Name;
        _propType = other._propType;
        if (_propType == SteamAppPropertyType.Table)
        {
            _value = new SteamAppPropertyTable((SteamAppPropertyTable)other._value);
        }
        else
        {
            _value = other._value;
        }
    }

    public override bool Equals(object obj)
    {
        bool result = false;
        if (obj is SteamAppProperty property)
        {
            result = Name == property.Name && _propType == property._propType && _value.Equals(property.Value);
        }
        return result;
    }

    public override int GetHashCode()
    {
        return GetType().GetHashCode() ^ Name.GetHashCode() ^ _propType.GetHashCode() ^ _value.GetHashCode();
    }
}
#endif