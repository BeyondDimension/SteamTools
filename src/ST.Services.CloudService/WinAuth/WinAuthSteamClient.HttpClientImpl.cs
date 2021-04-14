//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Net.Http;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web;
//using System.Net;

//namespace WinAuth
//{
//    partial class WinAuthSteamClient
//    {
//        protected async Task<string> RequestAsync(string url, HttpMethod method, NameValueCollection data, IReadOnlyDictionary<string, string> headers)
//        {
//            var query = data == null ? string.Empty : string.Join("&", Array.ConvertAll(data.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(data[key]))));
//            if (method == HttpMethod.Get)
//            {
//                url += (url.IndexOf("?") == -1 ? "?" : "&") + query;
//            }

//            var request = new HttpRequestMessage(method, url);
//            request.Headers.Accept.ParseAdd("text/javascript, text/html, application/xml, text/xml, */*");
//            request.Headers.UserAgent.ParseAdd(USERAGENT);
//            request.Headers.Referrer = new Uri(COMMUNITY_BASE);
//            if (headers != null)
//            {
//                foreach (var item in headers)
//                {
//                    request.Headers.Add(item.Key, item.Value);
//                }
//            }

//            var cookies = Session.Cookies.GetCookies(request.RequestUri)
//#if !NETCOREAPP3_0_OR_GREATER
//                .Cast<Cookie>()
//#endif
//                ;
//            if (cookies.Any_Nullable())
//            {
//                request.Headers.Add("Cookie", string.Join(';', cookies.Select(x => $"{x.Name}={x.Value}")));
//            }

//            // 在移动平台上 WebRequest 应该是托管实现，性能不如原生实现，所以要迁移到 HttpClient
//            // 但是 GZip，Cookies 都在 HttpMessageHandler 层实现，例如 WinHttp。不同的 Handler 支持的功能也不一致
//            // HttpClient Cookie支持差，有需要转义的字符作为键或值就可能引发问题，所以不能迁移
//        }
//    }
//}
