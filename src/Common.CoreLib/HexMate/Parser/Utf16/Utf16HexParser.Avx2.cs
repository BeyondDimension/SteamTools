//#if NETCOREAPP3_0_OR_GREATER

//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Runtime.Intrinsics;
//using static HexMate.VectorConstants;
//using static HexMate.VectorUtils;
//using static System.Runtime.Intrinsics.X86.Avx;
//using static System.Runtime.Intrinsics.X86.Avx2;

//namespace HexMate;

//internal static partial class Utf16HexParser
//{
//    internal static unsafe class Avx2
//    {
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        internal static bool TryParse(ref char* srcBytes, ref byte* destBytes, int destLength)
//        {
//            Debug.Assert(System.Runtime.Intrinsics.X86.Avx2.IsSupported);

//            var x0F = Vector256.Create((byte)0x0F);
//            var xF0 = Vector256.Create((byte)0xF0);
//            var digHexSelector = ReadVector256(s_upperLowerDigHexSelector);
//            var digits = ReadVector256(s_digits);
//            var hexs = ReadVector256(s_hexs);
//            var evenBytes = ReadVector256(s_evenBytes);
//            var oddBytes = ReadVector256(s_oddBytes);
//            var src = (byte*)srcBytes;
//            var dest = destBytes;

//            var target = dest + FastMath.RoundDownTo32(destLength);
//            int leftOk, rightOk;
//            while (dest != target)
//            {
//                var a = LoadVector256(src).AsInt16();
//                src += 32;
//                var b = LoadVector256(src).AsInt16();
//                src += 32;
//                var c = LoadVector256(src).AsInt16();
//                src += 32;
//                var d = LoadVector256(src).AsInt16();
//                src += 32;

//                var ab = PackUnsignedSaturate(a, b);
//                var cd = PackUnsignedSaturate(c, d);

//                var inputLeft = Permute4x64(ab.AsUInt64(), 0b11_01_10_00).AsByte();
//                var inputRight = Permute4x64(cd.AsUInt64(), 0b11_01_10_00).AsByte();

//                var loNibbleLeft = And(inputLeft, x0F);
//                var loNibbleRight = And(inputRight, x0F);

//                var hiNibbleLeft = And(inputLeft, xF0);
//                var hiNibbleRight = And(inputRight, xF0);

//                var leftDigits = Shuffle(digits, loNibbleLeft);
//                var leftHex = Shuffle(hexs, loNibbleLeft);

//                var hiNibbleShLeft = ShiftRightLogical(hiNibbleLeft.AsInt16(), 4).AsByte();
//                var hiNibbleShRight = ShiftRightLogical(hiNibbleRight.AsInt16(), 4).AsByte();

//                var rightDigits = Shuffle(digits, loNibbleRight);
//                var rightHex = Shuffle(hexs, loNibbleRight);

//                var magicLeft = Shuffle(digHexSelector, hiNibbleShLeft);
//                var magicRight = Shuffle(digHexSelector, hiNibbleShRight);

//                var valueLeft = BlendVariable(leftDigits, leftHex, magicLeft);
//                var valueRight = BlendVariable(rightDigits, rightHex, magicRight);

//                var errLeft = ShiftLeftLogical(magicLeft.AsInt16(), 7).AsByte();
//                var errRight = ShiftLeftLogical(magicRight.AsInt16(), 7).AsByte();

//                var evenBytesLeft = Shuffle(valueLeft, evenBytes);
//                var oddBytesLeft = Shuffle(valueLeft, oddBytes);
//                var evenBytesRight = Shuffle(valueRight, evenBytes);
//                var oddBytesRight = Shuffle(valueRight, oddBytes);

//                evenBytesLeft = ShiftLeftLogical(evenBytesLeft.AsUInt16(), 4).AsByte();
//                evenBytesRight = ShiftLeftLogical(evenBytesRight.AsUInt16(), 4).AsByte();

//                evenBytesLeft = Or(evenBytesLeft, oddBytesLeft);
//                evenBytesRight = Or(evenBytesRight, oddBytesRight);

//                var result = Merge(evenBytesLeft, evenBytesRight);

//                var validationResultLeft = Or(errLeft, valueLeft);
//                var validationResultRight = Or(errRight, valueRight);

//                leftOk = MoveMask(validationResultLeft);
//                rightOk = MoveMask(validationResultRight);

//                if ((leftOk | rightOk) != 0) goto Err;

//                Store(dest, result);
//                dest += 32;
//            }

//            srcBytes = (char*)src;
//            destBytes = dest;
//            return true;

//        Err:
//            if (leftOk != 0)
//            {
//                srcBytes = (char*)(src - 128);
//                destBytes = dest;
//                return false;
//            }
//            else
//            {
//                Debug.Assert(rightOk != 0);
//                srcBytes = (char*)(src - 64);
//                destBytes = dest;
//                return false;
//            }
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private static Vector256<byte> Merge(Vector256<byte> a, Vector256<byte> b)
//        {
//            var a1 = Permute4x64(a.AsUInt64(), 0b11_10_10_00);
//            var b1 = Permute4x64(b.AsUInt64(), 0b11_00_01_00);
//            return Blend(a1.AsUInt32(), b1.AsUInt32(), 0b1111_0000).AsByte();
//        }
//    }
//}

//#endif