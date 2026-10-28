using Net.Codecrete.QrCodeGenerator;
using SkiaSharp;

namespace BD.WTTS;

partial class QRCodeHelper
{
    /// <summary>Login codes always use dark modules on a white background for phone scanners.</summary>
    public static (QRCodeCreateResult result, Stream? stream, Exception? exception) CreateLoginCode(string text)
    {
        try
        {
            var code = QrCode.EncodeText(text, QrCode.Ecc.Medium);
            using var bitmap = code.ToBitmap(8, 4, SKColors.Black);
            using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
            return (QRCodeCreateResult.Success, new MemoryStream(data.ToArray(), writable: false), null);
        }
        catch (Exception ex) { return (QRCodeCreateResult.Exception, null, ex); }
    }
}
