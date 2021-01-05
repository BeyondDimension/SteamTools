using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Titanium.Web.Proxy.StreamExtended.BufferPool;

namespace Titanium.Web.Proxy
{
    public class WebSocketDecoder
    {
        private byte[] buffer;

        private long bufferLength;

        internal WebSocketDecoder(IBufferPool bufferPool)
        {
            buffer = new byte[bufferPool.BufferSize];
        }

        public IEnumerable<WebSocketFrame> Decode(byte[] data, int offset, int count)
        {
            var buffer = data.AsMemory(offset, count);

            bool copied = false;
            if (bufferLength > 0)
            {
                // already have remaining data
                buffer = copyToBuffer(buffer);
                copied = true;
            }

            while (true)
            {
                var data1 = buffer.Span;
                if (!isDataEnough(data1))
                {
                    break;
                }

                var opCode = (WebsocketOpCode)(data1[0] & 0xf);
                bool isFinal = (data1[0] & 0x80) != 0;
                byte b = data1[1];
                long size = b & 0x7f;

                // todo: size > int.Max??

                bool masked = (b & 0x80) != 0;

                int idx = 2;
                if (size > 125)
                {
                    if (size == 126)
                    {
                        size = (data1[2] << 8) + data1[3];
                        idx = 4;
                    }
                    else
                    {
                        size = ((long)data1[2] << 56) + ((long)data1[3] << 48) + ((long)data1[4] << 40) + ((long)data1[5] << 32) +
                               ((long)data1[6] << 24) + (data1[7] << 16) + (data1[8] << 8) + data1[9];
                        idx = 10;
                    }
                }

                if (data1.Length < idx + size)
                {
                    break;
                }

                if (masked)
                {
                    //mask = (uint)(((long)data1[idx++] << 24) + (data1[idx++] << 16) + (data1[idx++] << 8) + data1[idx++]);
                    //mask = (uint)(data1[idx++] + (data1[idx++] << 8) + (data1[idx++] << 16) + ((long)data1[idx++] << 24));
                    var uData = MemoryMarshal.Cast<byte, uint>(data1.Slice(idx, (int)size + 4));
                    idx += 4;

                    uint mask = uData[0];
                    long size1 = size;
                    if (size > 4)
                    {
                        uData = uData.Slice(1);
                        for (int i = 0; i < uData.Length; i++)
                        {
                            uData[i] = uData[i] ^ mask;
                        }

                        size1 -= uData.Length * 4;
                    }

                    if (size1 > 0)
                    {
                        int pos = (int)(idx + size - size1);
                        data1[pos] ^= (byte)mask;

                        if (size1 > 1)
                        {
                            data1[pos + 1] ^= (byte)(mask >> 8);
                        }

                        if (size1 > 2)
                        {
                            data1[pos + 2] ^= (byte)(mask >> 16);
                        }
                    }
                }

                var frameData = buffer.Slice(idx, (int)size);
                var frame = new WebSocketFrame { IsFinal = isFinal, Data = frameData, OpCode = opCode };
                yield return frame;

                buffer = buffer.Slice((int)(idx + size));
            }

            if (!copied && buffer.Length > 0)
            {
                copyToBuffer(buffer);
            }

            if (copied)
            {
                if (buffer.Length == 0)
                {
                    bufferLength = 0;
                }
                else
                {
                    buffer.CopyTo(this.buffer);
                    bufferLength = buffer.Length;
                }
            }
        }

        private Memory<byte> copyToBuffer(ReadOnlyMemory<byte> data)
        {
            long requiredLength = bufferLength + data.Length;
            if (requiredLength > buffer.Length)
            {
                Array.Resize(ref buffer, (int)Math.Min(requiredLength, buffer.Length * 2));
            }

            data.CopyTo(buffer.AsMemory((int)bufferLength));
            bufferLength += data.Length;
            return buffer.AsMemory(0, (int)bufferLength);
        }

        private static bool isDataEnough(ReadOnlySpan<byte> data)
        {
            int length = data.Length;
            if (length < 2)
                return false;

            byte size = data[1];
            if ((size & 0x80) != 0) // masked
                length -= 4;

            size &= 0x7f;

            if (size == 126)
            {
                if (length < 2)
                {
                    return false;
                }
            }
            else if (size == 127)
            {
                if (length < 10)
                {
                    return false;
                }
            }

            return length >= size;
        }
    }
}
