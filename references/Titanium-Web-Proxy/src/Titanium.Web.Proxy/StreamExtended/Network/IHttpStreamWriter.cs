using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    /// <summary>
    ///     A concrete implementation of this interface is required when calling CopyStream.
    /// </summary>
    public interface IHttpStreamWriter
    {
        void Write(byte[] buffer, int offset, int count);

        Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);

        ValueTask WriteLineAsync(CancellationToken cancellationToken = default);

        ValueTask WriteLineAsync(string value, CancellationToken cancellationToken = default);
    }
}
