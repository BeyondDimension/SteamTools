using System.Buffers;

namespace Titanium.Web.Proxy.StreamExtended.BufferPool
{

    /// <summary>
    ///     A concrete IBufferPool implementation using a thread-safe stack.
    ///     Works well when all consumers ask for buffers with the same size.
    ///     If your application would use variable size buffers consider implementing IBufferPool using System.Buffers library from Microsoft.
    /// </summary>
    internal class DefaultBufferPool : IBufferPool
    {
        /// <summary>
        ///     Buffer size in bytes used throughout this proxy.
        ///     Default value is 8192 bytes.
        /// </summary>
        public int BufferSize { get; set; } = 8192;

        /// <summary>
        /// Gets a buffer with a default size.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            return ArrayPool<byte>.Shared.Rent(BufferSize);
        }

        /// <summary>
        /// Gets a buffer.
        /// </summary>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns></returns>
        public byte[] GetBuffer(int bufferSize)
        {
            return ArrayPool<byte>.Shared.Rent(bufferSize);
        }

        /// <summary>
        /// Returns the buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        public void ReturnBuffer(byte[] buffer)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public void Dispose()
        {
        }
    }
}
