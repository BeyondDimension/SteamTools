// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class StringExtensions
    {
        #region System.String 内置函数增强型扩展(用于取代原有函数)

        #region IndexOf

        /// <summary>
        /// 报告指定 Unicode 字符在此字符串中的第一个匹配项的从零开始的索引。
        /// <para>注意：如果 value 仅为英文字符或数字符号，则使用原函数 <see cref="string.IndexOf(char)"/></para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要查找的 Unicode 字符。</param>
        /// <returns>如果找到该字符，则为 value 的从零开始的索引位置；如果未找到，则为 -1。</returns>
        public static int IndexOf_(this string str, char value) => str.IndexOf(value, StringComparison.Ordinal);

        /// <summary>
        /// 报告指定 Unicode 字符在此字符串中的第一个匹配项的从零开始的索引。
        /// <para>注意：如果 value 仅为英文字符或数字符号，则使用原函数 <see cref="string.IndexOf(char)"/></para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要查找的 Unicode 字符。</param>
        /// <returns>如果找到该字符，则为 value 的从零开始的索引位置；如果未找到，则为 -1。</returns>
        public static int IndexOf_Nullable(this string? str, char value)
        {
            if (str == null) return -1;
            return IndexOf_(str, value);
        }

        /// <summary>
        /// 报告指定字符串在此实例中的第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <returns>如果找到该字符串，则为 value 的从零开始的索引位置；如果未找到该字符串，则为 -1。 如果 value 为 Empty，则返回值为 0。</returns>
        public static int IndexOf_(this string str, string value) => str.IndexOf(value, StringComparison.Ordinal);

        /// <summary>
        /// 报告指定字符串在此实例中的第一个匹配项的从零开始的索引。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <returns>如果找到该字符串，则为 value 的从零开始的索引位置；如果未找到该字符串，则为 -1。 如果 value 为 Empty，则返回值为 0。</returns>
        public static int IndexOf_Nullable(this string? str, string value)
        {
            if (str == null) return -1;
            return IndexOf_(str, value);
        }

        #endregion

        #region Contains

        /// <summary>
        /// 返回一个值，该值指示指定的字符是否出现在此字符串中。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要查找的字符。</param>
        /// <returns>如果 value 参数在此字符串中出现，则为 true；否则为 false。</returns>
        public static bool Contains_(this string str, char value) => str.Contains(value, StringComparison.Ordinal);

        /// <summary>
        /// 返回一个值，该值指示指定的字符是否出现在此字符串中。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要查找的字符。</param>
        /// <returns>如果 value 参数在此字符串中出现，则为 true；否则为 false。</returns>
        public static bool Contains_Nullable(this string? str, char value)
        {
            if (str == null) return false;
            return Contains_(str, value);
        }

        /// <summary>
        /// 返回一个值，该值指示指定的子串是否出现在此字符串中。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <returns>如果 true 参数出现在此字符串中，或者 value 为空字符串 ("")，则为 value；否则为 false。</returns>
        public static bool Contains_(this string str, string value) => str.Contains(value, StringComparison.Ordinal);

        /// <summary>
        /// 返回一个值，该值指示指定的子串是否出现在此字符串中。
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <returns>如果 true 参数出现在此字符串中，或者 value 为空字符串 ("")，则为 value；否则为 false。</returns>
        public static bool Contains_Nullable(this string? str, string value)
        {
            if (str == null) return false;
            return Contains_(str, value);
        }

        #endregion

        #endregion
    }
}