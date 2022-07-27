using System.Diagnostics;
#if NETCOREAPP3_0_OR_GREATER
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
#endif

namespace HexMate;

internal static unsafe class CharHelper
{
    internal static int CountUsefulCharacters(char* inputPtr, int inputLength)
    {
        Debug.Assert(inputLength > 0);

        // All characters above ' ' (space) are considered useful, as it is good enough, if this method
        // returns an upper bound on the number of bytes encoded in the given input.
        var byteLength = inputLength * 2;
        var usefulCharacters = 0;
#if NETCOREAPP3_0_OR_GREATER
        var byteSrc = (byte*)inputPtr;

        if (Avx2.IsSupported && byteLength >= 32)
        {
            var space = Vector256.Create(' ').AsInt16();
            var bytesToProcess = FastMath.RoundDownTo32(byteLength);
            var end = byteSrc + bytesToProcess;
            while (byteSrc != end)
            {
                var input = Avx.LoadVector256(byteSrc).AsInt16();

                var cnt = Avx2.CompareGreaterThan(input, space);
                var mask = (uint)Avx2.MoveMask(cnt.AsByte());
                usefulCharacters += BitOperations.PopCount(mask) >> 1;

                byteSrc += 32;
            }

            byteLength -= bytesToProcess;
        }

        if (Sse2.IsSupported && byteLength >= 16)
        {
            var space = Vector128.Create(' ').AsInt16();
            var bytesToProcess = FastMath.RoundDownTo16(byteLength);
            var end = byteSrc + bytesToProcess;
            while (byteSrc != end)
            {
                var input = Sse2.LoadVector128(byteSrc).AsInt16();

                var cnt = Sse2.CompareGreaterThan(input, space);
                var mask = (uint)Sse2.MoveMask(cnt.AsByte());
                usefulCharacters += BitOperations.PopCount(mask) >> 1;

                byteSrc += 16;
            }

            byteLength -= bytesToProcess;
        }

        inputPtr = (char*)byteSrc;
#endif
        const uint intSpace = ' ';

        var charEnd = inputPtr + (byteLength / 2);
        while (inputPtr < charEnd)
        {
            uint c = *inputPtr++;

            if (c > intSpace)
                usefulCharacters++;
        }

        return usefulCharacters;
    }
}