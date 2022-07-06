#if NETCOREAPP3_0_OR_GREATER

using System.Diagnostics;
using System.Runtime.Intrinsics;
using static HexMate.VectorConstants;
using static HexMate.VectorUtils;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Ssse3;

namespace HexMate;

internal static partial class Utf16HexFormatter
{
    internal static unsafe class Ssse3
    {
        internal static int Format(ref byte* srcBytes, ref char* destBytes, int srcLength, bool toLower = false)
        {
            Debug.Assert(System.Runtime.Intrinsics.X86.Ssse3.IsSupported);
            Debug.Assert(srcLength >= 16);

            var x00 = Vector128<byte>.Zero;
            var x0F = Vector128.Create((byte)0x0F);
            var lowerHexLookupTable = ReadVector128(s_lowerHexLookupTable);
            var upperHexLookupTable = ReadVector128(s_upperHexLookupTable);
            var hexLookupTable = toLower ? lowerHexLookupTable : upperHexLookupTable;

            var src = srcBytes;
            var dest = (byte*)destBytes;

            var bytesToRead = FastMath.RoundDownTo16(srcLength);
            var bytesToWrite = bytesToRead << 2;

            var start = dest - bytesToWrite;

            do
            {
                src -= 16;
                var value = LoadVector128(src);

                var hiShift = ShiftRightLogical(value.AsInt16(), 4).AsByte();
                var hiHalf = And(hiShift, x0F);
                var loHalf = And(value, x0F);
                var hi = UnpackHigh(hiHalf, loHalf);
                var lo = UnpackLow(hiHalf, loHalf);

                var resHi = Shuffle(hexLookupTable, hi);
                var resLo = Shuffle(hexLookupTable, lo);

                var ef = UnpackLow(resHi, x00);
                var gh = UnpackHigh(resHi, x00);
                var ab = UnpackLow(resLo, x00);
                var cd = UnpackHigh(resLo, x00);

                dest -= 16;
                Store(dest, gh);
                dest -= 16;
                Store(dest, ef);

                dest -= 16;
                Store(dest, cd);
                dest -= 16;
                Store(dest, ab);
            } while (dest != start);

            srcBytes = src;
            destBytes = (char*)dest;

            return bytesToRead;
        }
    }
}

#endif