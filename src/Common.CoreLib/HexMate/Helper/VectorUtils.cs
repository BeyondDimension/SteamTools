#if NETCOREAPP3_0_OR_GREATER

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace HexMate;

static class VectorUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> ReadVector128(ReadOnlySpan<byte> data)
        => Unsafe.ReadUnaligned<Vector128<byte>>(ref MemoryMarshal.GetReference(data));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector256<byte> ReadVector256(ReadOnlySpan<byte> data)
    {
        var const128 = ReadVector128(data);
        return Vector256.Create(const128, const128);
    }
}

#endif