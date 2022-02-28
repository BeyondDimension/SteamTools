using Net.Codecrete.QrCodeGenerator;
using SkiaSharp;
using System.Application.Models;
using System.IO;

namespace System.Application.UI
{
    public static partial class QRCodeHelper
    {
        public static (QRCodeCreateResult result, Stream? stream, Exception? exception) Create(byte[] bytes, QrCode.Ecc? level = null)
        {
            level ??= QrCode.Ecc.Low;
            try
            {
                var qrCode = QrCode.EncodeBinary(bytes, level); // Make the QR code symbol
                var foreground = IApplication.Instance.ActualTheme switch
                {
                    AppTheme.Dark => SKColors.White,
                    _ => SKColors.Black,
                };
                int scale = 10, border = 4;
                using SKBitmap bitmap = qrCode.ToBitmap(scale, border, foreground);
                SKData data = bitmap.Encode(SKEncodedImageFormat.Png, 90);

                return (QRCodeCreateResult.Success, data.AsStream(true), null);
            }
            catch (DataTooLongException e)
            {
                return (QRCodeCreateResult.DataTooLong, null, e);
            }
            catch (Exception e)
            {
                return (QRCodeCreateResult.Exception, null, e);
            }
        }
    }
}