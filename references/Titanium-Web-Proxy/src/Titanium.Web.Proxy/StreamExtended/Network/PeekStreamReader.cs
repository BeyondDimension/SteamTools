using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    internal class PeekStreamReader
    {
        private readonly IPeekStream baseStream;

        public int Position { get; private set; }

        public PeekStreamReader(IPeekStream baseStream, int startPosition = 0)
        {
            this.baseStream = baseStream;
            Position = startPosition;
        }

        public async ValueTask<bool> EnsureBufferLength(int length, CancellationToken cancellationToken)
        {
            var val = await baseStream.PeekByteAsync(Position + length - 1, cancellationToken);
            return val != -1;
        }

        public byte ReadByte()
        {
            return baseStream.PeekByteFromBuffer(Position++);
        }

        public int ReadInt16()
        {
            int i1 = ReadByte();
            int i2 = ReadByte();
            return (i1 << 8) + i2;
        }

        public int ReadInt24()
        {
            int i1 = ReadByte();
            int i2 = ReadByte();
            int i3 = ReadByte();
            return (i1 << 16) + (i2 << 8) + i3;
        }

        public byte[] ReadBytes(int length)
        {
            var buffer = new byte[length];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = ReadByte();
            }

            return buffer;
        }
    }
}
