using System;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Extensions
{
    internal static class UriExtensions
    {
        public static string GetOriginalPathAndQuery(this Uri uri)
        {
            string leftPart = uri.GetLeftPart(UriPartial.Authority);
            if (uri.OriginalString.StartsWith(leftPart))
                return uri.OriginalString.Substring(leftPart.Length);

            return uri.IsWellFormedOriginalString() ? uri.PathAndQuery : uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
        }

        public static ByteString GetScheme(ByteString str)
        {
            if (str.Length < 3)
            {
                return ByteString.Empty;
            }

            // regex: "^[a-z]*://"
            int i;

            for (i = 0; i < str.Length - 3; i++)
            {
                byte ch = str[i];
                if (ch == ':')
                {
                    break;
                }

                if (ch < 'A' || ch > 'z' || (ch > 'Z' && ch < 'a')) // ASCII letter
                {
                    return ByteString.Empty;
                }
            }

            if (str[i++] != ':')
            {
                return ByteString.Empty;
            }

            if (str[i++] != '/')
            {
                return ByteString.Empty;
            }

            if (str[i] != '/')
            {
                return ByteString.Empty;
            }

            return new ByteString(str.Data.Slice(0, i - 2));
        }
    }
}
