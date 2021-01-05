using System;
using System.Net;
using System.Text;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.Models
{
    /// <summary>
    ///     Http Header object used by proxy
    /// </summary>
    public class HttpHeader
    {
        /// <summary>
        /// HPACK: Header Compression for HTTP/2
        /// Section 4.1. Calculating Table Size
        /// The additional 32 octets account for an estimated overhead associated with an entry.        
        /// </summary>
        public const int HttpHeaderOverhead = 32;

#if NETSTANDARD2_1
        internal static Version VersionUnknown => HttpVersion.Unknown;
#else
        internal static Version VersionUnknown { get; } = new Version(0, 0);
#endif

        internal static Version Version10 => HttpVersion.Version10;

        internal static Version Version11 => HttpVersion.Version11;

#if NETSTANDARD2_1
        internal static Version Version20 => HttpVersion.Version20;
#else
        internal static Version Version20 { get; } = new Version(2, 0);
#endif

        internal static readonly Encoding DefaultEncoding = Encoding.GetEncoding("ISO-8859-1");

        public static Encoding Encoding => DefaultEncoding;

        internal static readonly HttpHeader ProxyConnectionKeepAlive = new HttpHeader("Proxy-Connection", "keep-alive");

        private string? nameString;

        private string? valueString;

        /// <summary>
        ///     Initialize a new instance.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        public HttpHeader(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Name cannot be null or empty");
            }

            nameString = name.Trim();
            NameData = nameString.GetByteString();

            valueString = value.Trim();
            ValueData = valueString.GetByteString();
        }

        internal HttpHeader(KnownHeader name, string value)
        {
            nameString = name.String;
            NameData = name.String8;

            valueString = value.Trim();
            ValueData = valueString.GetByteString();
        }

        internal HttpHeader(KnownHeader name, KnownHeader value)
        {
            nameString = name.String;
            NameData = name.String8;

            valueString = value.String;
            ValueData = value.String8;
        }

        internal HttpHeader(ByteString name, ByteString value)
        {
            if (name.Length == 0)
            {
                throw new Exception("Name cannot be empty");
            }

            NameData = name;
            ValueData = value;
        }

        private protected HttpHeader(ByteString name, ByteString value, bool headerEntry)
        {
            // special header entry created in inherited class with empty name
            NameData = name;
            ValueData = value;
        }

        /// <summary>
        ///     Header Name.
        /// </summary>
        public string Name => nameString ??= NameData.GetString();

        internal ByteString NameData { get; }

        /// <summary>
        ///     Header Value.
        /// </summary>
        public string Value => valueString ??= ValueData.GetString();

        internal ByteString ValueData { get; private set; }

        public int Size => Name.Length + Value.Length + HttpHeaderOverhead;

        internal static int SizeOf(ByteString name, ByteString value)
        {
            return name.Length + value.Length + HttpHeaderOverhead;
        }

        internal void SetValue(string value)
        {
            valueString = value;
            ValueData = value.GetByteString();
        }

        internal void SetValue(KnownHeader value)
        {
            valueString = value.String;
            ValueData = value.String8;
        }

        /// <summary>
        ///     Returns header as a valid header string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name}: {Value}";
        }

        internal static HttpHeader GetProxyAuthorizationHeader(string? userName, string? password)
        {
            var result = new HttpHeader(KnownHeaders.ProxyAuthorization,
                "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
            return result;
        }
    }
}
