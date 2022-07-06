using System.Diagnostics;
using static HexMate.ScalarConstants;

namespace HexMate;

internal static partial class Utf16HexFormatter
{
    internal static unsafe class Fixed
    {
        internal static int Format(ref byte* srcBytes, ref char* destBytes, int srcLength, bool toLower = false)
        {
            Debug.Assert(srcLength > 0);

            var src = srcBytes;
            var dest = destBytes;

            var bytesToRead = srcLength;
            var charsToWrite = bytesToRead << 1;

            var start = dest - charsToWrite;

            var destShort = (uint*)dest;

            fixed (uint* lut =
                toLower
                    ? (BitConverter.IsLittleEndian ? s_utf16LookupHexLowerLE : s_utf16LookupHexLowerBE)
                    : (BitConverter.IsLittleEndian ? s_utf16LookupHexUpperLE : s_utf16LookupHexUpperBE))
            {
                do
                {
                    *(--destShort) = lut[*(--src)];
                } while (destShort != start);
            }

            srcBytes = src;
            destBytes = (char*)destShort;

            return bytesToRead;
        }
    }
}