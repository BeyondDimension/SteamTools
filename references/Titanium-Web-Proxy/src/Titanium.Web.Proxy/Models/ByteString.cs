using System;
using System.Text;
using Titanium.Web.Proxy.Extensions;

namespace Titanium.Web.Proxy.Models
{
    internal struct ByteString : IEquatable<ByteString>
    {
        public static ByteString Empty = new ByteString(ReadOnlyMemory<byte>.Empty);

        public ReadOnlyMemory<byte> Data { get; }

        public ReadOnlySpan<byte> Span => Data.Span;

        public int Length => Data.Length;

        public ByteString(ReadOnlyMemory<byte> data)
        {
            Data = data;
        }

        public override bool Equals(object? obj)
        {
            return obj is ByteString other && Equals(other);
        }

        public bool Equals(ByteString other)
        {
            return Data.Span.SequenceEqual(other.Data.Span);
        }

        public int IndexOf(byte value)
        {
            return Span.IndexOf(value);
        }

        public ByteString Slice(int start)
        {
            return Data.Slice(start);
        }

        public ByteString Slice(int start, int length)
        {
            return Data.Slice(start, length);
        }

        public override int GetHashCode()
        {
            return Data.GetHashCode();
        }

        public override string ToString()
        {
            return this.GetString();
        }

        public static explicit operator ByteString(string str) => new ByteString(Encoding.ASCII.GetBytes(str));

        public static implicit operator ByteString(byte[] data) => new ByteString(data);

        public static implicit operator ByteString(ReadOnlyMemory<byte> data) => new ByteString(data);

        public byte this[int i]
        {
            get => Span[i];
        }
    }
}
