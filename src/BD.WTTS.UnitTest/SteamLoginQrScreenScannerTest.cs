using BD.WTTS.Helpers;
using ZXing;
using ZXing.QrCode;

namespace BD.WTTS.UnitTest;

public sealed class SteamLoginQrScreenScannerTest
{
    [TestCase(false)]
    [TestCase(true)]
    public void ReadsSteamChallengeInEitherPolarity(bool inverted)
    {
        var qr = CreateQr("https://s.team/q/1/123456789", inverted);
        var results = SteamLoginQrScreenScanner.DecodeBgr32(qr, 240, 240);
        Assert.That(results.Select(x => x.ClientId), Is.EqualTo(new[] { 123456789UL }));
    }

    [Test]
    public void ReadsMixedPolarityCodesAndDeduplicatesAcrossPasses()
    {
        const int width = 800, height = 280;
        var pixels = Enumerable.Repeat((byte)128, width * height * 4).ToArray();
        Copy(CreateQr("https://s.team/q/1/123456789", false), 10);
        Copy(CreateQr("https://s.team/q/1/987654321", true), 280);
        Copy(CreateQr("https://s.team/q/1/123456789", true), 550);
        var results = SteamLoginQrScreenScanner.DecodeBgr32(pixels, width, height);
        Assert.That(results.Select(x => x.ClientId), Is.EquivalentTo(new[] { 123456789UL, 987654321UL }));

        void Copy(byte[] qr, int x)
        {
            for (var row = 0; row < 240; row++)
                Buffer.BlockCopy(qr, row * 240 * 4, pixels, ((row + 20) * width + x) * 4, 240 * 4);
        }
    }

    [TestCase("https://s.team.evil.example/q/1/123456789")]
    [TestCase("https://s.team/q/1/123456789?redirect=evil")]
    public void DoesNotAcceptUntrustedQrContents(string text)
    {
        var results = SteamLoginQrScreenScanner.DecodeBgr32(CreateQr(text, false), 240, 240);
        Assert.That(results, Is.Empty);
    }

    static byte[] CreateQr(string text, bool inverted)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions { Width = 240, Height = 240, Margin = 4 },
        };
        var pixels = writer.Write(text).Pixels;
        if (inverted)
        {
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);
                pixels[i + 1] = (byte)(255 - pixels[i + 1]);
                pixels[i + 2] = (byte)(255 - pixels[i + 2]);
            }
        }
        return pixels;
    }
}
