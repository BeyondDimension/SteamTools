//using System.Application.Services.Implementation;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
//using System.Application.Services.CloudService;

//namespace System.Application
//{
//    public static class WebProxyHelper
//    {
//        /// <summary>
//        /// 设置全局 HTTP 代理。
//        /// </summary>
//        public static IWebProxy? Proxy
//        {
//            set
//            {
//                GeneralHttpClientFactory.DefaultProxy = value;
//                var handlers = new List<HttpMessageHandler>();
//                handlers.AddRange(ArchiSteamFarmServiceImpl.GetAllHandlers());
//                Add(CloudServiceClientBase.HttpClientHandler);
//                GeneralHttpClientFactory.SetProxyToHandler(value, handlers);

//                void Add(HttpMessageHandler? item)
//                {
//                    if (item == null) return;
//                    handlers.Add(item);
//                }
//            }
//        }
//    }
//}
