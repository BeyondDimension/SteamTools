using System;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    /// <summary>
    ///     Wraps the data sent/received event argument.
    /// </summary>
    public class DataEventArgs : EventArgs
    {
        public DataEventArgs(byte[] buffer, int offset, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Count = count;
        }

        /// <summary>
        ///     The buffer with data.
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        ///     Offset in buffer from which valid data begins.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        ///     Length from offset in buffer with valid data.
        /// </summary>
        public int Count { get; }
    }
}
