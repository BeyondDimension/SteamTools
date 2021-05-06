using CefNet.Net;
using System.Net;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class CookieExtensions
    {
        public static CefNetCookie GetCefNetCookie(this Cookie cookie)
        {
            var cookie2 = new CefNetCookie(cookie.Name, cookie.Value)
            {
                Domain = cookie.Domain,
                Path = cookie.Path,
                Expired = cookie.Expired,
                Expires = cookie.Expires == default ? null : cookie.Expires,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
            };
            return cookie2;
        }
    }
}