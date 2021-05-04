using System.Application.Columns;
using System.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Properties;

namespace System.Application
{
    public static class PhoneNumberHelper
    {
        const char HideChar = '*';

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

        public const int ChineseMainlandPhoneNumberLength = 11;

        /// <summary>
        /// 获取中国大陆地区11位手机号码
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? GetChineseMainlandPhoneNumber(string? value)
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

        /// <summary>
        /// 中国大陆地区手机号码号段
        /// </summary>
        static class ChineseMainlandPhoneNumberSegment
        {
            /// <summary>
            /// 中国电信
            /// </summary>
            public static byte[] ChinaTelecom = { 133, 153, 177, 173, 180, 181, 189, 199, 149 };

            /// <summary>
            /// 中国联通
            /// </summary>
            public static byte[] ChinaUnicom = { 130, 131, 132, 155, 156, 185, 186, 145, 176, 166, 146, 175 };

            /// <summary>
            /// 中国移动
            /// </summary>
            public static byte[] ChinaMobile = { 139, 138, 137, 136, 135, 134, 147, 150, 151, 152, 157, 158, 159, 178, 182, 183, 184, 187, 188, 198 };

            /// <summary>
            /// 虚拟运营商
            /// </summary>
            public static byte[] MobileVirtualNetwork = { 170, 171, 172 };

            public static byte[] Other = { 190, 191, 192, 193, 195, 196, 197, 198, 199 };

            static readonly Lazy<byte[]> mSummary = new(() => new[]
            {
                ChinaTelecom,
                ChinaUnicom,
                ChinaMobile,
                MobileVirtualNetwork,
                Other,
            }.SelectMany(b => b).ToArray());

            /// <summary>
            /// 总号段
            /// </summary>
            public static byte[] Summary => mSummary.Value;

            /// <summary>
            /// 黑名单
            /// </summary>
            public static readonly byte[] Blacklist = new byte[] {
                148, 149, 146, // 物联网号段
            };
        }

        /// <summary>
        /// 中国大陆地区手机号码号段验证使用 黑名单(<see langword="true"/>) 或 白名单(<see langword="false"/>)
        /// </summary>
        public static bool ChineseMainlandPhoneNumberSegmentVerifyUseBlacklistOrWhitelist { get; set; } = true;

        /// <summary>
        /// 中国大陆地区手机号码号段验证(通常仅服务端验证)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static bool IsChineseMainlandPhoneNumberSegment(string value,
            [NotNullWhen(false)] out string? errMsg)
        {
            var segment = value.Substring(0, 3).TryParseByte();
            if (!segment.HasValue)
            {
                errMsg = SR.UnsupportedPhoneNumberSegment_.Format(0);
                return false;
            }
            if (ChineseMainlandPhoneNumberSegmentVerifyUseBlacklistOrWhitelist)
            {
                if (ChineseMainlandPhoneNumberSegment.Blacklist.Contains(segment.Value))
                {
                    errMsg = SR.UnsupportedPhoneNumberSegment_.Format(segment ?? 0);
                    return false;
                }
            }
            else
            {
                if (!ChineseMainlandPhoneNumberSegment.Summary.Contains(segment.Value))
                {
                    errMsg = SR.UnsupportedPhoneNumberSegment_.Format(segment ?? 0);
                    return false;
                }
            }

            errMsg = null;
            return true;
        }

        /// <summary>
        /// Android 模拟器默认电话号码，硬编码屏蔽此号
        /// </summary>
        public const string SimulatorDefaultValue = "15555218135";
    }
}