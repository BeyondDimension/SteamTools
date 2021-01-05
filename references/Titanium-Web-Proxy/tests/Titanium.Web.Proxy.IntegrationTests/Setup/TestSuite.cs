using System.Net.Http;
using Titanium.Web.Proxy.IntegrationTests.Helpers;
using Titanium.Web.Proxy.IntegrationTests.Setup;

namespace Titanium.Web.Proxy.IntegrationTests
{
    public class TestSuite
    {
        private TestServer server;

        public TestSuite()
        {
            var dummyProxy = new ProxyServer();
            var serverCertificate = dummyProxy.CertificateManager.CreateServerCertificate("localhost").Result;
            server = new TestServer(serverCertificate);
        }

        public TestServer GetServer()
        {
            return server;
        }

        public ProxyServer GetProxy(ProxyServer upStreamProxy = null)
        {
            if (upStreamProxy != null)
            {
                return new TestProxyServer(false, upStreamProxy).ProxyServer;
            }

            return new TestProxyServer(false).ProxyServer;
        }

        public ProxyServer GetReverseProxy(ProxyServer upStreamProxy = null)
        {
            if (upStreamProxy != null)
            {
                return new TestProxyServer(true, upStreamProxy).ProxyServer;
            }

            return new TestProxyServer(true).ProxyServer;
        }

        public HttpClient GetClient(ProxyServer proxyServer, bool enableBasicProxyAuthorization = false)
        {
            return TestHelper.GetHttpClient(proxyServer.ProxyEndPoints[0].Port, enableBasicProxyAuthorization);
        }

        public HttpClient GetReverseProxyClient()
        {
            return TestHelper.GetHttpClient();
        }
    }
}
