using SteamTool.Core;
using SteamTool.Proxy.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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

        private readonly HostsService hostsService = SteamToolCore.Instance.Get<HostsService>();
        private readonly ProxyServer proxyServer = new ProxyServer();

        public bool IsCertificate => proxyServer.CertificateManager == null;

        public List<ProxyDomainModel> ProxyDomains { get; set; }

        public HttpProxy()
        {
            ProxyDomains = DefaultData.ProxyDomains;
        }

        public bool ProxyRunning => proxyServer.ProxyRunning;

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
            #region 测试用
            /*
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
            */
            #endregion

            foreach (var item in ProxyDomains)
            {
                if (item.IsEnbale && e.HttpClient.Request.RequestUri.AbsoluteUri.Contains(item.Domain))
                {
                    IPAddress iP = null;
                    if (!string.IsNullOrEmpty(item.ProxyIPAddres))
                    {
                        iP = IPAddress.Parse(item.ProxyIPAddres);
                    }
                    var iPs = await Dns.GetHostAddressesAsync(item.ProxyDomain);
                    if (iPs.Length > 0)
                    {
                        iP = iPs[0];
                    }
                    if (iP != null)
                    {
                        e.HttpClient.UpStreamEndPoint = new IPEndPoint(IPAddress.Parse(iP.ToString()), item.Port);
                    }
                    if (e.HttpClient.ConnectRequest?.ClientHelloInfo != null)
                    {
                        if (!string.IsNullOrEmpty(item.ServerName))
                        {
                            var sni = e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"];
                            e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"] =
                                new Titanium.Web.Proxy.StreamExtended.Models.SslExtension(sni.Value, sni.Name, item.ServerName, sni.Position);
                        }
                        else
                        {
                            e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
                        }
                    }
                }
            }
        }

        public async Task OnResponse(object sender, SessionEventArgs e)
        {
            // read response headers
            //var responseHeaders = e.HttpClient.Response.Headers;
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
            //proxyServer.CertificateManager
            //    .CreateServerCertificate($"{Assembly.GetCallingAssembly().GetName().Name} Certificate")
            //    .ContinueWith(c => proxyServer.CertificateManager.RootCertificate = c.Result);

            proxyServer.CertificateManager.RootCertificateIssuerName = $"{Assembly.GetCallingAssembly().GetName().Name}";
            proxyServer.CertificateManager.RootCertificateName = $"{Assembly.GetCallingAssembly().GetName().Name} Certificate";
            proxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;

            // 可选地设置证书引擎
            // 在Mono之下，只有BouncyCastle将得到支持
            //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;

            proxyServer.CertificateManager.SaveFakeCertificates = true;
            proxyServer.CertificateManager.EnsureRootCertificate();
            proxyServer.CertificateManager.TrustRootCertificate(true);

            return proxyServer.CertificateManager.RootCertificate != null;
        }

        public bool DeleteCertificate()
        {
            if (ProxyRunning || proxyServer.CertificateManager.RootCertificate == null)
                return false;
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.MaxAllowed);
                if (store.Certificates.Contains(proxyServer.CertificateManager.RootCertificate))
                    store.Remove(proxyServer.CertificateManager.RootCertificate);
            }
            proxyServer.CertificateManager.ClearRootCertificate();
            proxyServer.CertificateManager.RemoveTrustedRootCertificate(true);
            proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin(true);
            proxyServer.CertificateManager.CertificateStorage.Clear();
            return true;
        }

        public void StartProxy()
        {
            if (proxyServer.CertificateManager.RootCertificate == null)
                SetupCertificate();

            #region 写入Hosts
            var hosts = new List<(string, string)>();
            foreach (var proxyDomain in ProxyDomains)
            {
                if (proxyDomain.IsEnbale)
                {
                    foreach (var host in proxyDomain.Hosts)
                    {
                        hosts.Add((IPAddress.Loopback.ToString(), host));
                    }
                }
            }
            hostsService.UpdateHosts(hosts);
            #endregion

            #region 启动代理
            proxyServer.BeforeRequest += OnRequest;
            //proxyServer.BeforeResponse += OnResponse;
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
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
                //GenericCertificateName= "steamcommunity-a.akamaihd.net"
            };
            proxyServer.AddEndPoint(transparentEndPoint);
            proxyServer.Start();
            //proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
            //proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
            #endregion

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
            hostsService.RemoveHostsByTag();
        }

        public void Dispose()
        {
            if (proxyServer.ProxyRunning)
                proxyServer.Stop();
            proxyServer.Dispose();
        }

    }
}
