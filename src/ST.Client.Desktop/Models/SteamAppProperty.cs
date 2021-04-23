using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace System.Application.Models
{
    public class SteamAppProperty
    {
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

        public Type ValueType
        {
            get
            {
                return SteamAppProperty._types[(int)_propType];
            }
        }

        internal object Value
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
                    return;
                }
                string text = value as string;
                if (text != null)
                {
                    switch (_propType)
                    {
                        case SteamAppPropertyType.String:
                            _value = text;
                            return;
                        case SteamAppPropertyType.Int32:
                            _value = int.Parse(text);
                            return;
                        case SteamAppPropertyType.Float:
                            _value = float.Parse(text);
                            return;
                        case (SteamAppPropertyType)4:
                        case SteamAppPropertyType.Color:
                            break;
                        case SteamAppPropertyType.WString:
                            _value = text;
                            return;
                        case SteamAppPropertyType.Uint64:
                            _value = ulong.Parse(text);
                            break;
                        default:
                            return;
                    }
                }
            }
        }

        public T GetValue<T>()
        {
            T result;
            TryGetValue<T>(out result);
            return result;
        }

        public bool TryGetValue<T>(out T result)
        {
            result = default(T);
            bool result2 = false;
            int propType = (int)_propType;
            if (propType >= 0 && propType < SteamAppProperty._types.Length)
            {
                Type type = SteamAppProperty._types[propType];
                if (type != null && typeof(T).IsAssignableFrom(type))
                {
                    result = (T)((object)_value);
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
                _value = (SteamAppPropertyTable)other._value;
                return;
            }
            _value = other._value;
        }

        public new bool Equals(object obj)
        {
            bool result = false;
            SteamAppProperty property = obj as SteamAppProperty;
            if (property != null)
            {
                result = (Name == property.Name && _propType == property._propType && _value.Equals(property.Value));
            }
            return result;
        }

        public new int GetHashCode()
        {
            return base.GetType().GetHashCode() ^ Name.GetHashCode() ^ _propType.GetHashCode() ^ _value.GetHashCode();
        }

        static SteamAppProperty()
        {
            Type[] array = new Type[9];
            array[0] = typeof(SteamAppPropertyTable);
            array[1] = typeof(string);
            array[2] = typeof(int);
            array[3] = typeof(float);
            array[5] = typeof(string);
            array[6] = typeof(Color);
            array[7] = typeof(ulong);
            SteamAppProperty._types = array;
        }

        private static readonly Type[] _types;

        public readonly string Name;

        private SteamAppPropertyType _propType;

        private object _value;
    }
}
