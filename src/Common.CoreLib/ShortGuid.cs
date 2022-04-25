// MIT License
// Copyright(c) 2007 Dave Transom
// https://github.com/AigioL/CSharpVitamins.ShortGuid/blob/master/CSharpVitamins.ShortGuid/ShortGuid.cs
// https://github.com/csharpvitamins/CSharpVitamins.ShortGuid/blob/master/CSharpVitamins.ShortGuid/ShortGuid.cs
using System.Diagnostics;
#if !NET40
using gfoidl.Base64;
#endif

namespace System
{
    /// <summary>
    /// A convenience wrapper struct for dealing with URL-safe Base64 encoded globally unique identifiers (GUID),
    /// making a shorter string value (22 vs 36 characters long).
    /// </summary>
    /// <remarks>
    /// What is URL-safe Base64? That's just a Base64 string with well known special characters replaced (/, +)
    /// or removed (==).
    ///
    /// <para>NB: As of version 2.0.0, <see cref="Decode(string)"/> performs a sanity check when decoding
    /// strings to ensure they haven't been tampered with, i.e. allowing the end of a Base64 string to be tweaked
    /// where it still produces that same byte array to create the underlying Guid. Effectively there is "unused
    /// space" in the Base64 string which is ignored, but will now result in an <see cref="FormatException"/> being
    /// thrown.</para>
    ///
    /// <para>ShortGuid will never produce an invalid string, however if one is supplied it, could result in an
    /// unintended collision where multiple URL-safe Base64 strings can point to the same Guid. To avoid this
    /// uncertainty, a round-trip check is performed to ensure a 1-1 match with the input string.</para>
    ///
    /// <para>Stick with version 1.1.0 if you require the old behaviour with opt-in strict parsing.</para>
    /// </remarks>
    [DebuggerDisplay("{Value}")]
    public struct ShortGuid
    {
        /// <summary>
        /// A read-only instance of the ShortGuid struct whose value is guaranteed to be all zeroes i.e. equivalent
        /// to <see cref="Guid.Empty"/>.
        /// </summary>
        public static readonly ShortGuid Empty = new(Guid.Empty);

        public const int StringLength = 22;

        readonly Guid underlyingGuid;
        readonly string encodedString;

        /// <summary>
        /// Creates a new instance with the given URL-safe Base64 encoded string.
        /// <para>See also <seealso cref="TryParse(string, out ShortGuid)"/> which will try to coerce the
        /// the value from URL-safe Base64 or normal Guid string.</para>
        /// </summary>
        /// <param name="value">A 22 character URL-safe Base64 encoded string to decode.</param>
        public ShortGuid(string value)
        {
            encodedString = value;
            underlyingGuid = Decode(value);
        }

        /// <summary>
        /// Creates a new instance with the given <see cref="System.Guid"/>.
        /// </summary>
        /// <param name="guid">The <see cref="System.Guid"/> to encode.</param>
        public ShortGuid(Guid guid)
        {
            encodedString = Encode(guid);
            underlyingGuid = guid;
        }

        /// <summary>
        /// Gets the underlying <see cref="System.Guid"/> for the encoded ShortGuid.
        /// </summary>
        public Guid Guid => underlyingGuid;

        /// <summary>
        /// Gets the encoded string value of the <see cref="Guid"/> as a URL-safe Base64 string.
        /// </summary>
        public string Value => encodedString;

        /// <summary>
        /// Returns the encoded URL-safe Base64 string.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => encodedString;

        /// <summary>
        /// Returns a value indicating whether this instance and a specified object represent the same type and value.
        /// <para>Compares for equality against other string, Guid and ShortGuid types.</para>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ShortGuid shortGuid)
            {
                return underlyingGuid.Equals(shortGuid.underlyingGuid);
            }

            if (obj is Guid guid)
            {
                return underlyingGuid.Equals(guid);
            }

