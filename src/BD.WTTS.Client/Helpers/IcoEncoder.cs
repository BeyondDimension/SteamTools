using SkiaSharp;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// Windows ICO 格式编码器
/// </summary>
public static class IcoEncoder
{
    public static void Encode(Stream stream, IReadOnlyList<SKBitmap> bitmaps)
    {
        // 写入文件头
        stream.WriteByte(0);
        stream.WriteByte(0);
        stream.WriteByte(1); // 1 为图标，2 为光标
        stream.WriteByte(0);

        // 写入长度，2 位
        stream.Write(BitConverter.GetBytes(Convert.ToInt16(bitmaps.Count)));

        Class253 class253_1;
        List<Class253> class253List = new();
        List<MemoryStream> memoryStreamList = new();
        for (int index = 0; index < bitmaps.Count; ++index)
        {
            var image = bitmaps[index];
            MemoryStream memoryStream1 = new();
            image.Encode(memoryStream1, SKEncodedImageFormat.Png, 100);
            int num1 = (int)memoryStream1.Length;
            memoryStreamList.Add(memoryStream1);
            class253_1 = new(image.Width, image.Height, 0, image.BytesPerPixel);
            class253_1.method_14(num1);
            Class253 class253_2 = class253_1;
            class253List.Add(class253_2);
        }
        Dictionary<Class253, MemoryStream> dictionary_0 = new();
        for (int index = 0; index < memoryStreamList.Count; ++index)
            dictionary_0.Add(class253List[index], memoryStreamList[index]);

        smethod_6(dictionary_0);

        smethod_21(stream, dictionary_0);
    }

    // it's just works

#pragma warning disable IDE1006 // 命名样式

    static void smethod_6(Dictionary<Class253, MemoryStream> dictionary_0)
    {
        var class253List = dictionary_0.Keys.ToArray();
        Class253 class253_1 = class253List[0];
        class253_1.method_16(6 + (16 * dictionary_0.Count));
        int int_0 = class253_1.method_15() + class253_1.method_13();
        for (int index = 1; index < class253List.Length; ++index)
        {
            Class253 class253_2 = class253List[index];
            class253_2.method_16(int_0);
            int_0 += class253_2.method_13();
        }
    }

    sealed class Class253
    {
        byte[] byte_0 = null!;

        public Class253(int int_0, int int_1, int int_2, int int_3, int int_4, int int_5)
        {
            List<byte> byteList = new List<byte>()
            {
                (byte)int_0,
                (byte)int_1,
                int_3 <= 8 ? (byte)int_2 : (byte)0,
                0,
            };
            byteList.AddRange(BitConverter.GetBytes((short)0));
            byteList.AddRange(BitConverter.GetBytes((short)int_3));
            byteList.AddRange(BitConverter.GetBytes(int_4));
            byteList.AddRange(BitConverter.GetBytes(int_5));
            method_1(byteList.ToArray());
        }

        public Class253(int int_0, int int_1, int int_2, int int_3)
          : this(int_0, int_1, int_2, int_3, 0, 0)
        {
        }

        public byte[] method_0() => byte_0;

        private void method_1(byte[] byte_1) => byte_0 = byte_1;

        public int method_13() => BitConverter.ToInt32(method_0(), 8);

        public void method_14(int int_0) => BitConverter.GetBytes(int_0).CopyTo(method_0(), 8);

        public int method_15() => BitConverter.ToInt32(method_0(), 12);

        public void method_16(int int_0) => BitConverter.GetBytes(int_0).CopyTo(method_0(), 12);
    }

    static void smethod_21(Stream stream_0, Dictionary<Class253, MemoryStream> dictionary_0)
    {
        smethod_6(dictionary_0);
        foreach (Class253 key in dictionary_0.Keys)
            stream_0.Write(key.method_0(), 0, 16);
        foreach (MemoryStream memoryStream in dictionary_0.Values)
        {
            using (memoryStream)
                memoryStream.WriteTo(stream_0);
        }
    }
}
