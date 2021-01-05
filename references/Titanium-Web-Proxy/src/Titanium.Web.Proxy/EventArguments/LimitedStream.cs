using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.StreamExtended.BufferPool;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.EventArguments
{
    internal class LimitedStream : Stream
    {
        private readonly IBufferPool bufferPool;
        private readonly IHttpStreamReader baseReader;
        private readonly bool isChunked;
        private long bytesRemaining;

        private bool readChunkTrail;

        internal LimitedStream(IHttpStreamReader baseStream, IBufferPool bufferPool, bool isChunked,
            long contentLength)
        {
            this.baseReader = baseStream;
            this.bufferPool = bufferPool;
            this.isChunked = isChunked;
            bytesRemaining = isChunked
                ? 0
                : contentLength == -1
                    ? long.MaxValue
                    : contentLength;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        private void getNextChunk()
        {
            if (readChunkTrail)
            {
                // read the chunk trail of the previous chunk
                string? s = baseReader.ReadLineAsync().Result;
                if (s == null)
                {
                    bytesRemaining = -1;
                    return;
                }
            }

            readChunkTrail = true;

            string? chunkHead = baseReader.ReadLineAsync().Result;
            if (chunkHead == null)
            {
                bytesRemaining = -1;
                return;
            }

            int idx = chunkHead.IndexOf(";", StringComparison.Ordinal);
            if (idx >= 0)
            {
                chunkHead = chunkHead.Substring(0, idx);
            }

            if (!int.TryParse(chunkHead, NumberStyles.HexNumber, null, out int chunkSize))
            {
                throw new ProxyHttpException($"Invalid chunk length: '{chunkHead}'", null, null);
            }

            bytesRemaining = chunkSize;

            if (chunkSize == 0)
            {
                bytesRemaining = -1;

                // chunk trail
                var task = baseReader.ReadLineAsync();
                if (!task.IsCompleted)
                    task.AsTask().Wait();
            }
        }

        private async Task getNextChunkAsync()
        {
            if (readChunkTrail)
            {
                // read the chunk trail of the previous chunk
                string? s = await baseReader.ReadLineAsync();
                if (s == null)
                {
                    bytesRemaining = -1;
                    return;
                }
            }

            readChunkTrail = true;

            string? chunkHead = await baseReader.ReadLineAsync();
            if (chunkHead == null)
            {
                bytesRemaining = -1;
                return;
            }

            int idx = chunkHead.IndexOf(";", StringComparison.Ordinal);
            if (idx >= 0)
            {
                chunkHead = chunkHead.Substring(0, idx);
            }

            if (!int.TryParse(chunkHead, NumberStyles.HexNumber, null, out int chunkSize))
            {
                throw new ProxyHttpException($"Invalid chunk length: '{chunkHead}'", null, null);
            }

            bytesRemaining = chunkSize;

            if (chunkSize == 0)
            {
                bytesRemaining = -1;

                // chunk trail
                await baseReader.ReadLineAsync();
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bytesRemaining == -1)
            {
                return 0;
            }

            if (bytesRemaining == 0)
            {
                if (isChunked)
                {
                    getNextChunk();
                }
                else
                {
                    bytesRemaining = -1;
                }
            }

            if (bytesRemaining == -1)
            {
                return 0;
            }

            int toRead = (int)Math.Min(count, bytesRemaining);
            int res = baseReader.Read(buffer, offset, toRead);
            bytesRemaining -= res;

            if (res == 0)
            {
                bytesRemaining = -1;
            }

            return res;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (bytesRemaining == -1)
            {
                return 0;
            }

            if (bytesRemaining == 0)
            {
                if (isChunked)
                {
                    await getNextChunkAsync();
                }
                else
                {
                    bytesRemaining = -1;
                }
            }

            if (bytesRemaining == -1)
            {
                return 0;
            }

            int toRead = (int)Math.Min(count, bytesRemaining);
            int res = await baseReader.ReadAsync(buffer, offset, toRead, cancellationToken);
            bytesRemaining -= res;

            if (res == 0)
            {
                bytesRemaining = -1;
            }

            return res;
        }

        public async Task Finish()
        {
            if (bytesRemaining != -1)
            {
                var buffer = bufferPool.GetBuffer();
                try
                {
                    int res = await ReadAsync(buffer, 0, buffer.Length);
                    if (res != 0)
                    {
                        throw new Exception("Data received after stream end");
                    }
                }
                finally
                {
                    bufferPool.ReturnBuffer(buffer);
                }
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
