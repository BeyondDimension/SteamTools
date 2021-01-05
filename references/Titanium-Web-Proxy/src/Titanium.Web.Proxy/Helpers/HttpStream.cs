using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Compression;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Shared;
using Titanium.Web.Proxy.StreamExtended.BufferPool;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Helpers
{
    internal class HttpStream : Stream, IHttpStreamWriter, IHttpStreamReader, IPeekStream
    {
        private readonly bool isNetworkStream;
        private readonly bool leaveOpen;
        private readonly byte[] streamBuffer;

        private static Encoding encoding => HttpHeader.Encoding;

        private static readonly bool networkStreamHack = true;

        private int bufferLength;

        private int bufferPos;

        private bool disposed;

        private bool closedWrite;
        private bool closedRead;

        private readonly IBufferPool bufferPool;
        private readonly CancellationToken cancellationToken;

        public event EventHandler<DataEventArgs>? DataRead;

        public event EventHandler<DataEventArgs>? DataWrite;

        private Stream baseStream { get; }

        public bool IsClosed => closedRead;

        static HttpStream()
        {
            // TODO: remove this hack when removing .NET 4.x support
            try
            {
                var method = typeof(NetworkStream).GetMethod(nameof(Stream.ReadAsync),
                    new[] { typeof(byte[]), typeof(int), typeof(int), typeof(CancellationToken) });
                if (method != null && method.DeclaringType != typeof(Stream))
                {
                    networkStreamHack = false;
                }
            }
            catch
            {
                // ignore
            }
        }

        private static readonly byte[] newLine = ProxyConstants.NewLineBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpStream"/> class.
        /// </summary>
        /// <param name="baseStream">The base stream.</param>
        /// <param name="bufferPool">Bufferpool.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="leaveOpen"><see langword="true" /> to leave the stream open after disposing the <see cref="T:CustomBufferedStream" /> object; otherwise, <see langword="false" />.</param>
        internal HttpStream(Stream baseStream, IBufferPool bufferPool, CancellationToken cancellationToken, bool leaveOpen = false)
        {
            if (baseStream is NetworkStream)
            {
                isNetworkStream = true;
            }

            this.baseStream = baseStream;
            this.leaveOpen = leaveOpen;
            streamBuffer = bufferPool.GetBuffer();
            this.bufferPool = bufferPool;
            this.cancellationToken = cancellationToken;
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if (closedWrite)
            {
                return;
            }

            try
            {
                baseStream.Flush();
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            bufferLength = 0;
            bufferPos = 0;
            return baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bufferLength == 0)
            {
                FillBuffer();
            }

            int available = Math.Min(bufferLength, count);
            if (available > 0)
            {
                Buffer.BlockCopy(streamBuffer, bufferPos, buffer, offset, available);
                bufferPos += available;
                bufferLength -= available;
            }

            return available;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        [DebuggerStepThrough]
        public override void Write(byte[] buffer, int offset, int count)
        {
            OnDataWrite(buffer, offset, count);

            if (closedWrite)
            {
                return;
            }

            try
            {
                baseStream.Write(buffer, offset, count);
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        /// <summary>
        /// Asynchronously reads the bytes from the current stream and writes them to another stream, using a specified buffer size and cancellation token.
        /// </summary>
        /// <param name="destination">The stream to which the contents of the current stream will be copied.</param>
        /// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero. The default size is 81920.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous copy operation.
        /// </returns>
        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            if (bufferLength > 0)
            {
                await destination.WriteAsync(streamBuffer, bufferPos, bufferLength, cancellationToken);

                bufferLength = 0;
            }

            await base.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous flush operation.
        /// </returns>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            if (closedWrite)
            {
                return;
            }

            try
            {
                await baseStream.FlushAsync(cancellationToken);
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream,
        /// advances the position within the stream by the number of bytes read,
        /// and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which 
        /// to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. 
        /// The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation.
        /// The value of the parameter contains the total 
        /// number of bytes read into the buffer.
        /// The result value can be less than the number of bytes
        /// requested if the number of bytes currently available is
        /// less than the requested number, or it can be 0 (zero)
        /// if the end of the stream has been reached.
        /// </returns>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (bufferLength == 0)
            {
                await FillBufferAsync(cancellationToken);
            }

            int available = Math.Min(bufferLength, count);
            if (available > 0)
            {
                Buffer.BlockCopy(streamBuffer, bufferPos, buffer, offset, available);
                bufferPos += available;
                bufferLength -= available;
            }

            return available;
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream,
        /// advances the position within the stream by the number of bytes read,
        /// and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. 
        /// The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation.
        /// The value of the parameter contains the total 
        /// number of bytes read into the buffer.
        /// The result value can be less than the number of bytes
        /// requested if the number of bytes currently available is
        /// less than the requested number, or it can be 0 (zero)
        /// if the end of the stream has been reached.
        /// </returns>
#if NETSTANDARD2_1
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
#else
        public async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
#endif
        {
            if (bufferLength == 0)
            {
                await FillBufferAsync(cancellationToken);
            }

            int available = Math.Min(bufferLength, buffer.Length);
            if (available > 0)
            {
                new Span<byte>(streamBuffer, bufferPos, available).CopyTo(buffer.Span);
                bufferPos += available;
                bufferLength -= available;
            }

            return available;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        public override int ReadByte()
        {
            if (bufferLength == 0)
            {
                FillBuffer();
            }

            if (bufferLength == 0)
            {
                return -1;
            }

            bufferLength--;
            return streamBuffer[bufferPos++];
        }

        /// <summary>
        /// Peeks a byte asynchronous.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async ValueTask<int> PeekByteAsync(int index, CancellationToken cancellationToken = default)
        {
            // When index is greater than the buffer size
            if (streamBuffer.Length <= index)
            {
                throw new Exception("Requested Peek index exceeds the buffer size. Consider increasing the buffer size.");
            }

            while (Available <= index)
            {
                // When index is greater than the buffer size
                bool fillResult = await FillBufferAsync(cancellationToken);
                if (!fillResult)
                {
                    return -1;
                }
            }

            return streamBuffer[bufferPos + index];
        }

        /// <summary>
        /// Peeks bytes asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer to copy.</param>
        /// <param name="offset">The offset where copying.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async ValueTask<int> PeekBytesAsync(byte[] buffer, int offset, int index, int count, CancellationToken cancellationToken = default)
        {
            // When index is greater than the buffer size
            if (streamBuffer.Length <= index + count)
            {
                throw new Exception("Requested Peek index and size exceeds the buffer size. Consider increasing the buffer size.");
            }

            while (Available <= index)
            {
                bool fillResult = await FillBufferAsync(cancellationToken);
                if (!fillResult)
                {
                    return 0;
                }
            }

            if (Available - index < count)
            {
                count = Available - index;
            }

            Buffer.BlockCopy(streamBuffer, index, buffer, offset, count);
            return count;
        }

        /// <summary>
        /// Peeks a byte from buffer.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Index is out of buffer size</exception>
        public byte PeekByteFromBuffer(int index)
        {
            if (bufferLength <= index)
            {
                throw new Exception("Index is out of buffer size");
            }

            return streamBuffer[bufferPos + index];
        }

        /// <summary>
        /// Reads a byte from buffer.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Buffer is empty</exception>
        public byte ReadByteFromBuffer()
        {
            if (bufferLength == 0)
            {
                throw new Exception("Buffer is empty");
            }

            bufferLength--;
            return streamBuffer[bufferPos++];
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in buffer from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"></see>.</param>
        [DebuggerStepThrough]
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            OnDataWrite(buffer, offset, count);

            if (closedWrite)
            {
                return;
            }

            try
            {
                await baseStream.WriteAsync(buffer, offset, count, cancellationToken);
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value)
        {
            if (closedWrite)
            {
                return;
            }

            var buffer = bufferPool.GetBuffer();
            try
            {
                buffer[0] = value;
                OnDataWrite(buffer, 0, 1);
                baseStream.Write(buffer, 0, 1);
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
            finally
            {
                bufferPool.ReturnBuffer(buffer);
            }
        }

        protected virtual void OnDataWrite(byte[] buffer, int offset, int count)
        {
            DataWrite?.Invoke(this, new DataEventArgs(buffer, offset, count));
        }

        protected virtual void OnDataRead(byte[] buffer, int offset, int count)
        {
            DataRead?.Invoke(this, new DataEventArgs(buffer, offset, count));
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
                closedRead = true;
                closedWrite = true;
                if (!leaveOpen)
                {
                    baseStream.Dispose();
                }

                bufferPool.ReturnBuffer(streamBuffer);
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => baseStream.CanRead;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => baseStream.CanSeek;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => baseStream.CanWrite;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        public override bool CanTimeout => baseStream.CanTimeout;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        public override long Length => baseStream.Length;

        /// <summary>
        /// Gets a value indicating whether data is available.
        /// </summary>
        public bool DataAvailable => bufferLength > 0;

        /// <summary>
        /// Gets the available data size.
        /// </summary>
        public int Available => bufferLength;

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        public override int ReadTimeout
        {
            get => baseStream.ReadTimeout;
            set => baseStream.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        public override int WriteTimeout
        {
            get => baseStream.WriteTimeout;
            set => baseStream.WriteTimeout = value;
        }

        /// <summary>
        /// Fills the buffer.
        /// </summary>
        public bool FillBuffer()
        {
            if (closedRead)
            {
                throw new Exception("Stream is already closed");
            }

            if (bufferLength > 0)
            {
                // normally we fill the buffer only when it is empty, but sometimes we need more data
                // move the remaining data to the beginning of the buffer 
                Buffer.BlockCopy(streamBuffer, bufferPos, streamBuffer, 0, bufferLength);
            }

            bufferPos = 0;

            bool result = false;
            try
            {
                int readBytes = baseStream.Read(streamBuffer, bufferLength, streamBuffer.Length - bufferLength);
                result = readBytes > 0;
                if (result)
                {
                    OnDataRead(streamBuffer, bufferLength, readBytes);
                    bufferLength += readBytes;
                }
            }
            catch
            {
                if (!isNetworkStream)
                    throw;
            }
            finally
            {
                if (!result)
                {
                    closedRead = true;
                    closedWrite = true;
                }
            }

            return result;

        }

        /// <summary>
        /// Fills the buffer asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async ValueTask<bool> FillBufferAsync(CancellationToken cancellationToken = default)
        {
            if (closedRead)
            {
                throw new Exception("Stream is already closed");
            }

            int bytesToRead = streamBuffer.Length - bufferLength;
            if (bytesToRead == 0)
            {
                return false;
            }

            if (bufferLength > 0)
            {
                // normally we fill the buffer only when it is empty, but sometimes we need more data
                // move the remaining data to the beginning of the buffer 
                Buffer.BlockCopy(streamBuffer, bufferPos, streamBuffer, 0, bufferLength);
            }

            bufferPos = 0;

            bool result = false;
            try
            {
                var readTask = baseStream.ReadAsync(streamBuffer, bufferLength, bytesToRead, cancellationToken);
                if (isNetworkStream)
                {
                    readTask = readTask.WithCancellation(cancellationToken);
                }

                int readBytes = await readTask;
                result = readBytes > 0;
                if (result)
                {
                    OnDataRead(streamBuffer, bufferLength, readBytes);
                    bufferLength += readBytes;
                }
            }
            catch
            {
                if (!isNetworkStream)
                    throw;
            }
            finally
            {
                if (!result)
                {
                    closedRead = true;
                    closedWrite = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Read a line from the byte stream
        /// </summary>
        /// <returns></returns>
        public ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            return ReadLineInternalAsync(this, bufferPool, cancellationToken);
        }

        /// <summary>
        /// Read a line from the byte stream
        /// </summary>
        /// <returns></returns>
        internal static async ValueTask<string?> ReadLineInternalAsync(ILineStream reader, IBufferPool bufferPool, CancellationToken cancellationToken = default)
        {
            byte lastChar = default;

            int bufferDataLength = 0;

            // try to use buffer from the buffer pool, usually it is enough
            var bufferPoolBuffer = bufferPool.GetBuffer();
            var buffer = bufferPoolBuffer;

            try
            {
                while (reader.DataAvailable || await reader.FillBufferAsync(cancellationToken))
                {
                    byte newChar = reader.ReadByteFromBuffer();
                    buffer[bufferDataLength] = newChar;

                    // if new line
                    if (newChar == '\n')
                    {
                        if (lastChar == '\r')
                        {
                            return encoding.GetString(buffer, 0, bufferDataLength - 1);
                        }

                        return encoding.GetString(buffer, 0, bufferDataLength);
                    }

                    bufferDataLength++;

                    // store last char for new line comparison
                    lastChar = newChar;

                    if (bufferDataLength == buffer.Length)
                    {
                        Array.Resize(ref buffer, bufferDataLength * 2);
                    }
                }
            }
            finally
            {
                bufferPool.ReturnBuffer(bufferPoolBuffer);
            }

            if (bufferDataLength == 0)
            {
                return null;
            }

            return encoding.GetString(buffer, 0, bufferDataLength);
        }

        /// <summary>
        /// Read until the last new line, ignores the result
        /// </summary>
        /// <returns></returns>
        public async Task ReadAndIgnoreAllLinesAsync(CancellationToken cancellationToken = default)
        {
            while (!string.IsNullOrEmpty(await ReadLineAsync(cancellationToken)))
            {
            }
        }

        /// <summary>        
        /// Base Stream.BeginRead will call this.Read and block thread (we don't want this, Network stream handles async)
        /// In order to really async Reading Launch this.ReadAsync as Task will fire NetworkStream.ReadAsync
        /// See Threads here :
        /// https://github.com/justcoding121/Stream-Extended/pull/43
        /// https://github.com/justcoding121/Titanium-Web-Proxy/issues/575
        /// </summary>
        /// <returns></returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!networkStreamHack)
            {
                return base.BeginRead(buffer, offset, count, callback, state);
            }

            var vAsyncResult = this.ReadAsync(buffer, offset, count, cancellationToken);
            if (isNetworkStream)
            {
                vAsyncResult = vAsyncResult.WithCancellation(cancellationToken);
            }

            vAsyncResult.ContinueWith(pAsyncResult =>
            {
                // use TaskExtended to pass State as AsyncObject
                // callback will call EndRead (otherwise, it will block)
                callback?.Invoke(new TaskResult<int>(pAsyncResult, state));
            }, cancellationToken);

            return vAsyncResult;
        }

        /// <summary>
        /// override EndRead to handle async Reading (see BeginRead comment)
        /// </summary>
        /// <returns></returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            if (!networkStreamHack)
            {
                return base.EndRead(asyncResult);
            }

            return ((TaskResult<int>)asyncResult).Result;
        }

        /// <summary>
        /// Fix the .net bug with SslStream slow WriteAsync
        /// https://github.com/justcoding121/Titanium-Web-Proxy/issues/495
        /// Stream.BeginWrite + Stream.BeginRead uses the same SemaphoreSlim(1)
        /// That's why we need to call NetworkStream.BeginWrite only (while read is waiting SemaphoreSlim)
        /// </summary>
        /// <returns></returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (!networkStreamHack)
            {
                return base.BeginWrite(buffer, offset, count, callback, state);
            }

            var vAsyncResult = this.WriteAsync(buffer, offset, count, cancellationToken);

            vAsyncResult.ContinueWith(pAsyncResult =>
            {
                callback?.Invoke(new TaskResult(pAsyncResult, state));
            }, cancellationToken);

            return vAsyncResult;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (!networkStreamHack)
            {
                base.EndWrite(asyncResult);
                return;
            }

            ((TaskResult)asyncResult).GetResult();
        }

        /// <summary>
        ///     Writes a line async
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token for this async task.</param>
        /// <returns></returns>
        public ValueTask WriteLineAsync(CancellationToken cancellationToken = default)
        {
            return WriteAsync(newLine, cancellationToken: cancellationToken);
        }

        private async ValueTask writeAsyncInternal(string value, bool addNewLine, CancellationToken cancellationToken)
        {
            if (closedWrite)
            {
                return;
            }

            int newLineChars = addNewLine ? newLine.Length : 0;
            int charCount = value.Length;
            if (charCount < bufferPool.BufferSize - newLineChars)
            {
                var buffer = bufferPool.GetBuffer();
                try
                {
                    int idx = encoding.GetBytes(value, 0, charCount, buffer, 0);
                    if (newLineChars > 0)
                    {
                        Buffer.BlockCopy(newLine, 0, buffer, idx, newLineChars);
                        idx += newLineChars;
                    }

                    await baseStream.WriteAsync(buffer, 0, idx, cancellationToken);
                }
                catch
                {
                    closedWrite = true;
                    if (!isNetworkStream)
                        throw;
                }
                finally
                {
                    bufferPool.ReturnBuffer(buffer);
                }
            }
            else
            {
                var buffer = new byte[charCount + newLineChars + 1];
                int idx = encoding.GetBytes(value, 0, charCount, buffer, 0);
                if (newLineChars > 0)
                {
                    Buffer.BlockCopy(newLine, 0, buffer, idx, newLineChars);
                    idx += newLineChars;
                }

                try
                {
                    await baseStream.WriteAsync(buffer, 0, idx, cancellationToken);
                }
                catch
                {
                    closedWrite = true;
                    if (!isNetworkStream)
                        throw;
                }
            }
        }

        public ValueTask WriteLineAsync(string value, CancellationToken cancellationToken = default)
        {
            return writeAsyncInternal(value, true, cancellationToken);
        }

        /// <summary>
        ///     Write the headers to client
        /// </summary>
        /// <param name="headerBuilder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal async Task WriteHeadersAsync(HeaderBuilder headerBuilder, CancellationToken cancellationToken = default)
        {
            var buffer = headerBuilder.GetBuffer();
            await WriteAsync(buffer.Array, buffer.Offset, buffer.Count, true, cancellationToken);
        }

        /// <summary>
        ///     Writes the data to the stream.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="flush">Should we flush after write?</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        internal async ValueTask WriteAsync(byte[] data, bool flush = false, CancellationToken cancellationToken = default)
        {
            if (closedWrite)
            {
                return;
            }

            try
            {
                await baseStream.WriteAsync(data, 0, data.Length, cancellationToken);
                if (flush)
                {
                    await baseStream.FlushAsync(cancellationToken);
                }
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        internal async Task WriteAsync(byte[] data, int offset, int count, bool flush,
            CancellationToken cancellationToken = default)
        {
            if (closedWrite)
            {
                return;
            }

            try
            {
                await baseStream.WriteAsync(data, offset, count, cancellationToken);
                if (flush)
                {
                    await baseStream.FlushAsync(cancellationToken);
                }
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }

        /// <summary>
        ///     Writes the byte array body to the stream; optionally chunked
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isChunked"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        internal ValueTask WriteBodyAsync(byte[] data, bool isChunked, CancellationToken cancellationToken)
        {
            if (isChunked)
            {
                return writeBodyChunkedAsync(data, cancellationToken);
            }

            return WriteAsync(data, cancellationToken: cancellationToken);
        }

        public async Task CopyBodyAsync(RequestResponseBase requestResponse, bool useOriginalHeaderValues, IHttpStreamWriter writer, TransformationMode transformation, Action<byte[], int, int>? onCopy, CancellationToken cancellationToken)
        {
            bool isChunked = useOriginalHeaderValues ? requestResponse.OriginalIsChunked : requestResponse.IsChunked;
            long contentLength = useOriginalHeaderValues ? requestResponse.OriginalContentLength : requestResponse.ContentLength;

            if (transformation == TransformationMode.None)
            {
                await CopyBodyAsync(writer, isChunked, contentLength, onCopy, cancellationToken);
                return;
            }

            LimitedStream limitedStream;
            Stream? decompressStream = null;

            string? contentEncoding = useOriginalHeaderValues ? requestResponse.OriginalContentEncoding : requestResponse.ContentEncoding;

            Stream s = limitedStream = new LimitedStream(this, bufferPool, isChunked, contentLength);

            if (transformation == TransformationMode.Uncompress && contentEncoding != null)
            {
                s = decompressStream = DecompressionFactory.Create(CompressionUtil.CompressionNameToEnum(contentEncoding), s);
            }

            try
            {
                var http = new HttpStream(s, bufferPool, cancellationToken, true);
                await http.CopyBodyAsync(writer, false, -1, onCopy, cancellationToken);
            }
            finally
            {
                decompressStream?.Dispose();

                await limitedStream.Finish();
                limitedStream.Dispose();
            }
        }

        /// <summary>
        ///     Copies the specified content length number of bytes to the output stream from the given inputs stream
        ///     optionally chunked
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="isChunked"></param>
        /// <param name="contentLength"></param>
        /// <param name="onCopy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task CopyBodyAsync(IHttpStreamWriter writer, bool isChunked, long contentLength,
            Action<byte[], int, int>? onCopy, CancellationToken cancellationToken)
        {
            // For chunked request we need to read data as they arrive, until we reach a chunk end symbol
            if (isChunked)
            {
                return copyBodyChunkedAsync(writer, onCopy, cancellationToken);
            }

            // http 1.0 or the stream reader limits the stream
            if (contentLength == -1)
            {
                contentLength = long.MaxValue;
            }

            // If not chunked then its easy just read the amount of bytes mentioned in content length header
            return copyBytesToStream(writer, contentLength, onCopy, cancellationToken);
        }

        /// <summary>
        ///     Copies the given input bytes to output stream chunked
        /// </summary>
        /// <param name="data"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async ValueTask writeBodyChunkedAsync(byte[] data, CancellationToken cancellationToken)
        {
            var chunkHead = Encoding.ASCII.GetBytes(data.Length.ToString("x2"));

            await WriteAsync(chunkHead, cancellationToken: cancellationToken);
            await WriteLineAsync(cancellationToken);
            await WriteAsync(data, cancellationToken: cancellationToken);
            await WriteLineAsync(cancellationToken);

            await WriteLineAsync("0", cancellationToken);
            await WriteLineAsync(cancellationToken);
        }

        /// <summary>
        ///     Copies the streams chunked
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="onCopy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task copyBodyChunkedAsync(IHttpStreamWriter writer, Action<byte[], int, int>? onCopy, CancellationToken cancellationToken)
        {
            while (true)
            {
                string? chunkHead = await ReadLineAsync(cancellationToken);
                if (chunkHead == null)
                {
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

                await writer.WriteLineAsync(chunkHead, cancellationToken);

                if (chunkSize != 0)
                {
                    await copyBytesToStream(writer, chunkSize, onCopy, cancellationToken);
                }

                await writer.WriteLineAsync(cancellationToken);

                // chunk trail
                await ReadLineAsync(cancellationToken);

                if (chunkSize == 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Copies the specified bytes to the stream from the input stream
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="count"></param>
        /// <param name="onCopy"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task copyBytesToStream(IHttpStreamWriter writer, long count, Action<byte[], int, int>? onCopy,
            CancellationToken cancellationToken)
        {
            var buffer = bufferPool.GetBuffer();

            try
            {
                long remainingBytes = count;

                while (remainingBytes > 0)
                {
                    int bytesToRead = buffer.Length;
                    if (remainingBytes < bytesToRead)
                    {
                        bytesToRead = (int)remainingBytes;
                    }

                    int bytesRead = await ReadAsync(buffer, 0, bytesToRead, cancellationToken);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    remainingBytes -= bytesRead;

                    await writer.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                    onCopy?.Invoke(buffer, 0, bytesRead);
                }
            }
            finally
            {
                bufferPool.ReturnBuffer(buffer);
            }
        }

        /// <summary>
        ///     Writes the request/response headers and body.
        /// </summary>
        /// <param name="requestResponse"></param>
        /// <param name="headerBuilder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async ValueTask WriteAsync(RequestResponseBase requestResponse, HeaderBuilder headerBuilder, CancellationToken cancellationToken = default)
        {
            var body = requestResponse.CompressBodyAndUpdateContentLength();

            headerBuilder.WriteHeaders(requestResponse.Headers);
            await WriteHeadersAsync(headerBuilder, cancellationToken);

            if (body != null)
            {
                await WriteBodyAsync(body, requestResponse.IsChunked, cancellationToken);
                requestResponse.IsBodySent = true;
            }
        }

#if NETSTANDARD2_1
        /// <summary>
        ///     Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (closedWrite)
            {
                return;
            }

            try
            {
                await baseStream.WriteAsync(buffer, cancellationToken);
            }
            catch
            {
                closedWrite = true;
                if (!isNetworkStream)
                    throw;
            }
        }
#else
        /// <summary>
        ///     Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
        {
            var buf = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.CopyTo(buf);
            try
            {
                await baseStream.WriteAsync(buf, 0, buf.Length, cancellationToken);
            }
            catch
            {
                if (!isNetworkStream)
                    throw;
            }
        }
#endif
    }
}
