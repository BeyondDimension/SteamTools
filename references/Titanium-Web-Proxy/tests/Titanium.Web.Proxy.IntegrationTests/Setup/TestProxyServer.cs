using System;
using System.Net;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace Titanium.Web.Proxy.IntegrationTests.Setup
{
    public class TestProxyServer : IDisposable
    {
        public ProxyServer ProxyServer { get; }

        public int ListeningPort => ProxyServer.ProxyEndPoints[0].Port;
        
        public CertificateManager CertificateManager => ProxyServer.CertificateManager;

        public TestProxyServer(bool isReverseProxy, ProxyServer upStreamProxy = null)
        {
            ProxyServer = new ProxyServer();

            var explicitEndPoint = isReverseProxy ?
                (ProxyEndPoint)new TransparentProxyEndPoint(IPAddress.Any, 0, true) :
                new ExplicitProxyEndPoint(IPAddress.Any, 0, true);

            ProxyServer.AddEndPoint(explicitEndPoint);

            if (upStreamProxy != null)
            {
                ProxyServer.UpStreamHttpProxy = new ExternalProxy("localhost", upStreamProxy.ProxyEndPoints[0].Port);
                ProxyServer.UpStreamHttpsProxy = new ExternalProxy("localhost", upStreamProxy.ProxyEndPoints[0].Port);
            }

            ProxyServer.Start();
        }

        public void Dispose()
        {
            ProxyServer.Stop();
            ProxyServer.Dispose();
        }
    }
}
