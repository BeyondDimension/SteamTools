using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ByteArrayExtensions
    {
        public static sbyte[] ToSByteArray(this byte[] buffer)
        {
            ReadOnlySpan<byte> buffer_ = buffer;
            return buffer_.ToSByteArray();
        }

        public static sbyte[] ToSByteArray(this ReadOnlySpan<byte> buffer)
            => MemoryMarshal.Cast<byte, sbyte>(buffer).ToArray();

        public static byte[] ToByteArray(this sbyte[] buffer)
        {
            ReadOnlySpan<sbyte> buffer_ = buffer;
            return buffer_.ToByteArray();
        }

        public static byte[] ToByteArray(this ReadOnlySpan<sbyte> buffer)
            => MemoryMarshal.Cast<sbyte, byte>(buffer).ToArray();

        [return: NotNullIfNotNull("buffer")]
        public static byte[]? CompressByteArray(this byte[]? buffer)
        {
            if (buffer == null) return null;

            using var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);

            return gZipBuffer;
        }

        [return: NotNullIfNotNull("buffer")]
        public static byte[]? CompressByteArrayByBrotli(this byte[]? buffer)
        {
            if (buffer == null) return null;

            using var memoryStream = new MemoryStream();
            using (var gZipStream = new BrotliStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);

            return gZipBuffer;
        }

        [return: NotNullIfNotNull("gZipBuffer")]
        public static byte[]? DecompressByteArray(this byte[]? gZipBuffer)
        {
            if (gZipBuffer == null) return null;

            using var memoryStream = new MemoryStream();
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];

            memoryStream.Position = 0;
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return buffer;
        }

        [return: NotNullIfNotNull("gZipBuffer")]
        public static byte[]? DecompressByteArrayByBrotli(this byte[]? gZipBuffer)
        {
            if (gZipBuffer == null) return null;

            using var memoryStream = new MemoryStream();
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];

            memoryStream.Position = 0;
            using (var gZipStream = new BrotliStream(memoryStream, CompressionMode.Decompress))
            {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return buffer;
        }
    }
}