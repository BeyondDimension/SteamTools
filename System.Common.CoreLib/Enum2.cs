using System.Linq.Expressions;

namespace System
{
    public static class Enum2
    {
        /// <summary>
        /// 获取枚举定义的所有常量
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum[] GetAll<TEnum>() where TEnum : Enum => (TEnum[])Enum.GetValues(typeof(TEnum));

        internal static TEnum ConvertToEnum<TEnum, TStruct>(this TStruct value)
        {
            var parameter = Expression.Parameter(typeof(TStruct));
            var dynamicMethod = Expression.Lambda<Func<TStruct, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile()(value);
        }

        public static int ConvertToInt32<TEnum>(TEnum value) where TEnum : Enum => ConvertToEnum<int, TEnum>(value);
    }
}