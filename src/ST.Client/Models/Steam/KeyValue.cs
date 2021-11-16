using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Application.Models
{
    public enum UserStatType
    {
        Invalid = 0,
        Integer = 1,
        Float = 2,
        AverageRate = 3,
        Achievements = 4,
        GroupAchievements = 5,
    }
    public class KeyValue
    {
        private static readonly KeyValue _Invalid = new();
        public string Name = "<root>";
        public KeyValueType Type = KeyValueType.None;
        public object? Value;
        public bool Valid;

        public List<KeyValue>? Children;

        public KeyValue this[string key]
        {
            get
            {
                if (Children == null)
                {
                    return _Invalid;
                }

                var child = Children.SingleOrDefault(
                    c => string.Compare(c.Name, key, StringComparison.InvariantCultureIgnoreCase) == 0);

                if (child == null)
                {
                    return _Invalid;
                }

                return child;
            }
        }

        public string? AsString(string defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            if (Value == null)
            {
                return defaultValue;
            }

            return Value.ToString();
        }

        public int AsInteger(int defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        if (int.TryParse(Value?.ToString(), out int value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case KeyValueType.Int32:
                    {
                        return (int)Value!;
                    }

                case KeyValueType.Float32:
                    {
                        return (int)(float)Value!;
                    }

                case KeyValueType.UInt64:
                    {
                        return (int)((ulong)Value! & 0xFFFFFFFF);
                    }
            }

            return defaultValue;
        }

        public float AsFloat(float defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        if (float.TryParse(Value?.ToString(), out float value) == false)
                        {
                            return defaultValue;
                        }
                        return value;
                    }

                case KeyValueType.Int32:
                    {
                        return (int)Value!;
                    }

                case KeyValueType.Float32:
                    {
                        return (float)Value!;
                    }

                case KeyValueType.UInt64:
                    {
                        return (ulong)Value! & 0xFFFFFFFF;
                    }
            }

            return defaultValue;
        }

        public bool AsBoolean(bool defaultValue)
        {
            if (Valid == false)
            {
                return defaultValue;
            }

            switch (Type)
            {
                case KeyValueType.String:
                case KeyValueType.WideString:
                    {
                        if (int.TryParse(Value?.ToString(), out int value) == false)
                        {
                            return defaultValue;
                        }
                        return value != 0;
                    }

                case KeyValueType.Int32:
                    {
                        return ((int)Value!) != 0;
                    }

                case KeyValueType.Float32:
                    {
                        return ((int)(float)Value!) != 0;
                    }

                case KeyValueType.UInt64:
                    {
                        return ((ulong)Value!) != 0;
                    }
            }

            return defaultValue;
        }

        public override string ToString()
        {
            if (Valid == false)
            {
                return "<invalid>";
            }

            if (Type == KeyValueType.None)
            {
                return Name;
            }

            return string.Format(CultureInfo.CurrentCulture,
                "{0} = {1}",
                Name,
                Value);
        }

        public static KeyValue? LoadAsBinary(string path)
        {
            if (File.Exists(path) == false)
            {
                return null;
            }

            try
            {
                using var input = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var kv = new KeyValue();
                if (kv.ReadAsBinary(input) == false)
                {
                    return null;
                }
                return kv;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool ReadAsBinary(Stream input)
        {
            Children = new List<KeyValue>();

            try
            {
                while (true)
                {
                    var type = (KeyValueType)input.ReadValueU8();

                    if (type == KeyValueType.End)
                    {
                        break;
                    }

                    var current = new KeyValue
                    {
                        Type = type,
                        Name = input.ReadStringUnicode(),
                    };

                    switch (type)
                    {
                        case KeyValueType.None:
                            {
                                current.ReadAsBinary(input);
                                break;
                            }

                        case KeyValueType.String:
                            {
                                current.Valid = true;
                                current.Value = input.ReadStringUnicode();
                                break;
                            }

                        case KeyValueType.WideString:
                            {
                                throw new FormatException("wstring is unsupported");
                            }

                        case KeyValueType.Int32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueS32();
                                break;
                            }

                        case KeyValueType.UInt64:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU64();
                                break;
                            }

                        case KeyValueType.Float32:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueF32();
                                break;
                            }

                        case KeyValueType.Color:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU32();
                                break;
                            }

                        case KeyValueType.Pointer:
                            {
                                current.Valid = true;
                                current.Value = input.ReadValueU32();
                                break;
                            }

                        default:
                            {
                                throw new FormatException();
                            }
                    }

                    if (input.Position >= input.Length)
                    {
                        throw new FormatException();
                    }

                    Children.Add(current);
                }

                Valid = true;
                return input.Position == input.Length;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public enum KeyValueType : byte
    {
        None = 0,
        String = 1,
        Int32 = 2,
        Float32 = 3,
        Pointer = 4,
        WideString = 5,
        Color = 6,
        UInt64 = 7,
        End = 8,
    }
}
