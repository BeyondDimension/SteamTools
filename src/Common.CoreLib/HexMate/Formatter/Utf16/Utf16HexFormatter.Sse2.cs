#if NETCOREAPP3_0_OR_GREATER

using System.Diagnostics;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Sse2;

namespace HexMate;

internal static partial class Utf16HexFormatter
{
    internal static unsafe class Sse2
    {
        internal static int Format(ref byte* srcBytes, ref char* destBytes, int srcLength, bool toLower = false)
        {
            Debug.Assert(IsSupported);
            Debug.Assert(srcLength >= 16);

            var x00 = Vector128<byte>.Zero;
            var x0F = Vector128.Create((byte)0x0F);
            var x30 = Vector128.Create((byte)0x30);
            var x09 = Vector128.Create((byte)0x09);
            var corr = toLower ? Vector128.Create((byte)0x27) : Vector128.Create((byte)0x07);

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

                var cHi = Add(hi, x30);
                var cLo = Add(lo, x30);

                var maskHi = CompareGreaterThan(hi.AsSByte(), x09.AsSByte()).AsByte();
                var maskLo = CompareGreaterThan(lo.AsSByte(), x09.AsSByte()).AsByte();

                var corrHi = And(maskHi, corr);
                var corrLo = And(maskLo, corr);

                var resHi = Add(cHi, corrHi);
                var resLo = Add(cLo, corrLo);

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