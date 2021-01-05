using System;
using System.ComponentModel;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http
{
    /// <summary>
    ///     Http(s) response object
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Response : RequestResponseBase
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        public Response(byte[] body)
        {
            Body = body;
        }

        /// <summary>
        ///     Response Status Code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///     Response Status description.
        /// </summary>
        public string StatusDescription { get; set; } = string.Empty;

        internal string RequestMethod { get; set; }

        /// <summary>
        ///     Has response body?
        /// </summary>
        public override bool HasBody
        {
            get
            {
                if (RequestMethod == "HEAD")
                {
                    return false;
                }

                long contentLength = ContentLength;

                // If content length is set to 0 the response has no body
                if (contentLength == 0)
                {
                    return false;
                }

                // Has body only if response is chunked or content length >0
                // If none are true then check if connection:close header exist, if so write response until server or client terminates the connection
                if (IsChunked || contentLength > 0 || !KeepAlive)
                {
                    return true;
                }

                // has response if connection:keep-alive header exist and when version is http/1.0
                // Because in Http 1.0 server can return a response without content-length (expectation being client would read until end of stream)
                if (KeepAlive && HttpVersion == HttpHeader.Version10)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Keep the connection alive?
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                string? headerValue = Headers.GetHeaderValueOrNull(KnownHeaders.Connection);

                if (headerValue != null)
                {
                    if (headerValue.EqualsIgnoreCase(KnownHeaders.ConnectionClose.String))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        ///     Gets the header text.
        /// </summary>
        public override string HeaderText
        {
            get
            {
                var headerBuilder = new HeaderBuilder();
                headerBuilder.WriteResponseLine(HttpVersion, StatusCode, StatusDescription);
                headerBuilder.WriteHeaders(Headers);
                return headerBuilder.GetString(HttpHeader.Encoding);
            }
        }

        internal override void EnsureBodyAvailable(bool throwWhenNotReadYet = true)
        {
            if (BodyInternal != null)
            {
                return;
            }

            if (!HasBody)
            {
                throw new BodyNotFoundException("Response don't have a body.");
            }

            if (!IsBodyRead && throwWhenNotReadYet)
            {
                throw new Exception("Response body is not read yet. " +
                                    "Use SessionEventArgs.GetResponseBody() or SessionEventArgs.GetResponseBodyAsString() " +
                                    "method to read the response body.");
            }
        }

        internal static void ParseResponseLine(string httpStatus, out Version version, out int statusCode, out string statusDescription)
        {
            int firstSpace = httpStatus.IndexOf(' ');
            if (firstSpace == -1)
            {
                throw new Exception("Invalid HTTP status line: " + httpStatus);
            }

            var httpVersion = httpStatus.AsSpan(0, firstSpace);

            version = HttpHeader.Version11;
            if (httpVersion.EqualsIgnoreCase("HTTP/1.0".AsSpan()))
            {
                version = HttpHeader.Version10;
            }

            int secondSpace = httpStatus.IndexOf(' ', firstSpace + 1);
            if (secondSpace != -1)
            {
#if NETSTANDARD2_1
                statusCode = int.Parse(httpStatus.AsSpan(firstSpace + 1, secondSpace - firstSpace - 1));
#else
                statusCode = int.Parse(httpStatus.AsSpan(firstSpace + 1, secondSpace - firstSpace - 1).ToString());
#endif
                statusDescription = httpStatus.AsSpan(secondSpace + 1).ToString();
            }
            else
            {
#if NETSTANDARD2_1
                statusCode = int.Parse(httpStatus.AsSpan(firstSpace + 1));
#else
                statusCode = int.Parse(httpStatus.AsSpan(firstSpace + 1).ToString());
#endif
                statusDescription = string.Empty;
            }
        }
    }
}
