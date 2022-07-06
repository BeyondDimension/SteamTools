#if NETCOREAPP3_0_OR_GREATER

using System.Diagnostics;
using System.Runtime.Intrinsics;
using static HexMate.VectorConstants;
using static HexMate.VectorUtils;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Avx2;

namespace HexMate;

internal static partial class Utf16HexFormatter
{
    internal static unsafe class Avx2
    {
        internal static int Format(ref byte* srcBytes, ref char* destBytes, int srcLength, bool toLower = false)
        {
            Debug.Assert(System.Runtime.Intrinsics.X86.Avx2.IsSupported);
            Debug.Assert(srcLength >= 32);

            var x00 = Vector256<byte>.Zero;
            var x0F = Vector256.Create((byte)0x0F);
            var lowerHexLookupTable = ReadVector256(s_lowerHexLookupTable);
            var upperHexLookupTable = ReadVector256(s_upperHexLookupTable);
            var hexLookupTable = toLower ? lowerHexLookupTable : upperHexLookupTable;

            var src = srcBytes;
            var dest = (byte*)destBytes;

            var bytesToRead = FastMath.RoundDownTo32(srcLength);
            var bytesToWrite = bytesToRead << 2;

            var start = dest - bytesToWrite;
            do
            {
                src -= 32;
                var value = LoadVector256(src);

                var hiShift = ShiftRightLogical(value.AsInt16(), 4).AsByte();
                var hiHalf = And(hiShift, x0F);
                var loHalf = And(value, x0F);
                var hi13 = UnpackHigh(hiHalf, loHalf);
                var lo02 = UnpackLow(hiHalf, loHalf);

                var resHi = Shuffle(hexLookupTable, hi13);
                var resLo = Shuffle(hexLookupTable, lo02);

                var cg = UnpackLow(resHi, x00);
                var dh = UnpackHigh(resHi, x00);
                var ae = UnpackLow(resLo, x00);
                var bf = UnpackHigh(resLo, x00);

                var cd = Permute2x128(cg, dh, 0b0010_0000);
                var gh = Permute2x128(cg, dh, 0b0011_0001);
                var ab = Permute2x128(ae, bf, 0b0010_0000);
                var ef = Permute2x128(ae, bf, 0b0011_0001);

                dest -= 32;
                Store(dest, gh);
                dest -= 32;
                Store(dest, ef);
                dest -= 32;
                Store(dest, cd);
                dest -= 32;
                Store(dest, ab);
            } while (dest != start);

            srcBytes = src;
            destBytes = (char*)dest;

            return bytesToRead;
        }
    }

}

#endif