            if (obj is string str)
            {
                // Try a ShortGuid string.
                if (TryDecode(str, out guid))
                    return underlyingGuid.Equals(guid);

                // Try a guid string.
                if (Guid.TryParse(str, out guid))
                    return underlyingGuid.Equals(guid);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for the underlying <see cref="System.Guid"/>.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => underlyingGuid.GetHashCode();

        /// <summary>
        /// Initialises a new instance of a ShortGuid using <see cref="Guid.NewGuid()"/>.
        /// <para>Equivalent of calling: <code>`new ShortGuid(Guid.NewGuid())`</code></para>
        /// </summary>
        /// <returns></returns>
        public static ShortGuid NewGuid() => new(Guid.NewGuid());

        /// <summary>
        /// Encodes the given value as an encoded ShortGuid string. The encoding is similar to Base64, with
        /// some non-URL safe characters replaced, and padding removed, resulting in a 22 character string.
        /// </summary>
        /// <param name="value">Any valid <see cref="System.Guid"/> string.</param>
        /// <returns>A 22 character ShortGuid URL-safe Base64 string.</returns>
        public static string Encode(string value)
        {
            var guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given <see cref="System.Guid"/> as an encoded ShortGuid string. The encoding is
        /// similar to Base64, with some non-URL safe characters replaced, and padding removed, resulting
        /// in a 22 character string.
        /// </summary>
        /// <param name="guid">The <see cref="System.Guid"/> to encode.</param>
        /// <returns>A 22 character ShortGuid URL-safe Base64 string.</returns>
        public static string Encode(Guid guid)
        {
#if NET40
            string encoded = Convert.ToBase64String(guid.ToByteArray());

            encoded = encoded
                .Replace("/", "_")
                .Replace("+", "-");

            return encoded.Substring(0, StringLength);
#else
            return Base64.Url.Encode(guid.ToByteArray());
#endif
        }

        /// <summary>
        /// Decodes the given value from a 22 character URL-safe Base64 string to a <see cref="System.Guid"/>.
        /// <para>Supports: ShortGuid format only.</para>
        /// <para>See also <seealso cref="TryDecode(string, out Guid)"/> or <seealso cref="TryParse(string, out Guid)"/>.</para>
        /// </summary>
        /// <param name="value">A 22 character URL-safe Base64 encoded string to decode.</param>
        /// <returns>A new <see cref="System.Guid"/> instance from the parsed string.</returns>
        /// <exception cref="FormatException">
        /// If <paramref name="value"/> is not a valid Base64 string (<seealso cref="Convert.FromBase64String(string)"/>)
        /// or if the decoded guid doesn't strictly match the input <paramref name="value"/>.
        /// </exception>
        public static Guid Decode(string value) => DecodeCore(value);

        static Guid DecodeCore(string? value)
        {
            // avoid parsing larger strings/blobs
            if (value?.Length != StringLength)
            {
                throw new ArgumentException(
                    $"A ShortGuid must be exactly 22 characters long. Received a {value?.Length ?? 0} character string.",
                    paramName: nameof(value)
                );
            }

#if NET40
            string base64 = value
                .Replace("_", "/")
                .Replace("-", "+") + "==";

            byte[] blob = Convert.FromBase64String(base64);
#else
            byte[] blob = Base64.Url.Decode(value.AsSpan());
#endif
            var guid = new Guid(blob);

            var sanityCheck = Encode(guid);
            if (sanityCheck != value)
            {
                throw new FormatException(
                    $"Invalid strict ShortGuid encoded string. The string '{value}' is valid URL-safe Base64, " +
                    $"but failed a round-trip test expecting '{sanityCheck}'."
                );
            }

            return guid;
        }

        /// <summary>
        /// <para>Supports ShortGuid format only.</para>
        /// <para>Attempts to decode the given value from a 22 character URL-safe Base64 string to
        /// a <see cref="System.Guid"/>.</para>
        /// <para>The difference between TryParse and TryDecode:</para>
        /// <list type="number">
        ///     <item>
        ///         <term><see cref="TryParse(string, out ShortGuid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryParse(string, out Guid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the underlying <see cref="System.Guid"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryDecode(string, out Guid)"/></term>
        ///         <description>Supports: ShortGuid;</description>
        ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
        ///         <see cref="ShortGuid"/> only, but outputs the result as a <see cref="System.Guid"/> - this method.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="value">The ShortGuid encoded string to decode.</param>
        /// <param name="guid">A new <see cref="System.Guid"/> instance from the parsed string.</param>
        /// <returns>A boolean indicating if the decode was successful.</returns>
        public static bool TryDecode(string? value, out Guid guid)
        {
            try
            {
                guid = DecodeCore(value);
                return true;
            }
            catch
            {
                guid = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// <para>Supports ShortGuid &amp; Guid formats.</para>
        /// <para>Tries to parse the value from either a 22 character URL-safe Base64 string or
        /// a <see cref="System.Guid"/> string, and outputs a <see cref="ShortGuid"/> instance.</para>
        /// <para>The difference between TryParse and TryDecode:</para>
        /// <list type="number">
        ///     <item>
        ///         <term><see cref="TryParse(string, out ShortGuid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid; </description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance - this method.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryParse(string, out Guid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="System.Guid"/>.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryDecode(string, out Guid)"/></term>
        ///         <description>Supports: ShortGuid;</description>
        ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
        ///         <see cref="ShortGuid"/> only, but outputs the result as a <see cref="System.Guid"/>.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="value">The ShortGuid encoded string or string representation of a Guid.</param>
        /// <param name="shortGuid">A new <see cref="ShortGuid"/> instance from the parsed string.</param>
        public static bool TryParse(string? value, out ShortGuid shortGuid)
        {
            // Try a ShortGuid string.
            if (TryDecode(value, out var guid))
            {
                shortGuid = guid;
                return true;
            }

            // Try a Guid string.
            if (value.TryParseGuid(out var guid2))
            {
                shortGuid = guid2;
                return true;
            }

            shortGuid = Empty;
            return false;
        }

        /// <summary>
        /// <para>Supports ShortGuid &amp; Guid formats.</para>
        /// <para>Tries to parse the value either a 22 character URL-safe Base64 string or
        /// <see cref="System.Guid"/> string, and outputs the <see cref="Guid"/> value.</para>
        /// <para>The difference between TryParse and TryDecode:</para>
        /// <list type="number">
        ///     <item>
        ///         <term><see cref="TryParse(string, out ShortGuid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="ShortGuid"/> instance.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryParse(string, out Guid)"/></term>
        ///         <description>Supports: Guid &amp; ShortGuid;</description>
        ///         <description>Tries to parse first as a <see cref="ShortGuid"/>, then as a
        ///         <see cref="System.Guid"/>, outputs the <see cref="System.Guid"/> - this method.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="TryDecode(string, out Guid)"/></term>
        ///         <description>Supports: ShortGuid;</description>
        ///         <description>Tries to decode a 22 character URL-safe Base64 string as a
        ///         <see cref="ShortGuid"/> only, outputting the result as a <see cref="System.Guid"/>.</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="value">The ShortGuid encoded string or string representation of a Guid.</param>
        /// <param name="guid">A new <see cref="System.Guid"/> instance from the parsed string.</param>
        /// <returns>A boolean indicating if the parse was successful.</returns>
        public static bool TryParse(string? value, out Guid guid)
        {
            // Try a ShortGuid string.
            if (TryDecode(value, out guid))
                return true;

            // Try a Guid string.
            if (value.TryParseGuid(out guid))
            {
                return true;
            }

            guid = Guid.Empty;
            return false;
        }

        #region Operators

        /// <summary>
        /// Determines if both ShortGuid instances have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator ==(ShortGuid x, ShortGuid y)
        {
            //if (ReferenceEquals(x, null))
            //    return ReferenceEquals(y, null);

            return x.underlyingGuid == y.underlyingGuid;
        }

        /// <summary>
        /// Determines if both instances have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator ==(ShortGuid x, Guid y)
        {
            //if (ReferenceEquals(x, null))
            //    return ReferenceEquals(y, null);

            return x.underlyingGuid == y;
        }

        /// <summary>
        /// Determines if both instances have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator ==(Guid x, ShortGuid y) => y == x; // NB: order of arguments

        /// <summary>
        /// Determines if both ShortGuid instances do not have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator !=(ShortGuid x, ShortGuid y) => !(x == y);

        /// <summary>
        /// Determines if both instances do not have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator !=(ShortGuid x, Guid y) => !(x == y);

        /// <summary>
        /// Determines if both instances do not have the same underlying <see cref="System.Guid"/> value.
        /// </summary>
        public static bool operator !=(Guid x, ShortGuid y) => !(x == y);

        /// <summary>
        /// Implicitly converts the ShortGuid to its string equivalent.
        /// </summary>
        public static implicit operator string(ShortGuid shortGuid) => shortGuid.encodedString;

        /// <summary>
        /// Implicitly converts the ShortGuid to its <see cref="System.Guid"/> equivalent.
        /// </summary>
        public static implicit operator Guid(ShortGuid shortGuid) => shortGuid.underlyingGuid;

        /// <summary>
        /// Implicitly converts the string to a ShortGuid.
        /// </summary>
        public static implicit operator ShortGuid(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Empty;

            if (TryParse(value, out ShortGuid shortGuid))
                return shortGuid;

            throw new FormatException("ShortGuid should contain 22 Base64 characters or "
                + "Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).");
        }

        /// <summary>
        /// Implicitly converts the <see cref="System.Guid"/> to a ShortGuid.
        /// </summary>
        public static implicit operator ShortGuid(Guid guid)
        {
            if (guid == Guid.Empty)
                return Empty;

            return new ShortGuid(guid);
        }

        #endregion
    }
}