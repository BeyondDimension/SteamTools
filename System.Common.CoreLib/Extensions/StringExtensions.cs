using System.Collections.Generic;
using System.Linq;

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
        public static int IndexOf_(this string str, char value) => DI.IsRunningOnMono ? str.IndexOf(value) : str.IndexOf(value, StringComparison.Ordinal);

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

        /// <summary>
        /// 将版本号字符串转换为 <see cref="Version"/> 对象，传入的字符串不能为 <see langword="null"/>
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static Version? VersionTryParse(string version)
        {
            var versionChars = GetVersion(version).ToArray();
            if (versionChars.Any() && Version.TryParse(new string(versionChars), out var versionObj))
            {
                return versionObj;
            }
            static IEnumerable<char> GetVersion(string s)
            {
                var dianCount = 0;
                var findChar = false;
                foreach (var item in s)
                {
                    var isDian = item == '.';
                    if (isDian)
                    {
                        if (dianCount++ >= 3)
                        {
                            yield break;
                        }
                    }
                    if (isDian || item >= '0' && item <= '9')
                    {
                        findChar = true;
                        yield return item;
                    }
                    else if (findChar)
                    {
                        yield break;
                    }
                }
            }
            return null;
        }

        #region TryParse

        /// <summary>
        /// 尝试将数字的字符串表示形式转换为它的等效 32 位有符号整数
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? TryParseInt32(this string value) => int.TryParse(value, out var temp) ? temp as int? : null;

        #endregion

        public static string TrimStart(this string s, string trimString)
        {
            if (trimString.StartsWith(trimString))
            {
                return s[trimString.Length..];
            }
            else
            {
                return s;
            }
        }

        public static string TrimEnd(this string s, string trimString)
        {
            if (trimString.EndsWith(trimString))
            {
                return s.Substring(0, s.Length - trimString.Length);
            }
            else
            {
                return s;
            }
        }

        #region 数字字母判断

        /// <summary>
        /// 字符串是否是一个数字（可以零开头 无符号不能+-.）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsDigital(this string input) => input?.All(IsDigital) ?? false;

        public static bool IsDigital(this char input) => input >= '0' && input <= '9';

        /// <summary>
        /// 字符串内含有数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasDigital(this string input) => input?.Any(x => x >= '0' && x <= '9') ?? false;

        /// <summary>
        /// 字符串内含有小写字母
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasLowerLetter(this string input) => input?.Any(x => x >= 'a' && x <= 'z') ?? false;

        /// <summary>
        /// 字符串内含有大写字母
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasUpperLetter(this string input) => input?.Any(x => x >= 'A' && x <= 'Z') ?? false;

        /// <summary>
        /// 字符串内含有其他字符
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool HasOther(this string input) => input?.Any(x => !(x >= 'a' && x <= 'z' || x >= 'A' && x <= 'Z' || x >= '0' && x <= '9')) ?? false;

        #endregion
    }
}