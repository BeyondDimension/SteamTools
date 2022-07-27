using System.Runtime.CompilerServices;
using static HexMate.ScalarConstants;

namespace HexMate;

internal static partial class Utf16HexParser
{
    internal static unsafe class Fixed
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryParse(ref char* srcBytes, int srcLength, ref byte* destBytes, int destLength)
        {
            var src = srcBytes;
            var dest = destBytes;

            var srcTarget = src + srcLength;
            var destTarget = dest + destLength;

            fixed (byte* lut = LookupUpperLower)
            {
                int hi, lo;
                while (srcTarget > src && destTarget > dest)
                {
                ReadHi:
                    int valHi = *src++;
                    if (valHi > 0xFF) { goto Err; }
                    hi = lut[valHi];
                    if (hi >= 0xFE)
                    {
                        if (hi == 0xFF) goto Err;
                        if (srcTarget > src) goto ReadHi; // skip whitespace
                    }
                    *dest |= (byte)(hi << 4);

                ReadLo:
                    int valLo = *src++;
                    if (valLo > 0xFF) goto Err;
                    lo = lut[valLo];
                    if (lo >= 0xFE)
                    {
                        if (lo == 0xFF) goto Err;
                        if (srcTarget > src) goto ReadLo; // skip whitespace
                    }
                    *dest |= (byte)lo;

                    dest++;
                }
            }

            srcBytes = src;
            destBytes = dest;
            return true;

        Err:
            srcBytes = src - 1;
            destBytes = dest;
            return false;
        }
    }
}