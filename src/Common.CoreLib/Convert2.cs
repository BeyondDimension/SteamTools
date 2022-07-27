using System.Runtime.CompilerServices;

namespace System;

public static class Convert2
{
    /// <summary>
    /// Converts the specified string, which encodes binary data as hex characters, to an equivalent 8-bit unsigned integer array.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>An array of 8-bit unsigned integers that is equivalent to s.</returns>
#if HEXMATE || NET5_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static byte[] FromHexString(string s)
#if HEXMATE
        => HexMate.Convert.FromHexString(s);
#elif NET5_0_OR_GREATER
        => Convert.FromHexString(s);
#else
    {
        var bytes = new byte[s.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
        }
    }
#endif
}
