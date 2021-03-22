using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ByteArrayExtensions
    {
        public static sbyte[] ToSByteArray(this byte[] buffer)
        {
            ReadOnlySpan<byte> _buffer = buffer;
            return _buffer.ToSByteArray();
        }

        public static sbyte[] ToSByteArray(this ReadOnlySpan<byte> buffer)
            => MemoryMarshal.Cast<byte, sbyte>(buffer).ToArray();

        public static byte[] ToByteArray(this sbyte[] buffer)
        {
            ReadOnlySpan<sbyte> _buffer = buffer;
            return _buffer.ToByteArray();
        }

        public static byte[] ToByteArray(this ReadOnlySpan<sbyte> buffer)
            => MemoryMarshal.Cast<sbyte, byte>(buffer).ToArray();
    }
}