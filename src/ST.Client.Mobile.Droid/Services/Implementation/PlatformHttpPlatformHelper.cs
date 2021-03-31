using Android.Webkit;
using System.IO;
using System.Net.Http;
using AndroidApplication = Android.App.Application;

namespace System.Application.Services.Implementation
{
    internal sealed class PlatformHttpPlatformHelper : HttpPlatformHelper
    {
        static readonly Lazy<string?> mUserAgent = new(() =>
        {
            var userAgent = WebSettings.GetDefaultUserAgent(AndroidApplication.Context);
            return userAgent;
        });

        public override string UserAgent => mUserAgent.Value ?? base.UserAgent;

        public override (string filePath, string mime)? TryHandleUploadFile(Stream fileStream)
        {
            //switch (uploadFileType)
            //{
            //    case UploadFileType.Image:
            //        var bitmap = BitmapFactory.DecodeStream(imageFileStream);
            //        throw new NotImplementedException(bitmap?.ToString());
            //    default:
            //        throw new ArgumentOutOfRangeException(nameof(uploadFileType), uploadFileType, null);
            //}
            return base.TryHandleUploadFile(fileStream);
        }
    }
}