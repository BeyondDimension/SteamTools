using System;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.StreamExtended.BufferPool;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    /// <summary>
    ///     Copies the source stream to destination stream.
    ///     But this let users to peek and read the copying process.
    /// </summary>
    internal class CopyStream : ILineStream, IDisposable
    {
        private readonly IHttpStreamReader reader;

        private readonly IHttpStreamWriter writer;

        private readonly IBufferPool bufferPool;

        private int bufferLength;

        private readonly byte[] buffer;

        private bool disposed;

        public bool DataAvailable => reader.DataAvailable;

        public long ReadBytes { get; private set; }

        public CopyStream(IHttpStreamReader reader, IHttpStreamWriter writer, IBufferPool bufferPool)
        {
            this.reader = reader;
            this.writer = writer;
            buffer = bufferPool.GetBuffer();
            this.bufferPool = bufferPool;
        }

        public async ValueTask<bool> FillBufferAsync(CancellationToken cancellationToken = default)
        {
            await FlushAsync(cancellationToken);
            return await reader.FillBufferAsync(cancellationToken);
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            // send out the current data from from the buffer
            if (bufferLength > 0)
            {
                await writer.WriteAsync(buffer, 0, bufferLength, cancellationToken);
                bufferLength = 0;
            }
        }

        public byte ReadByteFromBuffer()
        {
            byte b = reader.ReadByteFromBuffer();
            buffer[bufferLength++] = b;
            ReadBytes++;
            return b;
        }

        public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            return HttpStream.ReadLineInternalAsync(this, bufferPool, cancellationToken);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                bufferPool.ReturnBuffer(buffer);
            }
        }
    }
}
