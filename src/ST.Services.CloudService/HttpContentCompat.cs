#if !NET5_0_OR_GREATER
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class HttpContentCompat
    {
        public static Task<byte[]> ReadAsByteArrayAsync(this HttpContent @this, CancellationToken _)
        {
            return @this.ReadAsByteArrayAsync();
        }

        public static Task<Stream> ReadAsStreamAsync(this HttpContent @this, CancellationToken _)
        {
            return @this.ReadAsStreamAsync();
        }

        public static Task<string> ReadAsStringAsync(this HttpContent @this, CancellationToken _)
        {
            return @this.ReadAsStringAsync();
        }
    }
}

#endif