/*
 * Copyright (C) 2011 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WinAuth
{
    /// <summary>
    /// Class that implements conversion to Base32 RFC3548
    /// </summary>
    public class WinAuthBase32
    {
        /// <summary>
        /// Default base32 character set as per RFC 4648/3548
        /// </summary>
        const string DefaultAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary>
        /// Lookup for the number of zero bits on the right (e.g. 0100 = 2)
        /// </summary>
        static readonly int[] NumberTrailingZerosLookup =
        {
            32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17,
            0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18
        };

        /// <summary>
        /// Singleton instance
        /// </summary>
        static readonly WinAuthBase32 _instance = new(DefaultAlphabet);

        /// <summary>
        /// Get an instance of a Base32 object, either the standard singleton or for a custom alphabet
        /// </summary>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static WinAuthBase32 GetInstance(string? alphabet = null)
        {
            return alphabet == null ? _instance : new WinAuthBase32(alphabet);
        }

        /// <summary>
        /// array of alaphabet chars
        /// </summary>
        readonly char[] _digits;

        /// <summary>
        /// bit mask used
        /// </summary>
        readonly int _mask;

        /// <summary>
        /// shifting value
        /// </summary>
        readonly int _shift;

        /// <summary>
        /// map of chars to position
        /// </summary>
        readonly Dictionary<char, int> _map;

        /// <summary>
        /// Create a new Base32 object with a specified alphabet
        /// </summary>
        /// <param name="alphabet"></param>
        protected WinAuthBase32(string alphabet)
        {
            // initialise the decoder and precalculate the char map
            _digits = alphabet.ToCharArray();
            _mask = _digits.Length - 1;
            _shift = NumberOfTrailingZeros(_digits.Length);
            _map = new Dictionary<char, int>();
            for (var i = 0; i < _digits.Length; i++)
            {
                _map.Add(_digits[i], i);
            }
        }

        /// <summary>
        /// Calculate the number of zero trailing bits on the right (e.g. 0100 = 2)
        /// http://graphics.stanford.edu/~seander/bithacks.html#ZerosOnRightModLookup
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        static int NumberOfTrailingZeros(int i)
        {
            return NumberTrailingZerosLookup[(i & -i) % 37];
        }

        public byte[] Decode(string encoded)
        {
            // remove whitespace and any separators
            encoded = Regex.Replace(encoded, @"[\s-]+", "");

            // Google implementation ignores padding
            encoded = Regex.Replace(encoded, @"[=]*$", "");

            // convert as uppercase
            encoded = encoded.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

            // handle zero case
            if (encoded.Length == 0)
            {
                return new byte[0];
            }

            var encodedLength = encoded.Length;
            var outLength = encodedLength * _shift / 8;
            var result = new byte[outLength];
            var buffer = 0;
            var next = 0;
            var bitsLeft = 0;
            foreach (var c in encoded.ToCharArray())
            {
                if (_map.ContainsKey(c) == false)
                {
                    throw new WinAuthBase32DecodingException("Illegal character: " + c);
                }
                buffer <<= _shift;
                buffer |= _map[c] & _mask;
                bitsLeft += _shift;
                if (bitsLeft >= 8)
                {
                    result[next++] = (byte)(buffer >> (bitsLeft - 8));
                    bitsLeft -= 8;
                }
            }
            // We'll ignore leftover bits for now.
            //
            // if (next != outLength || bitsLeft >= SHIFT) {
            //  throw new DecodingException("Bits left: " + bitsLeft);
            // }

            return result;
        }

        public string Encode(byte[] data)
        {
            if (data.Length == 0)
            {
                return string.Empty;
            }

            /*
                        // _shift is the number of bits per output character, so the length of the
                        // output is the length of the input multiplied by 8/_shift, rounded up.
                        if (data.Length >= (1 << 28))
                        {
                            // The computation below will fail, so don't do it.
                            throw new IllegalArgumentException();
                        }

                        // calculate result length
                        int outputLength = (data.Length * 8 + _shift - 1) / _shift;
                        StringBuilder result = new StringBuilder(outputLength);
            */

            var result = new StringBuilder();

            // encode data and map chars into result buffer
            int buffer = data[0];
            var next = 1;
            var bitsLeft = 8;
            while (bitsLeft > 0 || next < data.Length)
            {
                if (bitsLeft < _shift)
                {
                    if (next < data.Length)
                    {
                        buffer <<= 8;
                        buffer |= data[next++] & 0xff;
                        bitsLeft += 8;
                    }
                    else
                    {
                        var pad = _shift - bitsLeft;
                        buffer <<= pad;
                        bitsLeft += pad;
                    }
                }
                var index = _mask & (buffer >> (bitsLeft - _shift));
                bitsLeft -= _shift;
                result.Append(_digits[index]);
            }

            return result.ToString();
        }
    }
}