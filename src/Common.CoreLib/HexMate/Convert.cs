using System.Diagnostics;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif
#if NETCOREAPP3_0_OR_GREATER
using System.Runtime.Intrinsics.X86;
#endif

namespace HexMate;

static class Convert
{
    /// <summary>
    /// Converts the specified string, which encodes binary data as hex characters, to an equivalent 8-bit unsigned integer array.
    /// </summary>
    /// <param name="s">The string to convert.</param>
    /// <returns>An array of 8-bit unsigned integers that is equivalent to <paramref name="s"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="s"/> is <code>null</code>.</exception>
    /// <exception cref="FormatException">The length of <paramref name="s"/>, ignoring white-space characters, is not zero or a multiple of 2.</exception>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid. <paramref name="s"/> contains a non-hex character.</exception>
    public static byte[] FromHexString(string s)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (s.Length == 0)
            return Array.Empty<byte>();

        unsafe
        {
            fixed (char* inPtr = s)
            {
                var resultLength = FromHex_ComputeResultLength(inPtr, s.Length);
#if NET5_0_OR_GREATER
                var result = GC.AllocateUninitializedArray<byte>(resultLength);
#else
                    var result = new byte[resultLength];
#endif
                fixed (byte* outPtr = result)
                {
                    var res = ConvertFromHexArray(outPtr, result.Length, inPtr, s.Length);
                    if (res < 0)
                        throw new FormatException(SR.Format_BadHexChar);
                    Debug.Assert(res == result.Length);
                }

                return result;
            }
        }
    }

    /// <summary>
    /// Converts an array of 8-bit unsigned integers to its equivalent string representation that is encoded with hex characters.
    /// A parameter specifies whether to insert line breaks in the return value and whether to insert upper- or lowercase hex characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
    /// <returns>The string representation in hex of the elements in <paramref name="inArray"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
    public static string ToHexString(byte[] inArray, HexFormattingOptions options = default)
        => ToHexString(inArray, 0, inArray?.Length ?? 0, options);

    /// <summary>
    /// Converts a subset of an array of 8-bit unsigned integers to its equivalent string representation that is encoded with hex characters.
    /// Parameters specify the subset as an offset in the input array, the number of elements in the array to convert,
    /// whether to insert line breaks in the return value, and whether to insert upper- or lowercase hex characters.
    /// </summary>
    /// <param name="inArray">An array of 8-bit unsigned integers.</param>
    /// <param name="offset">An offset in <paramref name="inArray"/>.</param>
    /// <param name="length">The number of elements of <paramref name="inArray"/> to convert.</param>
    /// <param name="options"><see cref="HexFormattingOptions.Lowercase"/> to produce lowercase output. <see cref="HexFormattingOptions.InsertLineBreaks"/> to insert a line break every 72 characters. <see cref="HexFormattingOptions.None"/> to do neither.</param>
    /// <returns>The string representation in hex of <paramref name="length"/> elements of <paramref name="inArray"/>, starting at position <paramref name="offset"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inArray"/> is <code>null</code>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="length"/> is negative.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> plus <paramref name="length"/> is greater than the length of <paramref name="inArray"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> is not a valid <see cref="HexFormattingOptions"/> value.</exception>
    public static string ToHexString(byte[] inArray, int offset, int length, HexFormattingOptions options = default)
    {
        if (inArray == null)
            throw new ArgumentNullException(nameof(inArray));
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), SR.ArgumentOutOfRange_Index);
        if (offset < 0)
            throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_GenericPositive);
        if (offset > (inArray.Length - length))
            throw new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_OffsetLength);
        if (options < HexFormattingOptions.None || options > (HexFormattingOptions.Lowercase | HexFormattingOptions.InsertLineBreaks))
            throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, (int)options), nameof(options));
        if (length == 0)
            return string.Empty;

        var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
        var outlen = ToHex_CalculateAndValidateOutputLength(inArray.Length - offset, insertLineBreaks);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return string.Create(outlen, (arr: inArray, off: offset, len: length, opt: options), (span, state) =>
        {
            unsafe
            {
                fixed (byte* inPtr = state.arr)
                fixed (char* outPtr = &MemoryMarshal.GetReference(span))
                {
                    ConvertToHexArray(outPtr, span.Length, inPtr + state.off, state.len, state.opt);
                }
            }
        });
