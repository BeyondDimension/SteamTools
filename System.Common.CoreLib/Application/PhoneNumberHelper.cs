using System.Application.Columns;
using System.Common;
using System.Linq;

namespace System.Application
{
    public static class PhoneNumberHelper
    {
        const char HideChar = '*';
        public const int Length = 11;

        /// <summary>
        /// 手机号码隐藏中间四位数字
        /// <para>实现逻辑：字符串大于等于7时，将下标3~6替换为隐藏字符</para>
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="hideChar"></param>
        /// <returns></returns>
        public static string ToStringHideMiddleFour(string? phoneNumber, char hideChar = HideChar)
        {
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                if (phoneNumber.Length >= 7)
                {
                    var array = phoneNumber.ToCharArray();
                    array[3] = hideChar;
                    array[4] = hideChar;
                    array[5] = hideChar;
                    array[6] = hideChar;
                    return new string(array);
                }
                return phoneNumber;
            }
            return string.Empty;
        }

        /// <inheritdoc cref="ToStringHideMiddleFour(string?, char)"/>
        public static string ToStringHideMiddleFour(this IPhoneNumber phoneNumber, char hideChar = HideChar) => ToStringHideMiddleFour(phoneNumber.PhoneNumber, hideChar);

        /// <inheritdoc cref="ToStringHideMiddleFour(string?, char)"/>
        public static string ToStringHideMiddleFour(this IReadOnlyPhoneNumber phoneNumber, char hideChar = HideChar) => ToStringHideMiddleFour(phoneNumber.PhoneNumber, hideChar);

        /// <summary>
        /// 获取中国大陆地区11位手机号码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? GetChineseMainlandPhoneNumber11(string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var chars = new char[11];
                var index = chars.Length - 1;
                for (var i = value.Length - 1; i >= 0; i--)
                {
                    if (index < 0) break;
                    var item = value[i];
                    if (Constants.Digits.Any(x => x == item))
                    {
                        chars[index--] = item;
                    }
                }
                var valueArray = chars.Where(x => x != default).ToArray();
                if (valueArray.Length == 11)
                {
                    value = new string(valueArray);
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }
    }
}