using System.Runtime.CompilerServices;
using Base64 = gfoidl.Base64.Base64;

// 使用 https://github.com/gfoidl/Base64 实现
// 与 2019年版本 比较
// Base64URL 不兼容
// Base64URLConfuse 不支持

namespace System.Text;

partial class Base64Extensions
{
    /// <summary>
    /// Base64编码(ReadOnlySpan&lt;Byte&gt; → String)
    /// </summary>
    /// <param name="inArray"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string Base64Encode(this ReadOnlySpan<byte> inArray, Base64FormattingOptions options = Base64FormattingOptions.None)
    {
        switch (options)
        {
            case Base64FormattingOptions.InsertLineBreaks:
                return Convert.ToBase64String(inArray, Base64FormattingOptions.InsertLineBreaks); ;
            case Base64FormattingOptions.None:
                return Base64.Default.Encode(inArray);

            default:
                throw new ArgumentOutOfRangeException(nameof(options));
        }
    }

    /// <summary>
    /// Base64解码(ReadOnlySpan&lt;Char&gt; → ByteArray)
    /// </summary>
    /// <param name="encoded"></param>
    /// <returns></returns>
    public static byte[] Base64DecodeToByteArray(this ReadOnlySpan<char> encoded) => Base64.Default.Decode(encoded);

    /// <summary>
    /// Base64解码(String → ByteArray)
    /// </summary>
    /// <param name="s"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static byte[] Base64DecodeToByteArray(this string s, Base64FormattingOptions options = Base64FormattingOptions.None)
    {
        switch (options)
        {
            case Base64FormattingOptions.InsertLineBreaks:
                return Convert.FromBase64String(s);

            case Base64FormattingOptions.None:
                var encoded = s.AsSpan();
                return Base64DecodeToByteArray(encoded);

            default:
                throw new ArgumentOutOfRangeException(nameof(options));
        }
    }
}

partial class Base64UrlExtensions
{
    /// <summary>
    /// Base64Url编码(ReadOnlySpan&lt;Byte&gt; → String)
    /// </summary>
    /// <param name="inArray"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Base64UrlEncode(this ReadOnlySpan<byte> inArray) => Base64.Url.Encode(inArray);

    /// <summary>
    /// Base64Url解码(ReadOnlySpan&lt;Char&gt; → ByteArray)
    /// </summary>
    /// <param name="encoded"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Base64UrlDecodeToByteArray(this ReadOnlySpan<char> encoded) => Base64.Url.Decode(encoded);
}