using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class StreamExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadValueU8(this Stream stream)
    {
        return (byte)stream.ReadByte();
    }

    public static int ReadValueS32(this Stream stream)
    {
        var data = new byte[4];
        int read = stream.Read(data, 0, 4);
        Debug.Assert(read == 4);
        return BitConverter.ToInt32(data, 0);
    }

    public static uint ReadValueU32(this Stream stream)
    {
        var data = new byte[4];
        int read = stream.Read(data, 0, 4);
        Debug.Assert(read == 4);
        return BitConverter.ToUInt32(data, 0);
    }

    public static ulong ReadValueU64(this Stream stream)
    {
        var data = new byte[8];
        int read = stream.Read(data, 0, 8);
        Debug.Assert(read == 8);
        return BitConverter.ToUInt64(data, 0);
    }

    public static float ReadValueF32(this Stream stream)
    {
        var data = new byte[4];
        int read = stream.Read(data, 0, 4);
        Debug.Assert(read == 4);
        return BitConverter.ToSingle(data, 0);
    }

    internal static string ReadStringInternalDynamic(this Stream stream, Encoding encoding, char end)
    {
        int characterSize = encoding.GetByteCount("e");
        Debug.Assert(characterSize == 1 || characterSize == 2 || characterSize == 4);
        string characterEnd = end.ToString(CultureInfo.InvariantCulture);

        int i = 0;
        var data = new byte[128 * characterSize];

        while (true)
        {
            if (i + characterSize > data.Length)
            {
                Array.Resize(ref data, data.Length + (128 * characterSize));
            }

            int read = stream.Read(data, i, characterSize);
            Debug.Assert(read == characterSize);

            if (encoding.GetString(data, i, characterSize) == characterEnd)
            {
                break;
            }

            i += characterSize;
        }

        if (i == 0)
        {
            return "";
        }

        return encoding.GetString(data, 0, i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadStringAscii(this Stream stream)
    {
        return stream.ReadStringInternalDynamic(Encoding.ASCII, '\0');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadStringUnicode(this Stream stream)
    {
        return stream.ReadStringInternalDynamic(Encoding.UTF8, '\0');
    }

    const int DefaultBufferSize = 1024;

    public static StreamWriter GetWriter(this Stream stream, Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false)
    {
        try
        {
            // https://docs.microsoft.com/zh-cn/dotnet/api/system.io.streamwriter.-ctor?view=net-5.0#System_IO_StreamWriter__ctor_System_IO_Stream_System_Text_Encoding_System_Int32_System_Boolean_
            // https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/StreamWriter.cs#L94
            return new(stream, encoding, bufferSize, leaveOpen);
        }
        catch (Exception e) when (e is ArgumentNullException || e is ArgumentOutOfRangeException)
        {
            if (encoding == null)
            {
                encoding = EncodingCache.UTF8NoBOM;
            }
            if (bufferSize == -1)
            {
                bufferSize = DefaultBufferSize;
            }
            return new(stream, encoding, bufferSize, leaveOpen);
        }
    }

    public static byte[] ToByteArray(this Stream stream)
    {
        if (stream is MemoryStream ms) return ms.ToArray();

        //try
        //{
        //    var len = stream.Length;
        //    var bytes = new byte[len];
        //    stream.Seek(0, SeekOrigin.Begin);
        //    stream.Read(bytes, 0, bytes.Length);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return bytes;
        //}
        //catch
        //{
        using var ms2 = new MemoryStream();
        stream.CopyTo(ms2);
        return ms2.ToArray();
        //}
    }

    public static async ValueTask<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
    {
        if (stream is MemoryStream ms) return ms.ToArray();

        //try
        //{
        //    var len = stream.Length;
        //    var bytes = new byte[len];
        //    stream.Seek(0, SeekOrigin.Begin);
        //    await stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken);
        //    stream.Seek(0, SeekOrigin.Begin);
        //    return bytes;
        //}
        //catch
        //{
        using var ms2 = new MemoryStream();
        await stream.CopyToAsync(ms2, cancellationToken);
        return ms2.ToArray();
        //}
    }
}