using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.Compression
{
    internal static class CompressionUtil
    {
        public static HttpCompression CompressionNameToEnum(string name)
        {
            if (KnownHeaders.ContentEncodingGzip.Equals(name))
                return HttpCompression.Gzip;

            if (KnownHeaders.ContentEncodingDeflate.Equals(name))
                return HttpCompression.Deflate;

            if (KnownHeaders.ContentEncodingBrotli.Equals(name))
                return HttpCompression.Brotli;

            return HttpCompression.Unsupported;
        }
    }
}
