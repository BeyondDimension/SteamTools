//#if NETCOREAPP3_0_OR_GREATER

//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Runtime.Intrinsics;
//using static System.Runtime.Intrinsics.X86.Sse2;

//namespace HexMate;

//internal static partial class Utf16HexParser
//{
//    internal static unsafe class Sse2
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        internal static bool TryParse(ref char* srcBytes, ref byte* destBytes, int destLength)
//        {
//            Debug.Assert(System.Runtime.Intrinsics.X86.Sse2.IsSupported);

//            var x05 = Vector128.Create((byte)0x05);
//            var x09 = Vector128.Create((byte)0x09);
//            var x0A = Vector128.Create((byte)0x0A);
//            var x20 = Vector128.Create((byte)0x20);
//            var x30 = Vector128.Create((byte)0x30);
//            var x40 = Vector128.Create((byte)0x40);
//            var x41 = Vector128.Create((byte)0x41);
//            var x60 = Vector128.Create((byte)0x60);
//            var xFF = Vector128.Create((byte)0xFF);
//            var src = (byte*)srcBytes;
//            var dest = destBytes;

//            var target = dest + FastMath.RoundDownTo16(destLength);
//            int leftOk, rightOk;
//            while (dest != target)
//            {
//                var a = LoadVector128(src).AsInt16();
//                src += 16;
//                var b = LoadVector128(src).AsInt16();
//                src += 16;
//                var c = LoadVector128(src).AsInt16();
//                src += 16;
//                var d = LoadVector128(src).AsInt16();
//                src += 16;

//                var inputLeft = PackUnsignedSaturate(a, b);
//                var inputRight = PackUnsignedSaturate(c, d);

//                var g40Left = CompareGreaterThan(inputLeft.AsSByte(), x40.AsSByte()).AsByte();
//                var g60Left = CompareGreaterThan(inputLeft.AsSByte(), x60.AsSByte()).AsByte();
//                var g40Right = CompareGreaterThan(inputRight.AsSByte(), x40.AsSByte()).AsByte();
//                var g60Right = CompareGreaterThan(inputRight.AsSByte(), x60.AsSByte()).AsByte();

//                var corr40Left = And(x41, g40Left);
//                var corr60Left = And(x20, g60Left);

//                var corr40Right = And(x41, g40Right);
//                var corr60Right = And(x20, g60Right);

//                var sub30Left = Subtract(inputLeft, x30);
//                var sub40Left = Subtract(inputLeft, corr40Left);

//                var sub30Right = Subtract(inputRight, x30);
//                var sub40Right = Subtract(inputRight, corr40Right);

//                var sub60Left = Subtract(sub40Left, corr60Left);
//                var sub60Right = Subtract(sub40Right, corr60Right);

//                var maskAfLeft = Or(g60Left, g40Left);
//                var maskAfRight = Or(g60Right, g40Right);

//                var maskDigLeft = Xor(maskAfLeft, xFF);
//                var maskDigRight = Xor(maskAfRight, xFF);

//                var afLeft = And(sub60Left, maskAfLeft);
//                var corrAfLeft = And(maskAfLeft, x0A);

//                var afRight = And(sub60Right, maskAfRight);
//                var corrAfRight = And(maskAfRight, x0A);

//                var digLeft = And(sub30Left, maskDigLeft);
//                var corrLeft = Add(afLeft, corrAfLeft);

//                var digRight = And(sub30Right, maskDigRight);
//                var corrRight = Add(afRight, corrAfRight);

//                var resultAfLeft = CompareGreaterThan(afLeft.AsSByte(), x05.AsSByte()).AsByte();
//                var resultDigLeft = CompareGreaterThan(digLeft.AsSByte(), x09.AsSByte()).AsByte();

//                var resultAfRight = CompareGreaterThan(afRight.AsSByte(), x05.AsSByte()).AsByte();
//                var resultDigRight = CompareGreaterThan(digRight.AsSByte(), x09.AsSByte()).AsByte();

//                var valueLeft = Or(corrLeft, digLeft);
//                var valueRight = Or(corrRight, digRight);

//                var digAfLeft = Or(digLeft, afLeft);
//                var digAfRight = Or(digRight, afRight);

//                var digAfOutOfRangeLeft = Or(resultAfLeft, resultDigLeft);
//                var digAfOutOfRangeRight = Or(resultAfRight, resultDigRight);

//                var resLeft = Or(digAfLeft, digAfOutOfRangeLeft);
//                var resRight = Or(digAfRight, digAfOutOfRangeRight);

//                var sh12Left = ShiftLeftLogical(valueLeft.AsUInt16(), 12).AsByte();
//                var sh12Right = ShiftLeftLogical(valueRight.AsUInt16(), 12).AsByte();

//                var unp1 = ShiftRightLogical(Or(valueLeft, sh12Left).AsUInt16(), 8).AsByte();
//                var unp2 = ShiftRightLogical(Or(valueRight, sh12Right).AsUInt16(), 8).AsByte();

//                var result = PackUnsignedSaturate(unp1.AsInt16(), unp2.AsInt16()).AsByte();

//                leftOk = MoveMask(resLeft);
//                rightOk = MoveMask(resRight);

//                if ((leftOk | rightOk) != 0) goto Err;

//                Store(dest, result);
//                dest += 16;
//            }

//            srcBytes = (char*)src;
//            destBytes = dest;
//            return true;

//        Err:
//            if (leftOk != 0)
//            {
//                srcBytes = (char*)(src - 64);
//                destBytes = dest;
//                return false;
//            }
//            else
//            {
//                Debug.Assert(rightOk != 0);
//                srcBytes = (char*)(src - 32);
//                destBytes = dest;
//                return false;
//            }
//        }
//    }
//}

//#endif