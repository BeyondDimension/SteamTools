using QRCoder;
using System.IO;
using static QRCoder.QRCodeGenerator;

namespace System.Application.UI
{
    public static class QRCodeHelper
    {
        public static Stream Create(byte[] bytes, ECCLevel level = ECCLevel.L, int pixelsPerModule = 4)
        {
            var qrCodeData = GenerateQrCode(bytes, level);
            using BitmapByteQRCode qrCode = new(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
            MemoryStream ms = new(qrCodeBytes);
            return ms;
        }
    }
}