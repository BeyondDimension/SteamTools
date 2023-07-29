using IBinding = BD.WTTS.Converters.Abstractions.IBinding;

namespace BD.WTTS.Converters;

public sealed class EnumEqualValueConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;
        if (value is Enum value2)
        {
            if (parameter is Enum parameter2)
            {
                return value2.Equals(parameter2);
            }
            else if (parameter is string parameter3)
            {
                if (Enum.TryParse(value2.GetType(), parameter3, true, out var result))
                    return value2 == result;
            }
            else if (parameter is IConvertible parameter4)
            {
                switch (value2.GetTypeCode())
                {
                    case TypeCode.SByte:
                        return parameter4.ToSByte(CultureInfo.InvariantCulture) == value2.ConvertToSByte();
                    case TypeCode.Byte:
                        return parameter4.ToByte(CultureInfo.InvariantCulture) == value2.ConvertToByte();
                    case TypeCode.Int16:
                        return parameter4.ToInt16(CultureInfo.InvariantCulture) == value2.ConvertToInt16();
                    case TypeCode.UInt16:
                        return parameter4.ToUInt16(CultureInfo.InvariantCulture) == value2.ConvertToUInt16();
                    case TypeCode.Int32:
                        return parameter4.ToInt32(CultureInfo.InvariantCulture) == value2.ConvertToInt32();
                    case TypeCode.UInt32:
                        return parameter4.ToUInt32(CultureInfo.InvariantCulture) == value2.ConvertToUInt32();
                    case TypeCode.Int64:
                        return parameter4.ToInt64(CultureInfo.InvariantCulture) == value2.ConvertToInt64();
                    case TypeCode.UInt64:
                        return parameter4.ToUInt64(CultureInfo.InvariantCulture) == value2.ConvertToUInt64();
                }
            }
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return ((IBinding)this).DoNothing;
        if (value is Enum)
            return value;
        if (value is bool b && b && parameter is Enum)
            return parameter;
        return ((IBinding)this).DoNothing;
    }
}