#else
        var result = new string('\0', outlen);
        unsafe
        {
            fixed (byte* inPtr = inArray)
            fixed (char* outPtr = result)
            {
                ConvertToHexArray(outPtr, outlen, inPtr + offset, length, options);
            }
        }

        return result;
#endif
    }

    private const int hexLineBreakPosition = 72;
    private const int errInvalidData = -1;
    private const int errDestTooShort = -2;

    private static int ToHex_CalculateAndValidateOutputLength(int inputLength, bool insertLineBreaks)
    {
        var outlen = (long)inputLength * 2;

        if (outlen == 0)
            return 0;

        if (insertLineBreaks)
        {
            var newLines = outlen / hexLineBreakPosition;
            if ((outlen % hexLineBreakPosition) == 0)
            {
                --newLines;
            }
            outlen += newLines * 2;              // the number of line break chars we'll add, "\r\n"
        }

        // If we overflow an int then we cannot allocate enough
        // memory to output the value so throw
        if (outlen > int.MaxValue)
            throw new OutOfMemoryException();

        return (int)outlen;
    }

    private static unsafe void ConvertToHexArray(char* outChars, int outLength, byte* inData, int inLength, HexFormattingOptions options)
    {
        const int perLine = hexLineBreakPosition / 2;
        var remaining = inLength;

        var src = inData + inLength;
        var dest = outChars + outLength;

        var toLower = (options & HexFormattingOptions.Lowercase) != 0;
        var insertLineBreaks = (options & HexFormattingOptions.InsertLineBreaks) != 0;
        if (!insertLineBreaks)
        {
#if NETCOREAPP3_0_OR_GREATER
            if (Avx2.IsSupported & remaining >= 32)
            {
                remaining -= Utf16HexFormatter.Avx2.Format(ref src, ref dest, remaining, toLower);
                Debug.Assert(remaining < 32);
            }

            if (remaining >= 16)
            {
                if (Ssse3.IsSupported)
                {
                    remaining -= Utf16HexFormatter.Ssse3.Format(ref src, ref dest, remaining, toLower);
                    Debug.Assert(remaining < 16);
                }
                else if (Sse2.IsSupported)
                {
                    remaining -= Utf16HexFormatter.Sse2.Format(ref src, ref dest, remaining, toLower);
                    Debug.Assert(remaining < 16);
                }
            }
#endif
            if (remaining > 0)
            {
                remaining -= Utf16HexFormatter.Fixed.Format(ref src, ref dest, remaining, toLower);
            }
            Debug.Assert(remaining == 0);
        }
        else
        {
            var remainder = remaining % perLine;
            var remainingOnCurrentLine = remainder == 0 ? perLine : remainder;
            var bytesInIteration = remainingOnCurrentLine;
            while (remaining != 0)
            {
#if NETCOREAPP3_0_OR_GREATER
                if (Avx2.IsSupported)
                {
                    if (remainingOnCurrentLine >= 32)
                    {
                        remainingOnCurrentLine -= Utf16HexFormatter.Avx2.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                        Debug.Assert(remainingOnCurrentLine < 32);
                    }
                }
                else if (remainingOnCurrentLine >= 16)
                {
                    // Never run SSE code if AVX2 is available, as the remaining 8 bytes per line cannot be SSE-processed
                    if (Ssse3.IsSupported)
                    {
                        remainingOnCurrentLine -= Utf16HexFormatter.Ssse3.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                        Debug.Assert(remainingOnCurrentLine < 16);
                    }
                    else if (Sse2.IsSupported)
                    {
                        remainingOnCurrentLine -= Utf16HexFormatter.Sse2.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                        Debug.Assert(remainingOnCurrentLine < 16);
                    }
                }
#endif
                if (remainingOnCurrentLine > 0)
                {
                    remainingOnCurrentLine -= Utf16HexFormatter.Fixed.Format(ref src, ref dest, remainingOnCurrentLine, toLower);
                }
                Debug.Assert(remainingOnCurrentLine == 0);

                remaining -= bytesInIteration;
                if (remaining == 0) return;

                *(--dest) = '\n';
                *(--dest) = '\r';
                remainingOnCurrentLine = perLine;
                bytesInIteration = remainingOnCurrentLine;
            }
        }
    }

    private static bool IsWhitespace(int character)
        => character == ' ' || character == '\n' || character == '\r' || character == '\t';

    private static bool IsValid(uint character)
    {
        character -= '0';
        if (character > 9) goto Invalid;
        character -= 'A' - '0';
        if (character > 5) goto Invalid;
        character -= 'a' - 'A';
        if (character > 5) goto Invalid;

        return true;
    Invalid:
        return false;
    }

    private static unsafe int FromHex_ComputeResultLength(char* inputPtr, int inputLength)
           => CharHelper.CountUsefulCharacters(inputPtr, inputLength) / 2;

    private static unsafe int ConvertFromHexArray(byte* outData, int outLength, char* inChars, int inLength)
    {
        Debug.Assert(inLength != 0);
        if (inLength == 1)
        {
            return errInvalidData;
        }

        var charsToRead = inLength;
        var remainingIn = charsToRead;
        var bytesToWrite = outLength;
        var remainingOut = bytesToWrite;
        var src = inChars;
        var last = src;
        var srcEnd = src + inLength;
        var dest = outData;

        while (remainingOut > 0)
        {
#if NETCOREAPP3_0_OR_GREATER
            // Ignore errors in the SIMD part, and handle them in scalar part below
            if (Avx2.IsSupported && remainingIn >= 64 && remainingOut >= 32)
            {
                Utf16HexParser.Avx2.TryParse(ref src, ref dest, remainingOut);
                remainingIn = charsToRead - (int)(src - inChars);
                remainingOut = bytesToWrite - (int)(dest - outData);
            }

            if (remainingIn >= 32 && remainingOut >= 16)
            {
                if (Ssse3.IsSupported)
                {
                    Utf16HexParser.Ssse3.TryParse(ref src, ref dest, remainingOut);
                    remainingIn = charsToRead - (int)(src - inChars);
                    remainingOut = bytesToWrite - (int)(dest - outData);
                }
                else if (Sse2.IsSupported)
                {
                    Utf16HexParser.Sse2.TryParse(ref src, ref dest, remainingOut);
                    remainingIn = charsToRead - (int)(src - inChars);
                    remainingOut = bytesToWrite - (int)(dest - outData);
                }
            }
#endif
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (!Utf16HexParser.Span.TryParse(ref src, remainingIn, ref dest, remainingOut))
#else
                if (!Utf16HexParser.Fixed.TryParse(ref src, remainingIn, ref dest, remainingOut))
#endif
            {
                goto InvalidData;
            }
            remainingIn = charsToRead - (int)(src - inChars);
            remainingOut = bytesToWrite - (int)(dest - outData);

            if (src >= srcEnd)
            {
                goto Ok;
            }

            if (src == last)
            {
                // No progress == invalid data
                goto InvalidData;
            }

            last = src;
        }

        // No more space in destination

        if (src < srcEnd)
        {
            while (IsWhitespace(*src))
            {
                // Skip past any consecutive trailing white-space in the input
                if (++src >= srcEnd)
                {
                    goto Ok;
                }
            }

            if (IsValid(*src))
            {
                goto OutLenTooShort;
            }
            else
            {
                goto InvalidData;
            }
        }

    Ok:
        return (int)(dest - outData);

    InvalidData:
        return errInvalidData;

    OutLenTooShort:
        return errDestTooShort;
    }
}
