using System.Diagnostics;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    partial class PlatformHttpMessageHandlerBuilder
    {
        [Conditional("DEBUG")]
        static void AddFiddlerRootCertificate(NSUrlSessionHandler handler)
        {
#if DEBUG
            //handler.AddTrustedCerts(FiddlerRootCertificateStream); // 添加https抓包调试用的证书，仅测试环境下
#endif
        }

        static HttpMessageHandler CreateHttpMessageHandler()
        {
            // https://docs.microsoft.com/zh-cn/xamarin/cross-platform/macios/http-stack
            var handler = new NSUrlSessionHandler();
            AddFiddlerRootCertificate(handler);
            return handler;
        }
    }
}