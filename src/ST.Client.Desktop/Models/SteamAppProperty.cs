using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace System.Application.Models
{
    class SteamAppProperty
    {
        public SteamAppPropertyType PropertyType
        {
            get
            {
                return this._propType;
            }
            internal set
            {
                if (this._propType != value)
                {
                    this._propType = value;
                    this._value = null;
                }
            }
        }

        public Type ValueType
        {
            get
            {
                return SteamAppProperty._types[(int)this._propType];
            }
        }

        internal object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = null;
                if (this.ValueType.IsAssignableFrom(value.GetType()))
                {
                    this._value = value;
                    return;
                }
                string text = value as string;
                if (text != null)
                {
                    switch (this._propType)
                    {
                        case SteamAppPropertyType.String:
                            this._value = text;
                            return;
                        case SteamAppPropertyType.Int32:
                            this._value = int.Parse(text);
                            return;
                        case SteamAppPropertyType.Float:
                            this._value = float.Parse(text);
                            return;
                        case (SteamAppPropertyType)4:
                        case SteamAppPropertyType.Color:
                            break;
                        case SteamAppPropertyType.WString:
                            this._value = text;
                            return;
                        case SteamAppPropertyType.Uint64:
                            this._value = ulong.Parse(text);
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
            this.TryGetValue<T>(out result);
            return result;
        }

        public bool TryGetValue<T>(out T result)
        {
            result = default(T);
            bool result2 = false;
            int propType = (int)this._propType;
            if (propType >= 0 && propType < SteamAppProperty._types.Length)
            {
                Type type = SteamAppProperty._types[propType];
                if (type != null && typeof(T).IsAssignableFrom(type))
                {
                    result = (T)((object)this._value);
                    result2 = true;
                }
            }
            return result2;
        }

        public SteamAppProperty(string name)
        {
            this.Name = name;
            this._propType = SteamAppPropertyType._Invalid_;
            this._value = null;
        }

        public SteamAppProperty(string name, SteamAppPropertyType type, object value)
        {
            this.Name = name;
            this._propType = type;
            this._value = value;
        }

        public SteamAppProperty(SteamAppProperty other)
        {
            this.Name = other.Name;
            this._propType = other._propType;
            if (this._propType == SteamAppPropertyType.Table)
            {
                this._value = new SteamAppPropertyTable((SteamAppPropertyTable)other._value);
                return;
            }
            this._value = other._value;
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            SteamAppProperty property = obj as SteamAppProperty;
            if (property != null)
            {
                result = (this.Name == property.Name && this._propType == property._propType && this._value.Equals(property.Value));
            }
            return result;
        }

        public override int GetHashCode()
        {
            return base.GetType().GetHashCode() ^ this.Name.GetHashCode() ^ this._propType.GetHashCode() ^ this._value.GetHashCode();
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
