using System.IO;

namespace System.Net.Http
{
    /// <inheritdoc cref="IHttpPlatformHelper"/>
    public class HttpPlatformHelper : IHttpPlatformHelper
    {
        const string DefaultUserAgent = "Mozilla/5.0 (Windows Phone 10.0; Android 4.2.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Mobile Safari/537.36 Edge/14.14263";

        public virtual string UserAgent => DefaultUserAgent;

        public (string filePath, string mime)? TryHandleUploadFile(Stream fileStream) => null;
    }
}