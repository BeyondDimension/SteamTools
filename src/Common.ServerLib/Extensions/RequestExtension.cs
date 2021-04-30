using Microsoft.AspNetCore.Http;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class RequestExtension
    {
        static bool AcceptWebP_(this HttpRequest httpRequest)
        {
            if (httpRequest.Headers.TryGetValue("Accept", out var accept))
            {
                return accept.ToArray().Any(x => x.IndexOf("image/webp", StringComparison.OrdinalIgnoreCase) != -1);
            }
            return false;
        }

        const string KEY_ACCEPT_WEBP = "KEY_ACCEPT_WEBP";

        public static bool AcceptWebP(this HttpRequest httpRequest)
        {
            if (httpRequest.HttpContext.Items[KEY_ACCEPT_WEBP] is bool b) return b;
            var result = httpRequest.AcceptWebP_();
            httpRequest.HttpContext.Items[KEY_ACCEPT_WEBP] = result;
            return result;
        }

        public static string ImgSrcToWebP(this HttpRequest httpRequest, string src, bool? acceptWebP = null)
        {
            acceptWebP ??= httpRequest.AcceptWebP();
            if (acceptWebP.Value)
            {
                return src.Substring(0, src.LastIndexOf(".", StringComparison.Ordinal)) + ".webp";
            }
            return src;
        }
    }
}