//
// Mono.Security.BitConverterLE.cs
//  Like System.BitConverter but always little endian
//
// Author:
//   Bernie Solomon
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Titanium.Web.Proxy.Network.WinAuth.Security
{
    internal sealed class LittleEndian
    {
        private LittleEndian()
        {
        }

        private static unsafe byte[] GetUShortBytes(byte* bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return new[] { bytes[0], bytes[1] };
            }

            return new[] { bytes[1], bytes[0] };
        }

        private static unsafe byte[] GetUIntBytes(byte* bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return new[] { bytes[0], bytes[1], bytes[2], bytes[3] };
            }

            return new[] { bytes[3], bytes[2], bytes[1], bytes[0] };
        }

        private static unsafe byte[] GetULongBytes(byte* bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                return new[] { bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6], bytes[7] };
            }

            return new[] { bytes[7], bytes[6], bytes[5], bytes[4], bytes[3], bytes[2], bytes[1], bytes[0] };
        }

        internal static byte[] GetBytes(bool value)
        {
            return new[] { value ? (byte)1 : (byte)0 };
        }

        internal static unsafe byte[] GetBytes(char value)
        {
            return GetUShortBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(short value)
        {
            return GetUShortBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(int value)
        {
            return GetUIntBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(long value)
        {
            return GetULongBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(ushort value)
        {
            return GetUShortBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(uint value)
        {
            return GetUIntBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(ulong value)
        {
            return GetULongBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(float value)
        {
            return GetUIntBytes((byte*)&value);
        }

        internal static unsafe byte[] GetBytes(double value)
        {
            return GetULongBytes((byte*)&value);
        }

        private static unsafe void UShortFromBytes(byte* dst, byte[] src, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                dst[0] = src[startIndex];
                dst[1] = src[startIndex + 1];
            }
            else
            {
                dst[0] = src[startIndex + 1];
                dst[1] = src[startIndex];
            }
        }

        private static unsafe void UIntFromBytes(byte* dst, byte[] src, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                dst[0] = src[startIndex];
                dst[1] = src[startIndex + 1];
                dst[2] = src[startIndex + 2];
                dst[3] = src[startIndex + 3];
            }
            else
            {
                dst[0] = src[startIndex + 3];
                dst[1] = src[startIndex + 2];
                dst[2] = src[startIndex + 1];
                dst[3] = src[startIndex];
            }
        }

        private static unsafe void ULongFromBytes(byte* dst, byte[] src, int startIndex)
        {
            if (BitConverter.IsLittleEndian)
            {
                for (int i = 0; i < 8; ++i)
                {
                    dst[i] = src[startIndex + i];
                }
            }
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    dst[i] = src[startIndex + (7 - i)];
                }
            }
        }

        internal static bool ToBoolean(byte[] value, int startIndex)
        {
            return value[startIndex] != 0;
        }

        internal static unsafe char ToChar(byte[] value, int startIndex)
        {
            char ret;

            UShortFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe short ToInt16(byte[] value, int startIndex)
        {
            short ret;

            UShortFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe int ToInt32(byte[] value, int startIndex)
        {
            int ret;

            UIntFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe long ToInt64(byte[] value, int startIndex)
        {
            long ret;

            ULongFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe ushort ToUInt16(byte[] value, int startIndex)
        {
            ushort ret;

            UShortFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe uint ToUInt32(byte[] value, int startIndex)
        {
            uint ret;

            UIntFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe ulong ToUInt64(byte[] value, int startIndex)
        {
            ulong ret;

            ULongFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe float ToSingle(byte[] value, int startIndex)
        {
            float ret;

            UIntFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }

        internal static unsafe double ToDouble(byte[] value, int startIndex)
        {
            double ret;

            ULongFromBytes((byte*)&ret, value, startIndex);

            return ret;
        }
    }
}
