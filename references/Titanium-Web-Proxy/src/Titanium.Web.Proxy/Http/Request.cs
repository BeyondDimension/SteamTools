using System;
using System.ComponentModel;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Http
{
    /// <summary>
    ///     Http(s) request object
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Request : RequestResponseBase
    {
        /// <summary>
        ///     Request Method.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        ///     Is Https?
        /// </summary>
        public bool IsHttps { get; internal set; }

        private ByteString requestUriString8;

        internal ByteString RequestUriString8
        {
            get => requestUriString8;
            set
            {
                requestUriString8 = value;
                var scheme = UriExtensions.GetScheme(value);
                if (scheme.Length > 0)
                {
                    IsHttps = scheme.Equals(ProxyServer.UriSchemeHttps8);
                }
            }
        }

        internal ByteString Authority { get; set; }

        /// <summary>
        ///     Request HTTP Uri.
        /// </summary>
        public Uri RequestUri
        {
            get
            {
                string url = Url;
                try
                {
                    return new Uri(url);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Invalid URI: '{url}'", ex);
                }
            }
            set
            {
                Url = value.OriginalString;
            }
        }

        /// <summary>
        ///     The request url as it is in the HTTP header
        /// </summary>
        public string Url
        {
            get
            {
                string url = RequestUriString8.GetString();
                if (UriExtensions.GetScheme(RequestUriString8).Length == 0)
                {
                    string? hostAndPath = Host ?? Authority.GetString();

                    if (url.StartsWith("/"))
                    {
                        hostAndPath += url;
                    }
                    else
                    {
                        //throw new Exception($"Invalid URL: '{url}'");
                    }

                    url = string.Concat(IsHttps ? "https://" : "http://", hostAndPath);
                }

                return url;
            }
            set
            {
                RequestUriString = value;
            }
        }

        /// <summary>
        ///     The request uri as it is in the HTTP header
        /// </summary>
        public string RequestUriString
        {
            get => RequestUriString8.GetString();
            set
            {
                RequestUriString8 = (ByteString)value;

                var scheme = UriExtensions.GetScheme(RequestUriString8);
                if (scheme.Length > 0 && Host != null)
                {
                    var uri = new Uri(value);
                    Host = uri.Authority;
                    Authority = ByteString.Empty;
                }
            }
        }

        /// <summary>
        ///     Has request body?
        /// </summary>
        public override bool HasBody
        {
            get
            {
                long contentLength = ContentLength;

                // If content length is set to 0 the request has no body
                if (contentLength == 0)
                {
                    return false;
                }

                // Has body only if request is chunked or content length >0
                if (IsChunked || contentLength > 0)
                {
                    return true;
                }

                // has body if POST and when version is http/1.0
                if (Method == "POST" && HttpVersion == HttpHeader.Version10)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Http hostname header value if exists.
        ///     Note: Changing this does NOT change host in RequestUri.
        ///     Users can set new RequestUri separately.
        /// </summary>
        public string? Host
        {
            get => Headers.GetHeaderValueOrNull(KnownHeaders.Host);
            set => Headers.SetOrAddHeaderValue(KnownHeaders.Host, value);
        }

        /// <summary>
        ///     Does this request has a 100-continue header?
        /// </summary>
        public bool ExpectContinue
        {
            get
            {
                string? headerValue = Headers.GetHeaderValueOrNull(KnownHeaders.Expect);
                return KnownHeaders.Expect100Continue.Equals(headerValue);
            }
        }

        /// <summary>
        ///     Does this request contain multipart/form-data?
        /// </summary>
        public bool IsMultipartFormData => ContentType?.StartsWith("multipart/form-data") == true;

        /// <summary>
        ///     Cancels the client HTTP request without sending to server.
        ///     This should be set when API user responds with custom response.
        /// </summary>
        internal bool CancelRequest { get; set; }

        /// <summary>
        ///     Does this request has an upgrade to websocket header?
        /// </summary>
        public bool UpgradeToWebSocket
        {
            get
            {
                string? headerValue = Headers.GetHeaderValueOrNull(KnownHeaders.Upgrade);

                if (headerValue == null)
                {
                    return false;
                }

                return headerValue.EqualsIgnoreCase(KnownHeaders.UpgradeWebsocket.String);
            }
        }

        /// <summary>
        ///     Did server respond positively for 100 continue request?
        /// </summary>
        public bool ExpectationSucceeded { get; internal set; }

        /// <summary>
        ///     Did server respond negatively for 100 continue request?
        /// </summary>
        public bool ExpectationFailed { get; internal set; }

        /// <summary>
        ///     Gets the header text.
        /// </summary>
        public override string HeaderText
        {
            get
            {
                var headerBuilder = new HeaderBuilder();
                headerBuilder.WriteRequestLine(Method, RequestUriString, HttpVersion);
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

            // GET request don't have a request body to read
            if (!HasBody)
            {
                throw new BodyNotFoundException("Request don't have a body. " +
                                                "Please verify that this request is a Http POST/PUT/PATCH and request " +
                                                "content length is greater than zero before accessing the body.");
            }

            if (!IsBodyRead)
            {
                if (Locked)
                {
                    throw new Exception("You cannot get the request body after request is made to server.");
                }

                if (throwWhenNotReadYet)
                {
                    throw new Exception("Request body is not read yet. " +
                                        "Use SessionEventArgs.GetRequestBody() or SessionEventArgs.GetRequestBodyAsString() " +
                                        "method to read the request body.");
                }
            }
        }

        internal static void ParseRequestLine(string httpCmd, out string method, out ByteString requestUri,
            out Version version)
        {
            int firstSpace = httpCmd.IndexOf(' ');
            if (firstSpace == -1)
            {
                // does not contain at least 2 parts
                throw new Exception("Invalid HTTP request line: " + httpCmd);
            }

            int lastSpace = httpCmd.LastIndexOf(' ');

            // break up the line into three components (method, remote URL & Http Version)

            // Find the request Verb
            method = httpCmd.Substring(0, firstSpace);
            if (!isAllUpper(method))
            {
                method = method.ToUpper();
            }

            version = HttpHeader.Version11;

            if (firstSpace == lastSpace)
            {
                requestUri = (ByteString)httpCmd.AsSpan(firstSpace + 1).ToString();
            }
            else
            {
                requestUri = (ByteString)httpCmd.AsSpan(firstSpace + 1, lastSpace - firstSpace - 1).ToString();

                // parse the HTTP version
                var httpVersion = httpCmd.AsSpan(lastSpace + 1);

                if (httpVersion.EqualsIgnoreCase("HTTP/1.0".AsSpan(0)))
                {
                    version = HttpHeader.Version10;
                }
            }
        }

        private static bool isAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch < 'A' || ch > 'Z')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
