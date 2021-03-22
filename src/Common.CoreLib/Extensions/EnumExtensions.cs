// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class EnumExtensions
    {
        /// <summary>
        /// 返回一个布尔值，该值指示给定的整数值或其名称字符串是否存在于指定的枚举中
        /// </summary>
        /// <param name="value"></param>
        /// <returns>如果 enumType 中的某个常量的值等于 value，则为 <see langword="true"/>；否则为 <see langword="false"/></returns>
        public static bool IsDefined(this Enum value) => Enum.IsDefined(value.GetType(), value);

        public static TEnum ConvertToEnum<TEnum>(this int value)
            where TEnum : struct, IConvertible
            => Enum2.ConvertToEnum<TEnum, int>(value);

        public static int ConvertToInt32<TEnum>(this TEnum value)
            where TEnum : struct, IConvertible
            => Enum2.ConvertToEnum<int, TEnum>(value);
    }
}