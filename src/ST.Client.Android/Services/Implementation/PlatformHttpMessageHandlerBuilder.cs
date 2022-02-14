using System.Diagnostics;
using System.Net.Http;
using Xamarin.Android.Net;

namespace System.Application.Services.Implementation
{
    partial class PlatformHttpMessageHandlerBuilder
    {
        [Conditional("DEBUG")]
        static void AddFiddlerRootCertificate(AndroidClientHandler handler)
        {
#if DEBUG
            //handler.AddTrustedCert(FiddlerRootCertificateStream); // 添加https抓包调试用的证书，仅测试环境下
#endif
        }

        public static AndroidClientHandler CreateAndroidClientHandler()
        {
            // https://docs.microsoft.com/zh-cn/xamarin/android/app-fundamentals/http-stack?context=xamarin%2Fcross-platform&tabs=macos
            var handler = new AndroidClientHandler();
            AddFiddlerRootCertificate(handler);
            GeneralHttpClientFactory.SetProxyToHandler(GeneralHttpClientFactory.DefaultProxy, handler);
            return handler;
        }

        static HttpMessageHandler CreateHttpMessageHandler() => CreateAndroidClientHandler();
    }
}