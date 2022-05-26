using System.Globalization;
using System.Linq.Expressions;

namespace System;

public static class ConvertibleHelper
{
    public static TOut Convert<TOut, TIn>(TIn value)
    {
        var parameter = Expression.Parameter(typeof(TIn));
        var dynamicMethod = Expression.Lambda<Func<TIn, TOut>>(
            Expression.Convert(parameter, typeof(TOut)),
            parameter);
        return dynamicMethod.Compile()(value);
    }

    public static T? Convert<T>(IConvertible? value) where T : notnull
    {
        if (value == default) return default;
        var typeCode = Type.GetTypeCode(typeof(T));
        switch (typeCode)
        {
            case TypeCode.Boolean:
                return Convert<T, bool>(value.ToBoolean(CultureInfo.InvariantCulture));
            case TypeCode.Byte:
                return Convert<T, byte>(value.ToByte(CultureInfo.InvariantCulture));
            case TypeCode.Char:
                return Convert<T, char>(value.ToChar(CultureInfo.InvariantCulture));
            case TypeCode.DateTime:
                return Convert<T, DateTime>(value.ToDateTime(CultureInfo.InvariantCulture));
            case TypeCode.Decimal:
                return Convert<T, decimal>(value.ToDecimal(CultureInfo.InvariantCulture));
            case TypeCode.Double:
                return Convert<T, double>(value.ToDouble(CultureInfo.InvariantCulture));
            case TypeCode.Int16:
                return Convert<T, short>(value.ToInt16(CultureInfo.InvariantCulture));
            case TypeCode.Int32:
                return Convert<T, int>(value.ToInt32(CultureInfo.InvariantCulture));
            case TypeCode.Int64:
                return Convert<T, long>(value.ToInt64(CultureInfo.InvariantCulture));
            case TypeCode.SByte:
                return Convert<T, sbyte>(value.ToSByte(CultureInfo.InvariantCulture));
            case TypeCode.Single:
                return Convert<T, float>(value.ToSingle(CultureInfo.InvariantCulture));
            case TypeCode.UInt16:
                return Convert<T, ushort>(value.ToUInt16(CultureInfo.InvariantCulture));
            case TypeCode.UInt32:
                return Convert<T, uint>(value.ToUInt32(CultureInfo.InvariantCulture));
            case TypeCode.UInt64:
                return Convert<T, ulong>(value.ToUInt64(CultureInfo.InvariantCulture));
            case TypeCode.String:
                return Convert<T, string>(value.ToString(CultureInfo.InvariantCulture));
            case TypeCode.Object:
                var jsonString = value.ToString(CultureInfo.InvariantCulture);
                return Serializable.DJSON<T>(jsonString);
            case TypeCode.DBNull:
            case TypeCode.Empty:
            default:
                return default;
        }
    }
}
