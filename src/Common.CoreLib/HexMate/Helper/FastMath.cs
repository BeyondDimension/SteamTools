using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HexMate;

static class FastMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RoundDownTo32(int x)
    {
        Debug.Assert(x >= 0);
        return x & 0x7FFFFFE0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int RoundDownTo16(int x)
    {
        Debug.Assert(x >= 0);
        return x & 0x7FFFFFF0;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal static int RoundDownTo2(int x)
    //{
    //    Debug.Assert(x >= 0);
    //    return x & 0x7FFFFFFE;
    //}
}