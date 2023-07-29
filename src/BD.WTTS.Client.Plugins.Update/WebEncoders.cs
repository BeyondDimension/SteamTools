// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/aspnetcore/blob/v7.0.5/src/Shared/WebEncoders/WebEncoders.cs

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.WebUtilities;

static class WebEncoders
{
    /// <summary>
    /// Decodes a base64url-encoded string.
    /// </summary>
    /// <param name="input">The base64url-encoded input to decode.</param>
    /// <returns>The base64url-decoded form of the input.</returns>
    /// <remarks>
    /// The input must not contain any whitespace or padding characters.
    /// Throws <see cref="FormatException"/> if the input is malformed.
    /// </remarks>
    public static byte[] Base64UrlDecode(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return Base64UrlDecode(input, offset: 0, count: input.Length);
    }

    /// <summary>
    /// Decodes a base64url-encoded substring of a given string.
    /// </summary>
    /// <param name="input">A string containing the base64url-encoded input to decode.</param>
    /// <param name="offset">The position in <paramref name="input"/> at which decoding should begin.</param>
    /// <param name="count">The number of characters in <paramref name="input"/> to decode.</param>
    /// <returns>The base64url-decoded form of the input.</returns>
    /// <remarks>
    /// The input must not contain any whitespace or padding characters.
    /// Throws <see cref="FormatException"/> if the input is malformed.
    /// </remarks>
    public static byte[] Base64UrlDecode(string input, int offset, int count)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        //ValidateParameters(input.Length, nameof(input), offset, count);

        // Special-case empty input
        if (count == 0)
        {
            return new byte[0];
        }

        // Create array large enough for the Base64 characters, not just shorter Base64-URL-encoded form.
        var buffer = new char[GetArraySizeRequiredToDecode(count)];

        return Base64UrlDecode(input, offset, buffer, bufferOffset: 0, count: count);
    }

    /// <summary>
    /// Decodes a base64url-encoded <paramref name="input"/> into a <c>byte[]</c>.
    /// </summary>
    /// <param name="input">A string containing the base64url-encoded input to decode.</param>
    /// <param name="offset">The position in <paramref name="input"/> at which decoding should begin.</param>
    /// <param name="buffer">
    /// Scratch buffer to hold the <see cref="char"/>s to decode. Array must be large enough to hold
    /// <paramref name="bufferOffset"/> and <paramref name="count"/> characters as well as Base64 padding
    /// characters. Content is not preserved.
    /// </param>
    /// <param name="bufferOffset">
    /// The offset into <paramref name="buffer"/> at which to begin writing the <see cref="char"/>s to decode.
    /// </param>
    /// <param name="count">The number of characters in <paramref name="input"/> to decode.</param>
    /// <returns>The base64url-decoded form of the <paramref name="input"/>.</returns>
    /// <remarks>
    /// The input must not contain any whitespace or padding characters.
    /// Throws <see cref="FormatException"/> if the input is malformed.
    /// </remarks>
    public static byte[] Base64UrlDecode(string input, int offset, char[] buffer, int bufferOffset, int count)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        //ValidateParameters(input.Length, nameof(input), offset, count);
        if (bufferOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferOffset));
        }

        if (count == 0)
        {
            return new byte[0];
        }

        // Assumption: input is base64url encoded without padding and contains no whitespace.

        var paddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);
        var arraySizeRequired = checked(count + paddingCharsToAdd);
        Debug.Assert(arraySizeRequired % 4 == 0, "Invariant: Array length must be a multiple of 4.");

        if (buffer.Length - bufferOffset < arraySizeRequired)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Invalid {0}, {1} or {2} length.",
                    nameof(count),
                    nameof(bufferOffset),
                    nameof(input)),
                nameof(count));
        }

        // Copy input into buffer, fixing up '-' -> '+' and '_' -> '/'.
        var i = bufferOffset;
        for (var j = offset; i - bufferOffset < count; i++, j++)
        {
            var ch = input[j];
            if (ch == '-')
            {
                buffer[i] = '+';
            }
            else if (ch == '_')
            {
                buffer[i] = '/';
            }
            else
            {
                buffer[i] = ch;
            }
        }

        // Add the padding characters back.
        for (; paddingCharsToAdd > 0; i++, paddingCharsToAdd--)
        {
            buffer[i] = '=';
        }

        // Decode.
        // If the caller provided invalid base64 chars, they'll be caught here.
        return Convert.FromBase64CharArray(buffer, bufferOffset, arraySizeRequired);
    }

    /// <summary>
    /// Gets the minimum <c>char[]</c> size required for decoding of <paramref name="count"/> characters
    /// with the <see cref="Base64UrlDecode(string, int, char[], int, int)"/> method.
    /// </summary>
    /// <param name="count">The number of characters to decode.</param>
    /// <returns>
    /// The minimum <c>char[]</c> size required for decoding  of <paramref name="count"/> characters.
    /// </returns>
    public static int GetArraySizeRequiredToDecode(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return 0;
        }

        var numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);

        return checked(count + numPaddingCharsToAdd);
    }

    private static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
    {
        switch (inputLength % 4)
        {
            case 0:
                return 0;
            case 2:
                return 2;
            case 3:
                return 1;
            default:
                throw new FormatException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Malformed input: {0} is an invalid input length.",
                        inputLength));
        }
    }
}
