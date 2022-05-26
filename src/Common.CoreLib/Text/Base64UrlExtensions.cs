using System.Runtime.CompilerServices;

namespace System.Text;

public static partial class Base64UrlExtensions
{
    /// <summary>
    /// Base64Url编码(ByteArray → String)
    /// </summary>
    /// <param name="inArray"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Base64UrlEncode(this byte[] inArray)
    {
        ReadOnlySpan<byte> inArray_ = inArray;
        return Base64UrlEncode(inArray_);
    }

    /// <summary>
    /// Base64Url编码(ByteArray → String)
    /// </summary>
    /// <param name="inArray"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Base64UrlEncode_Nullable(this byte[]? inArray)
    {
        if (inArray == default) return default;
        return Base64UrlEncode(inArray);
    }

    /// <summary>
    /// Base64Url编码(String → String)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Base64UrlEncode(this string s)
    {
        var inArray = Encoding.UTF8.GetBytes(s);
        return Base64UrlEncode(inArray);
    }

    /// <summary>
    /// Base64Url编码(String → String)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Base64UrlEncode_Nullable(this string? s)
    {
        if (s == default) return default;
        return Base64UrlEncode(s);
    }

    /// <summary>
    /// Base64Url解码(String → ByteArray)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Base64UrlDecodeToByteArray(this string s)
    {
        var encoded = s.AsSpan();
        return Base64UrlDecodeToByteArray(encoded);
    }

    /// <summary>
    /// Base64Url解码(String → ByteArray)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[]? Base64UrlDecodeToByteArray_Nullable(this string? s)
    {
        if (s == default) return default;
        return Base64UrlDecodeToByteArray(s);
    }

    /// <summary>
    /// Base64Url解码(String → String)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Base64UrlDecode(this string s)
    {
        var bytes = Base64UrlDecodeToByteArray(s);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Base64Url解码(String → String)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Base64UrlDecode_Nullable(this string? s)
    {
        if (s == default) return default;
        return Base64UrlDecode(s);
    }
}