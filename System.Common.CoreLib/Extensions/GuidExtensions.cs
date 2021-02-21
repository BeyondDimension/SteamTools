using System.Common;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class GuidExtensions
    {
        /// <summary>
        /// 返回 <see cref="Guid"/> 结构的此实例值的字符串表示形式(格式N)
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string ToStringN(this Guid guid) => guid.ToString(Constants.N);

        /// <summary>
        /// 返回 <see cref="Guid"/> 结构的此实例值的字符串表示形式(格式N)
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static string? ToStringN(this Guid? guid) => guid.HasValue ? guid.Value.ToStringN() : null;

        static bool TryParseGuid_Format_D(string? input, out Guid result) => Guid.TryParse(input, out result);

        static bool TryParseGuid_Format_N(string? input, out Guid result) => Guid.TryParseExact(input, Constants.N, out result);

        static bool TryParseGuid(string? input, out Guid result, bool isFormatNFirst)
        {
            if (isFormatNFirst)
            {
                if (TryParseGuid_Format_N(input, out result))
                {
                    return true;
                }
                else if (TryParseGuid_Format_D(input, out result))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (TryParseGuid_Format_D(input, out result))
                {
                    return true;
                }
                else if (TryParseGuid_Format_N(input, out result))
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 将包含 <see cref="Guid"/> 表示形式的指定只读字符范围转换为等效的 Guid 结构
        /// <para>优先尝试使用格式D</para>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseGuid(this string? input, out Guid result) => TryParseGuid(input, out result, false);

        /// <summary>
        /// 将包含 <see cref="Guid"/> 表示形式的指定只读字符范围转换为等效的 Guid 结构
        /// <para>优先尝试使用格式D</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid? TryParseGuid(this string? input) => TryParseGuid(input, out var result) ? result : default;

        /// <summary>
        /// 将包含 <see cref="Guid"/> 表示形式的指定只读字符范围转换为等效的 Guid 结构
        /// <para>优先尝试使用格式N</para>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParseGuidN(this string? input, out Guid result) => TryParseGuid(input, out result, true);

        /// <summary>
        /// 将包含 <see cref="Guid"/> 表示形式的指定只读字符范围转换为等效的 Guid 结构
        /// <para>优先尝试使用格式N</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid? TryParseGuidN(this string? input) => TryParseGuidN(input, out var result) ? result : default;
    }
}