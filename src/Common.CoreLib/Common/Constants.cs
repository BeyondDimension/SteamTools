namespace System.Common
{
    public static class Constants
    {
        /// <summary>
        /// 数字
        /// </summary>
        public const string Digits = "0123456789";

        /// <summary>
        /// 大写字母
        /// </summary>
        public const string UpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// 小写字母
        /// </summary>
        public const string LowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 字母
        /// </summary>
        public const string Letters = LowerCaseLetters + UpperCaseLetters;

        /// <summary>
        /// 数字与字母
        /// </summary>
        public const string DigitsLetters = Digits + Letters;

        /// <summary>
        /// utf-8
        /// </summary>
        public const string UTF8 = "utf-8";

        /// <summary>
        /// #
        /// </summary>
        public const string Sharp = "#";

        /// <summary>
        /// .
        /// </summary>
        public const char DOT = '.';

        /// <summary>
        /// UTC Time 1970/1/1
        /// </summary>
        public const long UnixEpochTicks = 621355968000000000;

        /// <summary>
        /// 13位(毫秒)时间戳最大值
        /// </summary>
        public const long TimestampMillisecondsMaxValue = 253402300799999;

        public const string N = "N";

        public const float MaxProgress = 100f;
    }
}