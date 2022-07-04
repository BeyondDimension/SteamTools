using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static partial class EnumExtensions
{
    /// <summary>
    /// 返回一个布尔值，该值指示给定的整数值或其名称字符串是否存在于指定的枚举中
    /// </summary>
    /// <param name="value"></param>
    /// <returns>如果 enumType 中的某个常量的值等于 value，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDefined(this Enum value) => Enum.IsDefined(value.GetType(), value);

    public static TEnum ConvertToEnum<TEnum>(this sbyte value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, sbyte>(value);

    public static TEnum ConvertToEnum<TEnum>(this byte value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, byte>(value);

    public static TEnum ConvertToEnum<TEnum>(this ushort value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, ushort>(value);

    public static TEnum ConvertToEnum<TEnum>(this short value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, short>(value);

    public static TEnum ConvertToEnum<TEnum>(this int value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, int>(value);

    public static TEnum ConvertToEnum<TEnum>(this uint value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, uint>(value);

    public static TEnum ConvertToEnum<TEnum>(this long value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, long>(value);

    public static TEnum ConvertToEnum<TEnum>(this ulong value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<TEnum, ulong>(value);

    public static sbyte ConvertToSByte<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<sbyte, TEnum>(value);

    public static byte ConvertToByte<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<byte, TEnum>(value);

    public static ushort ConvertToUInt16<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<ushort, TEnum>(value);

    public static short ConvertToInt16<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<short, TEnum>(value);

    public static uint ConvertToUInt32<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<uint, TEnum>(value);

    public static int ConvertToInt32<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<int, TEnum>(value);

    public static ulong ConvertToUInt64<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<ulong, TEnum>(value);

    public static long ConvertToInt64<TEnum>(this TEnum value)
        where TEnum : Enum
        => ConvertibleHelper.Convert<long, TEnum>(value);
}