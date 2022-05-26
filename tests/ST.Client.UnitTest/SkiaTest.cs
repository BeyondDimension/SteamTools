using NUnit.Framework;
using SkiaSharp;
using R = System.Application.Properties.Resources;

namespace System.Application;

[TestFixture]
public class SkiaTest
{
    //[Test]
    //public void CreateImage_Jacky_Fideles6() => LoadImage(R.Jacky_Fideles6);

    [Test]
    public void CreateImage_Jacky_1245620_hero() => LoadImage(R._1245620_hero);

    //[Test]
    //public void CreateImage_Jacky_490390_header() => LoadImage(R._490390_header);

    static void LoadImage(byte[] bytes) => LoadImage(new MemoryStream(bytes));

    static void LoadImage(Stream stream)
    {
        using var imgStream = new SKManagedStream(stream);
        using var codec = SKCodec.Create(imgStream, out SKCodecResult result);
        TestContext.WriteLine(result);

        /*
         * System.ArgumentNullException : Value cannot be null. (Parameter 'buffer')
         * 堆栈跟踪: 
         * SKManagedStream.OnReadManagedStream(IntPtr buffer, IntPtr size)
         * SKManagedStream.OnRead(IntPtr buffer, IntPtr size)
         * SKAbstractManagedStream.ReadInternal(IntPtr s, Void* context, Void* buffer, IntPtr size)
         * SkiaApi.sk_codec_new_from_stream(IntPtr stream, SKCodecResult* result)
         * SKCodec.Create(SKStream stream, SKCodecResult& result)
         * https://github.com/mono/SkiaSharp/issues/1551
         * https://github.com/mono/SkiaSharp/issues/2076
         * https://github.com/mono/SkiaSharp/issues/1621
         */
    }
}
