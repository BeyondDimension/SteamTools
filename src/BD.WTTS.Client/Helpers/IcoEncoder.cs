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
        try
        {
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
        finally
        {
            foreach (var item in memoryStreamList)
            {
                try
                {
                    item.Dispose();
                }
                catch
                {
                }
            }
        }
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
        internal readonly byte[] Byte_0 = null!;

        public Class253(int int_0, int int_1, int int_2, int int_3, int int_4, int int_5)
        {
            #region List

            //List<byte> byteList = new List<byte>()
            //{
            //    (byte)int_0,
            //    (byte)int_1,
            //    int_3 <= 8 ? (byte)int_2 : (byte)0,
            //    0,
            //};
            //byteList.AddRange(BitConverter.GetBytes((short)0));
            //byteList.AddRange(BitConverter.GetBytes((short)int_3));
            //byteList.AddRange(BitConverter.GetBytes(int_4));
            //byteList.AddRange(BitConverter.GetBytes(int_5));
            //byte_0 = byteList.ToArray();

            #endregion

            #region Array

            var byte_1 = new byte[4 + 2 + 2 + 4 + 4];
            byte_1[0] = (byte)int_0;
            byte_1[1] = (byte)int_1;
            byte_1[2] = int_3 <= 8 ? (byte)int_2 : (byte)0;

            var int_3_ = BitConverter.GetBytes((short)int_3);
            byte_1[6] = int_3_[0];
            byte_1[7] = int_3_[1];

            int_3_ = BitConverter.GetBytes(int_4);
            byte_1[8] = int_3_[0];
            byte_1[9] = int_3_[1];
            byte_1[10] = int_3_[2];
            byte_1[11] = int_3_[3];

            int_3_ = BitConverter.GetBytes(int_5);
            byte_1[12] = int_3_[0];
            byte_1[13] = int_3_[1];
            byte_1[14] = int_3_[2];
            byte_1[15] = int_3_[3];

            Byte_0 = byte_1;

            #endregion
        }

        public Class253(int int_0, int int_1, int int_2, int int_3)
          : this(int_0, int_1, int_2, int_3, 0, 0)
        {
        }

        public int method_13() => BitConverter.ToInt32(Byte_0, 8);

        public void method_14(int int_0) => BitConverter.GetBytes(int_0).CopyTo(Byte_0, 8);

        public int method_15() => BitConverter.ToInt32(Byte_0, 12);

        public void method_16(int int_0) => BitConverter.GetBytes(int_0).CopyTo(Byte_0, 12);
    }

    static void smethod_21(Stream stream_0, Dictionary<Class253, MemoryStream> dictionary_0)
    {
        smethod_6(dictionary_0);
        foreach (Class253 key in dictionary_0.Keys)
            stream_0.Write(key.Byte_0, 0, 16);
        foreach (MemoryStream memoryStream in dictionary_0.Values)
        {
            using (memoryStream)
                memoryStream.WriteTo(stream_0);
        }
    }
}
