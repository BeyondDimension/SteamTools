using System;
using System.Application.Models;
using System.Application.Properties;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Properties;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;

namespace System.Application.Services.Implementation
{
    public class HttpProxyServiceImpl : IHttpProxyService
    {
        private readonly ProxyServer proxyServer = new();

        public bool IsCertificate => proxyServer.CertificateManager == null || proxyServer.CertificateManager.RootCertificate == null;

        public IReadOnlyCollection<AccelerateProjectDTO?>? ProxyDomains { get; set; }

        public IReadOnlyCollection<ProxyScript?>? Scripts { get; set; }

        public bool IsEnableScript { get; set; }

        public bool IsOnlyWorkSteamBrowser { get; set; }

        public string CertificateName { get; set; } = ThisAssembly.AssemblyProduct;

        public CertificateEngine CertificateEngine { get; set; }

        public int ProxyPort { get; set; } = 26501;

        public bool ProxyRunning => proxyServer.ProxyRunning;

        public HttpProxyServiceImpl()
        {
            //proxyServer.CertificateManager.PfxPassword = $"{CertificateName}";
            //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;
            proxyServer.CertificateManager.PfxFilePath = Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.pfx");
            proxyServer.CertificateManager.RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
            proxyServer.CertificateManager.RootCertificateName = $"{CertificateName} Certificate";
            // 可选地设置证书引擎
            //proxyServer.CertificateManager.SaveFakeCertificates = true;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;
            else
                proxyServer.CertificateManager.CertificateEngine = CertificateEngine.BouncyCastle;
        }

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("OnRequest " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            Debug.WriteLine("OnRequest HTTP " + e.HttpClient.Request.HttpVersion);
#endif
            if (ProxyDomains is null)
            {
                return;
            }
            foreach (var item in ProxyDomains)
            {
                if (!item.Enable)
                {
                    continue;
                }
                foreach (var host in item.DomainNames)
                {
                    if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains(host))
                    {
                        if (e.HttpClient.Request.RequestUri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                        {
                            e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                            return;
                        }
                        if (item.Redirect)
                        {
                            e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Host, item.ForwardDomainName));
                            return;
                        }
                        IPAddress ip;
                        if (!item.ForwardDomainIsNameOrIP)
                        {
                            ip = IPAddress.Parse(item.ForwardDomainIP);
                        }
                        else
                        {
                            var iPs = await Dns.GetHostAddressesAsync(item.ForwardDomainName);
                            if (!iPs.Any_Nullable())
                                return;
                            ip = iPs.First();
                            //Logger.Info("Proxy IP: " + iP);
                        }
                        if (ip != null)
                        {
                            e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
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

            await Dns.GetHostAddressesAsync(e.HttpClient.Request.Host).ContinueWith(s =>
            {
                //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
                if (IPAddress.IsLoopback(s.Result.FirstOrDefault())
              && ProxyDomains.Count(w => w.Enable && w.Hosts.Contains(e.HttpClient.Request.Host)) == 0)
                {
                    e.Ok($"URL : {e.HttpClient.Request.RequestUri.AbsoluteUri} \r\n not support proxy");
                    return;
                }
                Log.Info("Proxy", "IsLoopback OnRequest: " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            });

            ////没有匹配到的结果直接返回不支持,避免出现Loopback死循环内存溢出
            //e.Ok($"URL : {e.HttpClient.Request.RequestUri.AbsoluteUri} {Environment.NewLine}not support proxy");
            return;
        }

        public async Task OnResponse(object sender, SessionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
            Log.Info("Proxy", "OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
            if (Scripts is null)
            {
                return;
            }
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
                                            if (script.Require.Length > 0 || script.Grant.Length > 0)
                                            {
                                                var t = e.HttpClient.Response.Headers.GetFirstHeader("Content-Security-Policy");
                                                if (!string.IsNullOrEmpty(t?.Value))
                                                {
                                                    e.HttpClient.Response.Headers.RemoveHeader(t);
                                                }
#if DEBUG
                                                Debug.WriteLine(e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
                                                foreach (var req in script.Require)
                                                {
                                                    var headIndex = doc.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                                                    var temp1 = $"<script type=\"text/javascript\" src=\"{req}\"></script>\n";
                                                    if (headIndex > -1)
                                                        doc = doc.Insert(headIndex, temp1);
                                                }
                                            }
                                            var index = doc.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                                            var temp = $"<script type=\"text/javascript\">{script.@Content}</script>";
                                            if (index > -1)
                                            {
                                                doc = doc.Insert(index, temp);
                                                e.SetResponseBodyString(doc);
                                            }
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

        // 允许重写默认的证书验证逻辑
        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            // 根据证书错误，设置IsValid为真/假
            //if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
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
            var result = proxyServer.CertificateManager.CreateRootCertificate(true);
            if (!result || proxyServer.CertificateManager.RootCertificate == null)
            {
                Log.Error("Proxy", SR.CreateCertificateFaild);
                Toast.Show(SR.CreateCertificateFaild);
                return false;
            }
            proxyServer.CertificateManager.RootCertificate.SaveCerCertificateFile(Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.cer"));

            proxyServer.CertificateManager.EnsureRootCertificate();

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
                proxyServer.CertificateManager.RootCertificate = null;
                if (File.Exists(proxyServer.CertificateManager.PfxFilePath))
                    File.Delete(proxyServer.CertificateManager.PfxFilePath);
                //proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin();
                //proxyServer.CertificateManager.CertificateStorage.Clear();
            }
            catch (CryptographicException)
            {
                //取消删除证书
            }
            catch (Exception ex)
            {
                throw;
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

        public bool StartProxy(bool IsWindowsProxy = false, bool IsProxyGOG = false)
        {
            if (!IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate))
            {
                var isOk = SetupCertificate();
                if (!isOk)
                {
                    return false;
                }
            }
            if (IsProxyGOG) { WirtePemCertificateToGoGSteamPlugins(); }

            #region 启动代理
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            //var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8888, true)
            //{
            //    // 在所有https请求上使用自颁发的通用证书
            //    // 当代理客户端不需要证书信任时非常有用
            //    //GenericCertificate = new X509Certificate2(Path.Combine(AppContext.BaseDirectory, "genericcert.pfx"), "password")
            //};
            var explicitProxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, ProxyPort, true)
            {
                // 通过不启用为每个http的域创建证书来优化性能
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
            };
            if (IsWindowsProxy)
            {
                //explicit endpoint 是客户端知道代理存在的地方
                proxyServer.AddEndPoint(explicitProxyEndPoint);
            }
            else
            {
                if (PortInUse(443))
                {
                    return false;
                }

                #region 写入Hosts
                //var hosts = new List<(string, string)>();
                //foreach (var proxyDomain in ProxyDomains)
                //{
                //    if (proxyDomain.IsEnable)
                //    {
                //        foreach (var host in proxyDomain.Hosts)
                //        {
                //            if (host.Contains(" "))
                //            {
                //                var h = host.Split(' ');
                //                hosts.Add((h[0], h[1]));
                //            }
                //            else
                //                hosts.Add((IPAddress.Loopback.ToString(), host));
                //        }
                //    }
                //}
                //IHostsFileService.Instance.UpdateHosts(hosts);
                #endregion

                proxyServer.AddEndPoint(new TransparentProxyEndPoint(IPAddress.Any, 443, true)
                {
                    // 通过不启用为每个http的域创建证书来优化性能
                    //GenericCertificate = proxyServer.CertificateManager.RootCertificate
                });
                proxyServer.AddEndPoint(new TransparentProxyEndPoint(IPAddress.Any, 80, false));
            }

            proxyServer.ExceptionFunc = ((Exception exception) =>
            {
                Log.Error("Proxy", exception, "ProxyServer ExceptionFunc");
            });

            try
            {
                proxyServer.Start();
            }
            catch (Exception ex)
            {
                Log.Error("Proxy", ex, nameof(StartProxy));
                return false;
            }

            if (IsWindowsProxy)
            {
                proxyServer.SetAsSystemHttpProxy(explicitProxyEndPoint);
                proxyServer.SetAsSystemHttpsProxy(explicitProxyEndPoint);
                //proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = "localhost", Port = 26501 };
                //proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = "localhost", Port = 26501 };      
            }
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
                proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
                proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
                proxyServer.DisableSystemHttpProxy();
                proxyServer.DisableSystemHttpsProxy();
                proxyServer.Stop();
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
                            var s = file.Substring(Constants.CERTIFICATE_TAG, Constants.CERTIFICATE_TAG, true);
                            if (string.IsNullOrEmpty(s))
                            {
                                File.AppendAllText(certifi, Environment.NewLine + pem);
                            }
                            else if (s.Trim() != pem.Trim())
                            {
                                var index = file.IndexOf(Constants.CERTIFICATE_TAG);
                                File.WriteAllText(certifi, file.Remove(index, s.Length) + pem);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool IsCertificateInstalled(X509Certificate2? certificate2)
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
            }
            proxyServer.Dispose();
        }

    }
}
