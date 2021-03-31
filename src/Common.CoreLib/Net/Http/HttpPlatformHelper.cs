using System.IO;
using System.IO.FileFormats;

namespace System.Net.Http
{
    /// <inheritdoc cref="IHttpPlatformHelper"/>
    public class HttpPlatformHelper : IHttpPlatformHelper
    {
        const string DefaultUserAgent = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/14.14263";

        public virtual string UserAgent => DefaultUserAgent;

        public virtual string AcceptImages => "image/png, image/*; q=0.8";

        public virtual string AcceptLanguage => "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7";

        static readonly Lazy<ImageFormat[]> mSupportedImageFormats = new(() => new ImageFormat[]
        {
            ImageFormat.JPEG,
            ImageFormat.PNG,
            ImageFormat.GIF,
        });

        public virtual ImageFormat[] SupportedImageFormats => mSupportedImageFormats.Value;

        public virtual (string filePath, string mime)? TryHandleUploadFile(Stream fileStream) => null;
    }
}