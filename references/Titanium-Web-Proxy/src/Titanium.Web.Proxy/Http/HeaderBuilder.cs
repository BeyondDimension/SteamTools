using System;
using System.Buffers;
using System.IO;
using System.Text;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Shared;

namespace Titanium.Web.Proxy.Http
{
    internal class HeaderBuilder
    {
        private readonly MemoryStream stream = new MemoryStream();

        public void WriteRequestLine(string httpMethod, string httpUrl, Version version)
        {
            // "{httpMethod} {httpUrl} HTTP/{version.Major}.{version.Minor}";

            Write(httpMethod);
            Write(" ");
            Write(httpUrl);
            Write(" HTTP/");
            Write(version.Major.ToString());
            Write(".");
            Write(version.Minor.ToString());
            WriteLine();
        }

        public void WriteResponseLine(Version version, int statusCode, string statusDescription)
        {
            // "HTTP/{version.Major}.{version.Minor} {statusCode} {statusDescription}";

            Write("HTTP/");
            Write(version.Major.ToString());
            Write(".");
            Write(version.Minor.ToString());
            Write(" ");
            Write(statusCode.ToString());
            Write(" ");
            Write(statusDescription);
            WriteLine();
        }

        public void WriteHeaders(HeaderCollection headers, bool sendProxyAuthorization = true,
            string? upstreamProxyUserName = null, string? upstreamProxyPassword = null)
        {
            if (upstreamProxyUserName != null && upstreamProxyPassword != null)
            {
                WriteHeader(HttpHeader.ProxyConnectionKeepAlive);
                WriteHeader(HttpHeader.GetProxyAuthorizationHeader(upstreamProxyUserName, upstreamProxyPassword));
            }

            foreach (var header in headers)
            {
                if (sendProxyAuthorization || !KnownHeaders.ProxyAuthorization.Equals(header.Name))
                {
                    WriteHeader(header);
                }
            }

            WriteLine();
        }

        public void WriteHeader(HttpHeader header)
        {
            Write(header.Name);
            Write(": ");
            Write(header.Value);
            WriteLine();
        }

        public void WriteLine()
        {
            var data = ProxyConstants.NewLineBytes;
            stream.Write(data, 0, data.Length);
        }

        public void Write(string str)
        {
            var encoding = HttpHeader.Encoding;

#if NETSTANDARD2_1
            var buf = ArrayPool<byte>.Shared.Rent(encoding.GetMaxByteCount(str.Length));
            var span = new Span<byte>(buf);

            int bytes = encoding.GetBytes(str.AsSpan(), span);

            stream.Write(span.Slice(0, bytes));
            ArrayPool<byte>.Shared.Return(buf);
#else
            var data = encoding.GetBytes(str);
            stream.Write(data, 0, data.Length);
#endif
        }

        public ArraySegment<byte> GetBuffer()
        {
#if NET45
            return new ArraySegment<byte>(stream.ToArray());
#else
            stream.TryGetBuffer(out var buffer);
            return buffer;
#endif
        }

        public string GetString(Encoding encoding)
        {
#if NET45
            return encoding.GetString(stream.ToArray());
#else
            stream.TryGetBuffer(out var buffer);
            return encoding.GetString(buffer.Array, buffer.Offset, buffer.Count);
#endif
        }
    }
}
