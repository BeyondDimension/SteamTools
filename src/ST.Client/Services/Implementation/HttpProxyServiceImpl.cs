using System.Application.Models;
using System.Application.Properties;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Properties;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

        public IReadOnlyCollection<ScriptDTO>? Scripts { get; set; }

        public bool IsEnableScript { get; set; }

        public bool IsOnlyWorkSteamBrowser { get; set; }

        public string CertificateName { get; set; } = ThisAssembly.AssemblyProduct;

        public CertificateEngine CertificateEngine { get; set; }

        public int ProxyPort { get; set; } = 26501;

        public IPAddress ProxyIp { get; set; } = IPAddress.Any;

        public bool ProxyRunning => proxyServer.ProxyRunning;
        public IList<HttpHeader> JsHeader => new List<HttpHeader>() { new HttpHeader("Content-Type", "text/javascript;charset=UTF-8") };

        public const string LocalDomain = "local.steampp.net";

        public HttpProxyServiceImpl()
        {
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //    proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;
            //else
            proxyServer.EnableConnectionPool = true;
            // 可选地设置证书引擎
            proxyServer.CertificateManager.CertificateEngine = CertificateEngine;
            //proxyServer.CertificateManager.PfxPassword = $"{CertificateName}";
            //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;
            proxyServer.CertificateManager.PfxFilePath = Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.pfx");
            proxyServer.CertificateManager.RootCertificateIssuerName = $"{CertificateName} Certificate Authority";
            proxyServer.CertificateManager.RootCertificateName = $"{CertificateName} Certificate";
            proxyServer.CertificateManager.CertificateValidDays = 300;
            //proxyServer.CertificateManager.SaveFakeCertificates = true;

            proxyServer.CertificateManager.RootCertificate = proxyServer.CertificateManager.LoadRootCertificate();
        }

        public async Task HttpRequest(SessionEventArgs e)
        {
            //IHttpService.Instance.SendAsync<object>();
            var url = Web.HttpUtility.UrlDecode(e.HttpClient.Request.RequestUri.Query.Replace("?request=", ""));
            var cookie = e.HttpClient.Request.Headers.GetFirstHeader("cookie-steamTool")?.Value ??
                e.HttpClient.Request.Headers.GetFirstHeader("Cookie")?.Value;
            var headrs = new List<HttpHeader>() {
                new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                new HttpHeader("Access-Control-Allow-Headers", "*"), new HttpHeader("Access-Control-Allow-Methods", "*"),
                new HttpHeader("Access-Control-Allow-Credentials", "true")
            };
            //if (cookie != null)
            //    headrs.Add(new HttpHeader("Cookie", cookie));
            if (e.HttpClient.Request.ContentType != null)
                headrs.Add(new HttpHeader("Content-Type", e.HttpClient.Request.ContentType));
            switch (e.HttpClient.Request.Method.ToUpperInvariant())
            {
                case "GET":
                    var body = await IHttpService.Instance.GetAsync<string>(url, cookie: cookie);
                    e.Ok(body ?? "500", headrs);
                    return;
                case "POST":
                    var requestStream = new StreamWriter(new MemoryStream());
                    try
                    {
                        if (e.HttpClient.Request.ContentLength > 0)
                        {
                            requestStream.Write(e.HttpClient.Request.BodyString);

                            var send = new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                Content = new StreamContent(requestStream.BaseStream),
                            };
                            send.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(e.HttpClient.Request.ContentType);
                            send.Content.Headers.ContentLength = e.HttpClient.Request.BodyString.Length;
                            var conext = await IHttpService.Instance.SendAsync<string>(url, send, null, false, new CancellationToken());
                            e.Ok(conext ?? "500", headrs);
                        }
                        else
                        {
                            e.Ok("500", headrs);
                        }
                    }
                    catch (Exception error)
                    {
                        e.Ok(error.Message ?? "500", headrs);
                    }
                    return;
            }
            //e.Ok(respone, new List<HttpHeader>() { new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*") });
        }

        public async Task OnRequest(object sender, SessionEventArgs e)
        {
#if DEBUG
            Debug.WriteLine("OnRequest " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            Debug.WriteLine("OnRequest HTTP " + e.HttpClient.Request.HttpVersion);
            Debug.WriteLine("ClientRemoteEndPoint " + e.ClientRemoteEndPoint.ToString());
#endif
            if (ProxyDomains is null || e.HttpClient.Request.Host == null)
            {
                return;
            }
            if (e.HttpClient.Request.Host.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase))
            {
                if (e.HttpClient.Request.Method.ToUpperInvariant() == "OPTIONS")
                {
                    e.Ok("", new List<HttpHeader>() {
                        new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                        new HttpHeader("Access-Control-Allow-Headers", "*"),
                        new HttpHeader("Access-Control-Allow-Methods", "*"),
                        new HttpHeader("Access-Control-Allow-Credentials", "true") });
                    return;
                }
                var type = e.HttpClient.Request.Headers.GetFirstHeader("requestType")?.Value;
                switch (type)
                {
                    case "xhr":
                        await HttpRequest(e);

                        return;
                    default:
                        e.Ok(Scripts.FirstOrDefault(x => x.JsPathUrl == e.HttpClient.Request.RequestUri.LocalPath)?.Content ?? "404", JsHeader);
                        return;
                }
            }
            foreach (var item in ProxyDomains)
            {
                foreach (var host in item.DomainNamesArray)
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
                            if (CloudService.Constants.IsHttpUrl(item.ForwardDomainName))
                            {
                                e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Scheme + "://" + e.HttpClient.Request.RequestUri.Host, item.ForwardDomainName));
                                return;
                            }

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
                            if (IPAddress.IsLoopback(iPs.FirstOrDefault()))
                                return;
                            ip = iPs.First();
                            //Logger.Info("Proxy IP: " + iP);
                        }
                        if (ip != null)
                        {
                            e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
                        }
                        //e.HttpClient.Request.Host = item.ForwardDomainName ?? e.HttpClient.Request.Host;
                        if (e.HttpClient.ConnectRequest?.ClientHelloInfo?.Extensions != null)
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

            //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
            //if (!e.IsTransparent)
            //{
            if (IPAddress.IsLoopback(e.ClientRemoteEndPoint.Address))
            {
                e.TerminateSession();
                Log.Info("Proxy", "IsLoopback OnRequest: " + e.HttpClient.Request.RequestUri.AbsoluteUri);
                return;
            }
            //}

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
                if (e.HttpClient.Request.Method == "GET")
                {
                    if (e.HttpClient.Response.StatusCode == 200)
                    {
                        if (e.HttpClient.Response.ContentType != null && e.HttpClient.Response.ContentType.Trim().ToLower().Contains("text/html"))
                        {
                            if (IsOnlyWorkSteamBrowser)
                            {
                                var ua = e.HttpClient.Request.Headers.GetHeaders("User-Agent");
                                if (ua.Any_Nullable())
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
                                if (script.ExcludeDomainNamesArray != null)
                                    foreach (var host in script.ExcludeDomainNamesArray)
                                    {
                                        if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host))
                                            goto close;
                                    }
                                foreach (var host in script.MatchDomainNamesArray)
                                {
                                    var state = host.IndexOf("/") == 0;
                                    if (state)
                                        state = Regex.IsMatch(e.HttpClient.Request.RequestUri.AbsoluteUri, host.Substring(1), RegexOptions.Compiled);
                                    else
                                        state = e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host);
                                    if (state)
                                    {
                                        var t = e.HttpClient.Response.Headers.GetFirstHeader("Content-Security-Policy");
                                        if (!string.IsNullOrEmpty(t?.Value))
                                        {
                                            e.HttpClient.Response.Headers.RemoveHeader(t);
                                        }
                                        var doc = await e.GetResponseBodyAsString();
                                        var index = doc.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                                        if (index == -1)
                                            index = doc.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                                        if (script.JsPathUrl == null)
                                            script.JsPathUrl = $"/{Guid.NewGuid()}";
                                        var temp = $"<script type=\"text/javascript\" src=\"https://local.steampp.net{script.JsPathUrl}\"></script>";
                                        if (index > -1)
                                        {
                                            doc = doc.Insert(index, temp);
                                            e.SetResponseBodyString(doc);
                                        }
                                    }
                                }
                            close:;
                            }
                        }
                    }
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
        public void TrustCer() {
            var filePath = Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.cer");
            IPlatformService.Instance.AdminShell($"security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain \"{filePath}\"", true);
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

            var filePath = Path.Combine(IOPath.AppDataDirectory, $@"{CertificateName}.Certificate.cer");

            proxyServer.CertificateManager.RootCertificate.SaveCerCertificateFile(filePath);
            try
            {
                proxyServer.CertificateManager.TrustRootCertificate();
            }
            catch { }
            proxyServer.CertificateManager.EnsureRootCertificate();
            if (DI.IsmacOS)
            {
                TrustCer(); 

            }
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
                if (IsCertificateInstalled(proxyServer.CertificateManager.RootCertificate) == false)
                {
                    proxyServer.CertificateManager.RootCertificate = null;
                    if (File.Exists(proxyServer.CertificateManager.PfxFilePath))
                        File.Delete(proxyServer.CertificateManager.PfxFilePath);
                }
                //proxyServer.CertificateManager.RemoveTrustedRootCertificateAsAdmin();
                //proxyServer.CertificateManager.CertificateStorage.Clear();
            }
            catch (CryptographicException)
            {
                //取消删除证书
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }

        public int GetRandomUnusedPort()
        {
            var listener = new TcpListener(ProxyIp, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public bool PortInUse(int port)
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
                DeleteCertificate();
                var isOk = SetupCertificate();
                if (!isOk)
                {
                    return false;
                }
            }
            //else
            //{
            //    SetupCertificate();
            //}

            if (IsProxyGOG) { WirtePemCertificateToGoGSteamPlugins(); }

            #region 启动代理
            //proxyServer.Enable100ContinueBehaviour = true;
            proxyServer.EnableHttp2 = true;
            proxyServer.BeforeRequest += OnRequest;
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            try
            {
                var explicitProxyEndPoint = new ExplicitProxyEndPoint(ProxyIp, GetRandomUnusedPort(), true)
                {
                    // 通过不启用为每个http的域创建证书来优化性能
                    //GenericCertificate = proxyServer.CertificateManager.RootCertificate
                };
                explicitProxyEndPoint.BeforeTunnelConnectRequest += ExplicitProxyEndPoint_BeforeTunnelConnectRequest;

                if (IsWindowsProxy)
                {
                    //explicit endpoint 是客户端知道代理存在的地方
                    proxyServer.AddEndPoint(explicitProxyEndPoint);
                }
                else
                {
                    //if (PortInUse(443))
                    //{
                    //    return false;
                    //}
                    var transparentProxyEndPoint = new TransparentProxyEndPoint(ProxyIp, 443, true)
                    {
                        // 通过不启用为每个http的域创建证书来优化性能
                        //GenericCertificate = proxyServer.CertificateManager.RootCertificate
                    };
                    transparentProxyEndPoint.BeforeSslAuthenticate += TransparentProxyEndPoint_BeforeSslAuthenticate;

                    proxyServer.AddEndPoint(transparentProxyEndPoint);

                    if (PortInUse(80) == false)
                        proxyServer.AddEndPoint(new TransparentProxyEndPoint(ProxyIp, 80, false));
                }

                proxyServer.ExceptionFunc = ((Exception exception) =>
                {
                    Log.Error("Proxy", exception, "ProxyServer ExceptionFunc");
                });

                proxyServer.Start();

                if (IsWindowsProxy)
                {
                    if (DI.Platform == Platform.Windows)
                    {
                        proxyServer.SetAsSystemHttpProxy(explicitProxyEndPoint);
                        proxyServer.SetAsSystemHttpsProxy(explicitProxyEndPoint);
                    }
                    else if (DI.IsmacOS)
                    {
                        var stringList = IPlatformService.Instance.GetMacNetworksetup();
                        var shellContent = new StringBuilder();
                        foreach (var item in stringList)
                        {
                            if (item.Trim().Length > 0) {
                                //shellContent.AppendLine($"networksetup -setsocksfirewallproxy '{item}' '{explicitProxyEndPoint.IpAddress}' {explicitProxyEndPoint.Port}");
                                //shellContent.AppendLine($"networksetup -setsocksfirewallproxystate '{item}' on");
                                shellContent.AppendLine($"networksetup -setwebproxy '{item}' '127.0.0.1' {explicitProxyEndPoint.Port}");
                                shellContent.AppendLine($"networksetup -setwebproxystate '{item}' on");
                                shellContent.AppendLine($"networksetup -setsecurewebproxy '{item}' '127.0.0.1' {explicitProxyEndPoint.Port}");
                                shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' on");
                            }
                        }
                        IPlatformService.Instance.AdminShell(shellContent.ToString(), false);

                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error("Proxy", ex, nameof(StartProxy));
                return false;
            }

            #endregion
#if DEBUG
            foreach (var endPoint in proxyServer.ProxyEndPoints)
                Debug.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
                    endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
#endif
            return true;
        }

        private Task TransparentProxyEndPoint_BeforeSslAuthenticate(object sender, BeforeSslAuthenticateEventArgs e)
        {
            e.DecryptSsl = false;
            if (e.SniHostName.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase))
            {
                e.DecryptSsl = true;
                return Task.CompletedTask;
            }
            if (ProxyDomains is null)
            {
                return Task.CompletedTask;
            }
            foreach (var item in ProxyDomains)
            {
                foreach (var host in item.DomainNamesArray)
                {
                    if (e.SniHostName.Contains(new Uri("https://" + host).Host))
                    {
                        e.DecryptSsl = true;
                        return Task.CompletedTask;
                    }
                }
            }

            var ip = Dns.GetHostAddresses(e.SniHostName).FirstOrDefault();
            if (IPAddress.IsLoopback(ip))
            {
                e.TerminateSession();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private Task ExplicitProxyEndPoint_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            e.DecryptSsl = false;
            if (ProxyDomains is null || e.HttpClient?.Request?.Host == null)
            {
                return Task.CompletedTask;
            }
            if (e.HttpClient.Request.Host.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase))
            {
                e.DecryptSsl = true;
                return Task.CompletedTask;
            }
            foreach (var item in ProxyDomains)
            {
                foreach (var host in item.DomainNamesArray)
                {
                    if (e.HttpClient.Request.Url.Contains(host))
                    {
                        e.DecryptSsl = true;
                        return Task.CompletedTask;
                    }
                }
            }
            var ip = Dns.GetHostAddresses(e.HttpClient.Request.Host).FirstOrDefault();
            if (IPAddress.IsLoopback(ip))
            {
                e.TerminateSession();
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
        public void StopMacProxy()
        {
            var stringList = IPlatformService.Instance.GetMacNetworksetup();
            var shellContent = new StringBuilder();
            foreach (var item in stringList)
            {
                if (item.Trim().Length > 0)
                { 
                    //shellContent.AppendLine($"networksetup -setsocksfirewallproxystate '{item}' off");
                    shellContent.AppendLine($"networksetup -setwebproxystate '{item}' off");
                    shellContent.AppendLine($"networksetup -setsecurewebproxystate '{item}' off");
                }
            }
            IPlatformService.Instance.AdminShell(shellContent.ToString(), false);

        }
        public void StopProxy()
        {
            if (proxyServer.ProxyRunning)
            {
                proxyServer.BeforeRequest -= OnRequest;
                proxyServer.BeforeResponse -= OnResponse;
                proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
                proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
                if (DI.IsmacOS)
                {
                    StopMacProxy();
                }
                proxyServer.Stop();

            }
            try
            {
                proxyServer.DisableAllSystemProxies();
            }
            catch
            {
                //忽略异常导致的崩溃
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
            if (certificate2.NotAfter <= DateTime.Now)
                return false;
            using var store = new X509Store(DI.Platform == Platform.Apple ? StoreName.My : StoreName.Root, StoreLocation.CurrentUser);
            store.Open(OpenFlags.MaxAllowed);
            return store.Certificates.Contains(certificate2);
        }

        public void Dispose()
        {
            if (proxyServer.ProxyRunning)
            {
                if (DI.IsmacOS)
                {
                    StopMacProxy();
                }
                proxyServer.Stop();
            }
            proxyServer.Dispose();
        }
    }
}