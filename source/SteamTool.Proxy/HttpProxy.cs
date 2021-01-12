using SteamTool.Core;
using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using SteamTool.Model;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace SteamTool.Proxy
{
    public class HttpProxy : IDisposable
    {
        private readonly HostsService hostsService = SteamToolCore.Instance.Get<HostsService>();
        private readonly ProxyServer proxyServer = new ProxyServer();

        public bool IsCertificate => proxyServer.CertificateManager == null;

        public IReadOnlyCollection<ProxyDomainModel> ProxyDomains { get; set; }
        public List<ProxyScript> Scripts { get; set; }
        public bool IsEnableScript { get; set; }
        public bool IsOnlyWorkSteamBrowser { get; set; }
        public string CertificateName { get; }

        public HttpProxy(IReadOnlyCollection<ProxyDomainModel> proxyDomains, string certificateName)
        {
            ProxyDomains = proxyDomains;
            CertificateName = certificateName;
            //proxyServer.CertificateManager.PfxPassword = $"{CertificateName}";
            proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;
            proxyServer.CertificateManager.PfxFilePath = Path.Combine(AppContext.BaseDirectory, $@"{CertificateName}.Certificate.pfx");
            proxyServer.CertificateManager.RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
            proxyServer.CertificateManager.RootCertificateName = $"{CertificateName} Certificate";
            proxyServer.CertificateManager.CertificateEngine = Titanium.Web.Proxy.Network.CertificateEngine.DefaultWindows;
        }

        public bool ProxyRunning => proxyServer.ProxyRunning;

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
#if DEBUG
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
            Debug.WriteLine("OnRequest " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            Debug.WriteLine("OnRequest HTTP " + e.HttpClient.Request.HttpVersion);
            //Logger.Info("OnRequest" + e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
            // Dns.GetHostAddressesAsync(e.HttpClient.Request.Host).ContinueWith(s =>
            //{
            //    //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
            //    if (IPAddress.IsLoopback(s.Result.FirstOrDefault())
            //   && ProxyDomains.Count(w => w.IsEnable && w.Hosts.Contains(e.HttpClient.Request.Host)) == 0)
            //    {
            //        e.Ok($"URL : {e.HttpClient.Request.RequestUri.AbsoluteUri} \r\n not support proxy");
            //        return;
            //    }
            //});
            //Logger.Info("Steam++ OnRequest: " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            foreach (var item in ProxyDomains)
            {
                if (!item.IsEnable)
                {
                    continue;
                }
                foreach (var host in item.Domains)
                {
                    if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains(host))
                    {
                        if (e.HttpClient.Request.RequestUri.Scheme == "http")
                        {
                            e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                        }
                        IPAddress iP = null;
                        if (!string.IsNullOrEmpty(item.ProxyIPAddres))
                        {
                            iP = IPAddress.Parse(item.ProxyIPAddres);
                        }
                        else
                        {
                            var iPs = await Dns.GetHostAddressesAsync(item.ToDomain);
                            iP = iPs.FirstOrDefault();
                            //Logger.Info("Proxy IP: " + iP);
                        }
                        if (iP != null)
                        {
                            e.HttpClient.UpStreamEndPoint = new IPEndPoint(IPAddress.Parse(iP.ToString()), item.Port);
                        }
                        if (e.HttpClient.ConnectRequest?.ClientHelloInfo != null)
                        {
                            //Logger.Info("ClientHelloInfo Info: " + e.HttpClient.ConnectRequest.ClientHelloInfo);
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
                        return;
                    }
                }
            }

            //没有匹配到的结果直接返回不支持,避免出现Loopback死循环内存溢出
            e.Ok($"URL : {e.HttpClient.Request.RequestUri.AbsoluteUri} {Environment.NewLine} not support proxy");
            return;
        }
        public async Task OnResponse(object sender, SessionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
            Logger.Info("OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
            if (IsEnableScript)
            {
                if (IsOnlyWorkSteamBrowser)
                {
                    var ua = e.HttpClient.Request.Headers.GetHeaders("User-Agent");
                    if (ua.Any())
                    {
                        if (!ua.First().Value.Contains("Valve Steam"))
                        {
                            return;
                        }
                    }
                    else
                        return;
                }
                foreach (var script in Scripts)
                {
                    if (script.Enable)
                    {
                        if (e.HttpClient.Request.Method == "GET")
                        {
                            if (e.HttpClient.Response.StatusCode == 200)
                            {
                                if (e.HttpClient.Response.ContentType != null && e.HttpClient.Response.ContentType.Trim().ToLower().Contains("text/html"))
                                {
                                    foreach (var host in script.Exclude)
                                    {
                                        if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host))
                                            goto close;
                                    }
                                    foreach (var host in script.Match)
                                    {
                                        if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host))
                                        {
                                            var doc = await e.GetResponseBodyAsString();
                                            if (script.Require.Length > 0)
                                            {
                                                //var headIndex = doc.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                                                //doc = doc.Insert(headIndex, "<meta http-equiv=\"Content-Security-Policy\" content=\"default - src 'self' data: gap: https://ssl.gstatic.com 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; media-src *\">");
                                                var t = e.HttpClient.Response.Headers.GetFirstHeader("Content-Security-Policy");
                                                if (!string.IsNullOrEmpty(t.Value))
                                                {
                                                    //var tt = t.Value.Split(';');
                                                    //for (var i = 0; i < tt.Length; i++)
                                                    //{
                                                    //    if (tt[i].Contains("script-src"))
                                                    //    {
                                                    //        var cc = tt[i].Split(' ').ToList();
                                                    //        foreach (var req in script.Require)
                                                    //        {
                                                    //            var u = new Uri(req);
                                                    //            cc.Add($"{u.Scheme}://{u.Host}/");
                                                    //        }
                                                    //        tt[i] = string.Join(" ", cc);
                                                    //    }
                                                    //}
                                                    //var result = string.Join(";", tt);
                                                    e.HttpClient.Response.Headers.RemoveHeader(t);
                                                    //e.HttpClient.Response.Headers.AddHeader("Content-Security-Policy", result);
                                                }
#if DEBUG
                                                Debug.WriteLine(e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
                                                foreach (var req in script.Require)
                                                {
                                                    var headIndex = doc.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                                                    //<script type="text/javascript" src=""></script>
                                                    //var result = await httpServices.Get(req);
                                                    var temp1 = $"<script type=\"text/javascript\" src=\"{req}\"></script>\n";
                                                    doc = doc.Insert(headIndex, temp1);
                                                }
                                            }
                                            var index = doc.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                                            var temp = $"<script type=\"text/javascript\">{script.@Content}</script>";
                                            doc = doc.Insert(index, temp);
                                            e.SetResponseBodyString(doc);
                                        }
                                    }
                                }
                            }
                        }
                    }
                close:;
                }
            }
        }

        //// 允许重写默认的证书验证逻辑
        //public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        //{
        //    // 根据证书错误，设置IsValid为真/假
        //    //if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
        //    e.IsValid = true;
        //    return Task.CompletedTask;
        //}

        //// 允许在相互身份验证期间重写默认客户端证书选择逻辑
        //public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        //{
        //    // set e.clientCertificate to override
        //    return Task.CompletedTask;
        //}

        public bool SetupCertificate()
        {
            // 此代理使用的本地信任根证书 
            //proxyServer.CertificateManager.TrustRootCertificate(true);
            //proxyServer.CertificateManager
            //    .CreateServerCertificate($"{Assembly.GetCallingAssembly().GetName().Name} Certificate")
            //    .ContinueWith(c => proxyServer.CertificateManager.RootCertificate = c.Result);

            // 可选地设置证书引擎
            // 在Mono之下，只有BouncyCastle将得到支持
            //proxyServer.CertificateManager.CertificateEngine = Network.CertificateEngine.BouncyCastle;
            //proxyServer.CertificateManager.SaveFakeCertificates = true;
            var result = proxyServer.CertificateManager.CreateRootCertificate(true);
            //if (result)
            //{
            proxyServer.CertificateManager.EnsureRootCertificate();
            proxyServer.CertificateManager.RootCertificate.SaveCerCertificateFile(Path.Combine(AppContext.BaseDirectory, $@"{CertificateName}.Certificate.cer"));
            //}
            return IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate);
        }

        public bool DeleteCertificate()
        {
            if (ProxyRunning)
                return false;
            try
            {
                //using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser))
                //{
                //    store.Open(OpenFlags.MaxAllowed);
                //    var test = store.Certificates.Find(X509FindType.FindByIssuerName, CertificateName, true);
                //    foreach (var item in test)
                //    {
                //        store.Remove(item);
                //    }
                //}
                //proxyServer.CertificateManager.ClearRootCertificate();
                proxyServer.CertificateManager.RemoveTrustedRootCertificate();
                //proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin();
                //proxyServer.CertificateManager.CertificateStorage.Clear();
            }
            catch (Exception ex)
            {
                if (ex is CryptographicException)
                {
                    //取消删除证书
                }
                else
                {
                    throw;
                }
            }
            return true;
        }

        public static bool PortInUse(int port)
        {
            bool inUse = false;

            var ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }

            return inUse;
        }

        public bool StartProxy(bool IsProxyGOG = false)
        {
            if (!IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate))
            {
                var isOk = SetupCertificate();
                if (!isOk)
                {
                    return false;
                }
            }
            if (PortInUse(443))
            {
                return false;
            }
            if (IsProxyGOG) { WirtePemCertificateToGoGSteamPlugins(); }

            #region 写入Hosts
            var hosts = new List<(string, string)>();
            foreach (var proxyDomain in ProxyDomains)
            {
                if (proxyDomain.IsEnable)
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
            proxyServer.BeforeResponse += OnResponse;
            //proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            //var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 443, true)
            //{
            //    // 在所有https请求上使用自颁发的通用证书
            //    // 通过不为每个启用http的域创建证书来优化性能
            //    // 当代理客户端不需要证书信任时非常有用
            //    //GenericCertificate = new X509Certificate2(Path.Combine(AppContext.BaseDirectory, "genericcert.pfx"), "password")
            //};
            //当接收到连接请求时触发
            //explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelRequest;
            //explicit endpoint 是客户端知道代理存在的地方
            //因此，客户端以代理友好的方式发送请求
            //proxyServer.AddEndPoint(explicitEndPoint);
            // transparentEndPoint 对于反向代理很有用(客户端不知道代理的存在)
            // transparentEndPoint 通常需要一个网络路由器端口来转发HTTP(S)包或DNS
            // 发送数据到此endpoint 
            var transparentEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 443, true)
            {
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
            };
            proxyServer.AddEndPoint(transparentEndPoint);
            var transparentEndPoint80 = new TransparentProxyEndPoint(IPAddress.Any, 80, true)
            {
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
            };
            proxyServer.AddEndPoint(transparentEndPoint80);
            proxyServer.ExceptionFunc = ((Exception exception) =>
            {
                Logger.Error(exception);
            });

            try
            {
                proxyServer.Start();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
            //proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
            //proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = "localhost", Port = 8888 };
            #endregion
#if DEBUG
            foreach (var endPoint in proxyServer.ProxyEndPoints)
                Debug.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
#endif
            return true;

        }

        public void StopProxy()
        {
            if (proxyServer.ProxyRunning)
            {
                proxyServer.BeforeRequest -= OnRequest;
                proxyServer.BeforeResponse -= OnResponse;
                //proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
                //proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
                proxyServer.Stop();
                hostsService.RemoveHostsByTag();
            }
        }

        public bool WirtePemCertificateToGoGSteamPlugins()
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var gogPlugins = Path.Combine(local, "GOG.com", "Galaxy", "plugins", "installed");
            if (Directory.Exists(gogPlugins))
            {
                foreach (var dir in Directory.GetDirectories(gogPlugins))
                {
                    if (dir.Contains("steam"))
                    {
                        var pem = proxyServer.CertificateManager.RootCertificate.GetPublicPemCertificateString();
                        var certifi = Path.Combine(local, dir, "certifi", "cacert.pem");
                        if (File.Exists(certifi))
                        {
                            var file = File.ReadAllText(certifi);
                            var s = file.Substring(Const.HOST_TAG, Const.HOST_TAG, true);
                            if (string.IsNullOrEmpty(s))
                            {
                                File.AppendAllText(certifi, Environment.NewLine + pem);
                            }
                            else if (s.Trim() != pem.Trim())
                            {
                                var index = file.IndexOf(Const.HOST_TAG);
                                File.WriteAllText(certifi, file.Remove(index, s.Length) + pem);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsCertificateInstalled(X509Certificate2 certificate2)
        {
            if (certificate2 == null)
                return false;
            using var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.MaxAllowed);
            return store.Certificates.Contains(certificate2);
        }

        public void Dispose()
        {
            if (proxyServer.ProxyRunning)
            {
                proxyServer.Stop();
                hostsService.RemoveHostsByTag();
            }
            proxyServer.Dispose();
        }

    }
}
