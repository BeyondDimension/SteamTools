using System;
using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    public interface IPeekStream
    {
        /// <summary>
        /// Peeks a byte from buffer.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Index is out of buffer size</exception>
        byte PeekByteFromBuffer(int index);

        /// <summary>
        /// Peeks a byte asynchronous.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<int> PeekByteAsync(int index, CancellationToken cancellationToken = default);

        /// <summary>
        /// Peeks bytes asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer to copy.</param>
        /// <param name="offset">The offset where copying.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        ValueTask<int> PeekBytesAsync(byte[] buffer, int offset, int index, int count, CancellationToken cancellationToken = default);
    }
}