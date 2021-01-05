using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Helpers.WinHttp;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.UnitTests
{
    [TestClass]
    public class SystemProxyTest
    {
        [TestMethod]
        public void CompareProxyAddressReturnedByWebProxyAndWinHttpProxyResolver()
        {
            var proxyManager = new SystemProxyManager();

            try
            {
                compareUrls();

                proxyManager.SetProxy("127.0.0.1", 8000, ProxyProtocolType.Http);
                compareUrls();

                proxyManager.SetProxy("127.0.0.1", 8000, ProxyProtocolType.Https);
                compareUrls();

                proxyManager.SetProxy("127.0.0.1", 8000, ProxyProtocolType.AllHttp);
                compareUrls();

                // for this test you need to add a proxy.pac file to a local webserver
                //function FindProxyForURL(url, host)
                //{
                //    if (shExpMatch(host, "google.com"))
                //    {
                //        return "PROXY 127.0.0.1:8888";
                //    }

                //    return "DIRECT";
                //}

                //proxyManager.SetAutoProxyUrl("http://localhost/proxy.pac");
                //CompareUrls();

                proxyManager.SetProxyOverride("<-loopback>");
                compareUrls();

                proxyManager.SetProxyOverride("<local>");
                compareUrls();

                proxyManager.SetProxyOverride("yahoo.com");
                compareUrls();

                proxyManager.SetProxyOverride("*.local");
                compareUrls();

                proxyManager.SetProxyOverride("http://*.local");
                compareUrls();

                proxyManager.SetProxyOverride("<-loopback>;*.local");
                compareUrls();

                proxyManager.SetProxyOverride("<-loopback>;*.local;<local>");
                compareUrls();
            }
            finally
            {
                proxyManager.RestoreOriginalSettings();
            }
        }

        private void compareUrls()
        {
            var webProxy = WebRequest.GetSystemWebProxy();

            var resolver = new WinHttpWebProxyFinder();
            resolver.LoadFromIE();

            CompareProxy(webProxy, resolver, "http://127.0.0.1");
            CompareProxy(webProxy, resolver, "https://127.0.0.1");
            CompareProxy(webProxy, resolver, "http://localhost");
            CompareProxy(webProxy, resolver, "https://localhost");

            string hostName = null;
            try
            {
                hostName = Dns.GetHostName();
            }
            catch
            {
            }

            if (hostName != null)
            {
                CompareProxy(webProxy, resolver, "http://" + hostName);
                CompareProxy(webProxy, resolver, "https://" + hostName);
            }

            CompareProxy(webProxy, resolver, "http://google.com");
            CompareProxy(webProxy, resolver, "https://google.com");
            CompareProxy(webProxy, resolver, "http://bing.com");
            CompareProxy(webProxy, resolver, "https://bing.com");
            CompareProxy(webProxy, resolver, "http://yahoo.com");
            CompareProxy(webProxy, resolver, "https://yahoo.com");
            CompareProxy(webProxy, resolver, "http://test.local");
            CompareProxy(webProxy, resolver, "https://test.local");
        }

        private void CompareProxy(IWebProxy webProxy, WinHttpWebProxyFinder resolver, string url)
        {
            var uri = new Uri(url);

            var expectedProxyUri = webProxy.GetProxy(uri);

            var proxy = resolver.GetProxy(uri);

            if (expectedProxyUri == uri)
            {
                // no proxy
                Assert.AreEqual(proxy, null);
                return;
            }

            Assert.AreEqual(expectedProxyUri.ToString(), $"http://{proxy.HostName}:{proxy.Port}/");
        }
    }
}
