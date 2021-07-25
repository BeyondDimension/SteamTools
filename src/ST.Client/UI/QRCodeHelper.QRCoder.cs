using QRCoder;
using QRCoder.Exceptions;
using System.IO;
using static QRCoder.QRCodeGenerator;

namespace System.Application.UI
{
    public static partial class QRCodeHelper
    {
        const int DefaultPixelsPerModule = 6;

        static Stream GetStream(this QRCodeData qrCodeData, int pixelsPerModule = DefaultPixelsPerModule)
        {
            using BitmapByteQRCode qrCode = new(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(pixelsPerModule);
            MemoryStream ms = new(qrCodeBytes);
            return ms;
        }

        public static (QRCodeCreateResult result, Stream? stream, Exception? exception) Create(byte[] bytes, ECCLevel level = ECCLevel.L, int pixelsPerModule = DefaultPixelsPerModule)
        {
            try
            {
                var qrCodeData = GenerateQrCode(bytes, level);
                var ms = qrCodeData.GetStream(pixelsPerModule);
                return (QRCodeCreateResult.Success, ms, null);
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

        public static (QRCodeCreateResult result, Stream? stream, Exception? exception) Create(string str, ECCLevel level = ECCLevel.L, int pixelsPerModule = DefaultPixelsPerModule)
        {
            try
            {
                var qrCodeData = GenerateQrCode(str, level);
                var ms = qrCodeData.GetStream(pixelsPerModule);
                return (QRCodeCreateResult.Success, ms, null);
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