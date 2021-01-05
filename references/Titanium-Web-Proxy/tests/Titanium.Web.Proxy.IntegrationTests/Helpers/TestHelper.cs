using System;
using System.Net;
using System.Net.Http;

namespace Titanium.Web.Proxy.IntegrationTests.Helpers
{
    public static class TestHelper
    {
        public static HttpClient GetHttpClient(int localProxyPort,
            bool enableBasicProxyAuthorization = false)
        {
            var proxy = new TestProxy($"http://localhost:{localProxyPort}", enableBasicProxyAuthorization);

            var handler = new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = true
            };

            return new HttpClient(handler);
        }

        public static HttpClient GetHttpClient()
        {
            return new HttpClient(new HttpClientHandler());
        }

        public class TestProxy : IWebProxy
        {
            public Uri ProxyUri { get; set; }
            public ICredentials Credentials { get; set; }

            public TestProxy(string proxyUri, bool enableAuthorization)
                : this(new Uri(proxyUri))
            {
                if (enableAuthorization)
                {
                    Credentials = new NetworkCredential("test", "Test56");
                }
            }

            private TestProxy(Uri proxyUri)
            {
                this.ProxyUri = proxyUri;
            }

            public Uri GetProxy(Uri destination)
            {
                return this.ProxyUri;
            }

            public bool IsBypassed(Uri host)
            {
                return false;
            }

        }
    }
}
