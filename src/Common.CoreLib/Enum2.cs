using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        public static string[] GetAllStrings<TEnum>() where TEnum : Enum => GetAll<TEnum>().Select(x => x.ToString()).ToArray();

        /// <summary>
        /// 将 Flags 枚举进行拆分
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="resultant"></param>
        /// <returns></returns>
        public static IEnumerable<TEnum> FlagsSplit<TEnum>(TEnum resultant) where TEnum : Enum
            => GetAll<TEnum>().Where(x => resultant.HasFlag(x)).ToArray();

        internal static TEnum ConvertToEnum<TEnum, TStruct>(this TStruct value)
        {
            var parameter = Expression.Parameter(typeof(TStruct));
            var dynamicMethod = Expression.Lambda<Func<TStruct, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile()(value);
        }

        public static int ConvertToInt32<TEnum>(TEnum value) where TEnum : Enum => ConvertToEnum<int, TEnum>(value);

        /// <summary>
        /// 返回指定枚举值的描述（通过
        /// <see cref="System.ComponentModel.DescriptionAttribute"/> 指定）。
        /// 如果没有指定描述，则返回枚举常数的名称，没有找到枚举常数则返回枚举值。
        /// </summary>
        /// <param name="value">要获取描述的枚举值。</param>
        /// <returns>指定枚举值的描述。</returns>
        public static string? GetDescription<TEnum>(this TEnum value) where TEnum : struct, Enum
        {
            Type enumType = value.GetType();
            // 获取枚举常数名称。
            string name = Enum.GetName(enumType, value);
            if (name != null)
            {
                // 获取枚举字段。
                FieldInfo fieldInfo = enumType.GetField(name);
                if (fieldInfo != null)
                {
                    if (Attribute.GetCustomAttribute(fieldInfo,
                        typeof(DescriptionAttribute), false) is DescriptionAttribute description)
                    {
                        if (description != null)
                        {
                            return description.Description;
                        }
                    }
                }
            }
            return null;
        }
    }
}