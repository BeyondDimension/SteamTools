using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Shared;
using Titanium.Web.Proxy.StreamExtended.BufferPool;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Helpers
{
    internal static class HttpHelper
    {
        struct SemicolonSplitEnumerator
        {
            private readonly ReadOnlyMemory<char> data;

            private ReadOnlyMemory<char> current;

            private int idx;

            public SemicolonSplitEnumerator(string str) : this(str.AsMemory())
            {
            }

            public SemicolonSplitEnumerator(ReadOnlyMemory<char> data)
            {
                this.data = data;
                current = null;
                idx = 0;
            }

            public SemicolonSplitEnumerator GetEnumerator() { return this; }

            public bool MoveNext()
            {
                if (this.idx > data.Length) return false;

                int idx = data.Span.Slice(this.idx).IndexOf(';');
                if (idx == -1)
                {
                    idx = data.Length;
                }
                else
                {
                    idx += this.idx;
                }

                current = data.Slice(this.idx, idx - this.idx);
                this.idx = idx + 1;
                return true;
            }


            public ReadOnlyMemory<char> Current => current;
        }

        /// <summary>
        ///     Gets the character encoding of request/response from content-type header
        /// </summary>
        /// <param name="contentType"></param>  
        /// <returns></returns>
        internal static Encoding GetEncodingFromContentType(string? contentType)
        {
            try
            {
                // return default if not specified
                if (contentType == null)
                {
                    return HttpHeader.DefaultEncoding;
                }

                // extract the encoding by finding the charset
                foreach (var p in new SemicolonSplitEnumerator(contentType))
                {
                    var parameter = p.Span;
                    int equalsIndex = parameter.IndexOf('=');
                    if (equalsIndex != -1 && KnownHeaders.ContentTypeCharset.Equals(parameter.Slice(0, equalsIndex).TrimStart()))
                    {
                        var value = parameter.Slice(equalsIndex + 1);
                        if (value.EqualsIgnoreCase("x-user-defined".AsSpan()))
                        {
                            continue;
                        }

                        if (value.Length > 2 && value[0] == '"' && value[value.Length - 1] == '"')
                        {
                            value = value.Slice(1, value.Length - 2);
                        }

                        return Encoding.GetEncoding(value.ToString());
                    }
                }
            }
            catch
            {
                // parsing errors
                // ignored
            }

            // return default if not specified
            return HttpHeader.DefaultEncoding;
        }

        internal static ReadOnlyMemory<char> GetBoundaryFromContentType(string? contentType)
        {
            if (contentType != null)
            {
                // extract the boundary
                foreach (var parameter in new SemicolonSplitEnumerator(contentType))
                {
                    int equalsIndex = parameter.Span.IndexOf('=');
                    if (equalsIndex != -1 && KnownHeaders.ContentTypeBoundary.Equals(parameter.Span.Slice(0, equalsIndex).TrimStart()))
                    {
                        var value = parameter.Slice(equalsIndex + 1);
                        if (value.Length > 2 && value.Span[0] == '"' && value.Span[value.Length - 1] == '"')
                        {
                            value = value.Slice(1, value.Length - 2);
                        }

                        return value;
                    }
                }
            }

            // return null if not specified
            return null;
        }

        /// <summary>
        ///     Tries to get root domain from a given hostname
        ///     Adapted from below answer
        ///     https://stackoverflow.com/questions/16473838/get-domain-name-of-a-url-in-c-sharp-net
        /// </summary>
        /// <param name="hostname"></param>
        /// <returns></returns>
        internal static string GetWildCardDomainName(string hostname)
        {
            // only for subdomains we need wild card
            // example www.google.com or gstatic.google.com
            // but NOT for google.com or IP address

            if (IPAddress.TryParse(hostname, out _))
            {
                return hostname;
            }

            if (hostname.Split(ProxyConstants.DotSplit).Length > 2)
            {
                int idx = hostname.IndexOf(ProxyConstants.DotSplit);

                // issue #352
                if (hostname.Substring(0, idx).Contains("-"))
                {
                    return hostname;
                }

                string rootDomain = hostname.Substring(idx + 1);
                return "*." + rootDomain;
            }

            // return as it is
            return hostname;
        }

        /// <summary>
        ///     Gets the HTTP method from the stream.
        /// </summary>
        public static async ValueTask<KnownMethod> GetMethod(IPeekStream httpReader, IBufferPool bufferPool, CancellationToken cancellationToken = default)
        {
            const int lengthToCheck = 20;
            if (bufferPool.BufferSize < lengthToCheck)
            {
                throw new Exception($"Buffer is too small. Minimum size is {lengthToCheck} bytes");
            }

            byte[] buffer = bufferPool.GetBuffer(bufferPool.BufferSize);
            try
            {
                int i = 0;
                while (i < lengthToCheck)
                {
                    int peeked = await httpReader.PeekBytesAsync(buffer, i, i, lengthToCheck - i, cancellationToken);
                    if (peeked <= 0)
                        return KnownMethod.Invalid;

                    peeked += i;

                    while (i < peeked)
                    {
                        int b = buffer[i];

                        if (b == ' ' && i > 2)
                            return getKnownMethod(buffer.AsSpan(0, i));

                        char ch = (char)b;
                        if ((ch < 'A' || ch > 'z' || (ch > 'Z' && ch < 'a')) && (ch != '-')) // ASCII letter
                            return KnownMethod.Invalid;

                        i++;
                    }
                }

                // only letters, but no space (or shorter than 3 characters)
                return KnownMethod.Invalid;
            }
            finally
            {
                bufferPool.ReturnBuffer(buffer);
            }
        }

        private static KnownMethod getKnownMethod(ReadOnlySpan<byte> method)
        {
            // the following methods are supported:
            // Connect
            // Delete
            // Get
            // Head
            // Options
            // Post
            // Put
            // Trace
            // Pri

            // method parameter should have at least 3 bytes
            byte b1 = method[0];
            byte b2 = method[1];
            byte b3 = method[2];

            switch (method.Length)
            {
                case 3:
                    // Get or Put
                    if (b1 == 'G')
                        return b2 == 'E' && b3 == 'T' ? KnownMethod.Get : KnownMethod.Unknown;

                    if (b1 == 'P')
                    {
                        if (b2 == 'U')
                            return b3 == 'T' ? KnownMethod.Put : KnownMethod.Unknown;

                        if (b2 == 'R')
                            return b3 == 'I' ? KnownMethod.Pri : KnownMethod.Unknown;
                    }

                    break;
                case 4:
                    // Head or Post
                    if (b1 == 'H')
                        return b2 == 'E' && b3 == 'A' && method[3] == 'D' ? KnownMethod.Head : KnownMethod.Unknown;

                    if (b1 == 'P')
                        return b2 == 'O' && b3 == 'S' && method[3] == 'T' ? KnownMethod.Post : KnownMethod.Unknown;

                    break;
                case 5:
                    // Trace
                    if (b1 == 'T')
                        return b2 == 'R' && b3 == 'A' && method[3] == 'C' && method[4] == 'E' ? KnownMethod.Trace : KnownMethod.Unknown;

                    break;
                case 6:
                    // Delete
                    if (b1 == 'D')
                        return b2 == 'E' && b3 == 'L' && method[3] == 'E' && method[4] == 'T' && method[5] == 'E' ? KnownMethod.Delete : KnownMethod.Unknown;

                    break;
                case 7:
                    // Connect or Options
                    if (b1 == 'C')
                        return b2 == 'O' && b3 == 'N' && method[3] == 'N' && method[4] == 'E' && method[5] == 'C' && method[6] == 'T' ? KnownMethod.Connect : KnownMethod.Unknown;

                    if (b1 == 'O')
                        return b2 == 'P' && b3 == 'T' && method[3] == 'I' && method[4] == 'O' && method[5] == 'N' && method[6] == 'S' ? KnownMethod.Options : KnownMethod.Unknown;

                    break;
            }


            return KnownMethod.Unknown;
        }
    }
}
