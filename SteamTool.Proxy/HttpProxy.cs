using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace SteamTool.Proxy
{
    public class HttpProxy : IDisposable
    {
        // singleton
        public readonly static HttpProxy Current = new HttpProxy();

        private readonly ProxyServer proxyServer = new ProxyServer();

        public bool ProxyRunning => proxyServer.ProxyRunning;

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("steampowered.com"))
            {
                var ip = Dns.GetHostAddresses("steampowered.com");
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(IPAddress.Parse(ip[0].ToString()), 443);
                if (e.HttpClient.ConnectRequest?.ClientHelloInfo != null)
                {
                    e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
                }
            }

            if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("steamcommunity.com"))
            {
                var ip = Dns.GetHostAddresses("steamcommunity-a.akamaihd.net");
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(IPAddress.Parse(ip[0].ToString()), 443);
                if (e.HttpClient.ConnectRequest?.ClientHelloInfo != null)
                {
                    e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
                }
            }
        }

        // Modify response
        public async Task OnResponse(object sender, SessionEventArgs e)
        {
            // read response headers
            var responseHeaders = e.HttpClient.Response.Headers;
            //if (!e.ProxySession.Request.Host.Equals("medeczane.sgk.gov.tr")) return;
            if (e.HttpClient.Request.Method == "GET" || e.HttpClient.Request.Method == "POST")
            {
                if (e.HttpClient.Response.StatusCode == 200)
                {
                    if (e.HttpClient.Response.ContentType != null && e.HttpClient.Response.ContentType.Trim().ToLower().Contains("text/html"))
                    {
                        byte[] bodyBytes = await e.GetResponseBody();
                        e.SetResponseBody(bodyBytes);
                        string body = await e.GetResponseBodyAsString();
                        e.SetResponseBodyString(body);
                    }
                }
            }
        }

        // 允许重写默认的证书验证逻辑
        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // 根据证书错误，设置IsValid为真/假
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;
            return Task.CompletedTask;
        }

        // 允许在相互身份验证期间重写默认客户端证书选择逻辑
        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            // set e.clientCertificate to override
            return Task.CompletedTask;
        }


        public bool SetupCertificate()
        {
            // 此代理使用的本地信任根证书 
            //proxyServer.CertificateManager.TrustRootCertificate(true);
            proxyServer.CertificateManager.RootCertificateIssuerName = $"{Assembly.GetCallingAssembly().GetName()}Certificate";
            proxyServer.CertificateManager.RootCertificateName = $"{Assembly.GetCallingAssembly().GetName()}Certificate";
            proxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;

            // 可选地设置证书引擎
            // 在Mono之下，只有BouncyCastle将得到支持
            //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;

            proxyServer.CertificateManager.EnsureRootCertificate();

            return proxyServer.CertificateManager == null;
        }

        public void DeleteCertificate()
        {
            proxyServer.CertificateManager.ClearRootCertificate();
            proxyServer.CertificateManager.RemoveTrustedRootCertificate(true);
            proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin(true);
            proxyServer.CertificateManager.CertificateStorage.Clear();
        }


        public void StartProxy()
        {
            if (proxyServer.CertificateManager == null)
                return;
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            //proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            //var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 443, true)
            //{
            //    // 在所有https请求上使用自颁发的通用证书
            //    // 通过不为每个启用http的域创建证书来优化性能
            //    // 当代理客户端不需要证书信任时非常有用
            //    //GenericCertificate = new X509Certificate2(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "genericcert.pfx"), "password")
            //};
            //当接收到连接请求时触发
            //explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelRequest;
            //explicit endpoint 是客户端知道代理存在的地方
            //因此，客户端以代理友好的方式发送请求
            //proxyServer.AddEndPoint(explicitEndPoint);


            // 透明endpoint 对于反向代理很有用(客户端不知道代理的存在)
            // 透明endpoint 通常需要一个网络路由器端口来转发HTTP(S)包或DNS
            // 发送数据到此endpoint 
            var transparentEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 443, true)
            {
                //GenericCertificateName= "steamcommunity-a.akamaihd.net"
            };
            proxyServer.AddEndPoint(transparentEndPoint);
            proxyServer.Start();
            //proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
            //proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
#if DEBUG
            foreach (var endPoint in proxyServer.ProxyEndPoints)
                Debug.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
#endif
        }

        public void StopProxy()
        {
            proxyServer.BeforeRequest -= OnRequest;
            proxyServer.BeforeResponse -= OnResponse;
            proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
            proxyServer.Stop();
        }

        public void Dispose()
        {
            if (proxyServer.ProxyRunning)
                proxyServer.Stop();
            proxyServer.Dispose();
        }
    }
}
