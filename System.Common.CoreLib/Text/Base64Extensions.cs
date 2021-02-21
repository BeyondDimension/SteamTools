namespace System.Text
{
    public static partial class Base64Extensions
    {
        /// <summary>
        /// Base64编码(ByteArray → String)
        /// </summary>
        /// <param name="inArray"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Base64Encode(this byte[] inArray, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            ReadOnlySpan<byte> inArray_ = inArray;
            return Base64Encode(inArray_, options);
        }

        /// <summary>
        /// Base64编码(ByteArray → String)
        /// </summary>
        /// <param name="inArray"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string? Base64Encode_Nullable(this byte[]? inArray, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (inArray == default) return default;
            return Base64Encode(inArray, options);
        }

        /// <summary>
        /// Base64编码(String → String)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Base64Encode(this string s, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            var inArray = Encoding.UTF8.GetBytes(s);
            return Base64Encode(inArray, options);
        }

        /// <summary>
        /// Base64编码(String → String)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string? Base64Encode_Nullable(this string? s, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (s == default) return default;
            return Base64Encode(s, options);
        }

        /// <summary>
        /// Base64解码(String → ByteArray)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static byte[]? Base64DecodeToByteArray_Nullable(this string? s, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (s == default) return default;
            return Base64DecodeToByteArray(s, options);
        }

        /// <summary>
        /// Base64解码(String → String)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Base64Decode(this string s, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            var bytes = Base64DecodeToByteArray(s, options);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Base64解码(String → String)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string? Base64Decode_Nullable(this string? s, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            if (s == default) return default;
            return Base64Decode(s, options);
        }
    }
}