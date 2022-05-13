using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System
{
    public static class Enum2
    {
        static TOut Convert<TOut, TIn>(TIn value)
        {
            var parameter = Expression.Parameter(typeof(TIn), null);
            var dynamicMethod = Expression.Lambda<Func<TIn, TOut>>(
                Expression.Convert(parameter, typeof(TOut)),
                parameter);
            return dynamicMethod.Compile()(value);
        }

        [MethodImpl((MethodImplOptions)256)]
        static bool HasFlags(this int @this, int flag) => (@this & flag) == flag;

        [MethodImpl((MethodImplOptions)256)]
        static bool HasFlags(this long @this, long flag) => (@this & flag) == flag;

        [MethodImpl((MethodImplOptions)256)]
        static bool HasFlags(this uint @this, uint flag) => (@this & flag) == flag;

        [MethodImpl((MethodImplOptions)256)]
        static bool HasFlags(this ulong @this, ulong flag) => (@this & flag) == flag;

        public static bool HasFlags<T>(this T @this, T flag) where T : Enum
        {
            var typeCode = @this.GetTypeCode();
            return typeCode switch
            {
                TypeCode.Byte => HasFlags(Convert<byte, T>(@this), Convert<byte, T>(flag)),
                TypeCode.Int16 => HasFlags(Convert<short, T>(@this), Convert<short, T>(flag)),
                TypeCode.Int32 => HasFlags(Convert<int, T>(@this), Convert<int, T>(flag)),
                TypeCode.Int64 => HasFlags(Convert<long, T>(@this), Convert<long, T>(flag)),
                TypeCode.SByte => HasFlags(Convert<sbyte, T>(@this), Convert<sbyte, T>(flag)),
                TypeCode.UInt16 => HasFlags(Convert<ushort, T>(@this), Convert<ushort, T>(flag)),
                TypeCode.UInt32 => HasFlags(Convert<uint, T>(@this), Convert<uint, T>(flag)),
                TypeCode.UInt64 => HasFlags(Convert<ulong, T>(@this), Convert<ulong, T>(flag)),
                _ => throw new ArgumentOutOfRangeException(nameof(typeCode), typeCode, null),
            };
        }
    }
}
