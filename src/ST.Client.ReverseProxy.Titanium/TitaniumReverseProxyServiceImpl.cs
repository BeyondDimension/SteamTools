using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;
using Titanium.Web.Proxy.StreamExtended.Models;
using static System.Application.Services.IReverseProxyService;

namespace System.Application.Services.Implementation;

sealed class TitaniumReverseProxyServiceImpl : ReverseProxyServiceImpl, IReverseProxyService
{
    public override EReverseProxyEngine ReverseProxyEngine => EReverseProxyEngine.Titanium;

    readonly ProxyServer proxyServer = new();

    private static bool IsIpv6Support = false;

    public TitaniumReverseProxyServiceImpl(
        IPlatformService platformService,
        IDnsAnalysisService dnsAnalysis) : base(platformService, dnsAnalysis)
    {
        //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;
        //else
        proxyServer.ExceptionFunc = OnException;

        proxyServer.EnableHttp2 = true;
        proxyServer.EnableConnectionPool = true;
        proxyServer.CheckCertificateRevocation = X509RevocationMode.NoCheck;

        CertificateManager = new TitaniumCertificateManagerImpl(platformService, this, proxyServer.CertificateManager);
        //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;
    }

    public IPAddress[] DefaultDnsServers => IDnsAnalysisService.DNS_Alis;

    public override ICertificateManager CertificateManager { get; }

    public override bool ProxyRunning => proxyServer.ProxyRunning;

    static IList<HttpHeader> JsHeader => new List<HttpHeader>() { new HttpHeader("Content-Type", "text/javascript;charset=UTF-8") };

    static async Task HttpRequest(SessionEventArgs e)
    {
        //IHttpService.Instance.SendAsync<object>();
        var url = Web.HttpUtility.UrlDecode(e.HttpClient.Request.RequestUri.Query.Replace("?request=", ""));
        var cookie = e.HttpClient.Request.Headers.GetFirstHeader("cookie-steamTool")?.Value ??
            e.HttpClient.Request.Headers.GetFirstHeader("Cookie")?.Value;
        var headrs = new List<HttpHeader>()
            {
                new("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                new("Access-Control-Allow-Headers", "*"),
                new("Access-Control-Allow-Methods", "*"),
                new("Access-Control-Allow-Credentials", "true"),
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
                try
                {
                    if (e.HttpClient.Request.ContentLength > 0)
                    {
                        var conext = await IHttpService.Instance.SendAsync<string>(url, () =>
                        {
                            using var sw = new MemoryStream().GetWriter(leaveOpen: true);
                            sw.Write(e.HttpClient.Request.BodyString);
                            var req = new HttpRequestMessage
                            {
                                Method = HttpMethod.Post,
                                Content = new StreamContent(sw.BaseStream),
                            };
                            req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(e.HttpClient.Request.ContentType);
                            req.Content.Headers.ContentLength = e.HttpClient.Request.BodyString.Length;
                            return req;
                        }, null/*, false*/, default);
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

    async Task<IPAddress?> GetReverseProxyIp(string url, IPAddress? proxyDns, bool isDomain = false)
    {
        if (isDomain || !IPAddress.TryParse(url, out var ip))
        {
            if (proxyDns != null)
            {
                ip = await DnsAnalysis.AnalysisDomainIpAsync(url, new[] { proxyDns }, IsIpv6Support).FirstOrDefaultAsync();
            }
            else
            {
                if (!OperatingSystem2.IsWindows() && !IsSystemProxy)
                {
                    // 非 windows 环境 hosts 加速下不能使用系统默认 DNS 解析代理，会解析到 hosts 上无限循环
                    ip = await DnsAnalysis.AnalysisDomainIpAsync(url, DefaultDnsServers, IsIpv6Support).FirstOrDefaultAsync();
                }
                else
                {
                    ip = await DnsAnalysis.AnalysisDomainIpAsync(url, IsIpv6Support).FirstOrDefaultAsync();
                }
            }
        }
        return ip;
    }

    async Task OnRequest(object sender, SessionEventArgs e)
    {
#if DEBUG
        Debug.WriteLine("OnRequest " + e.HttpClient.Request.RequestUri.AbsoluteUri);
        Debug.WriteLine("OnRequest HTTP " + e.HttpClient.Request.HttpVersion);
        Debug.WriteLine("ClientRemoteEndPoint " + e.ClientRemoteEndPoint.ToString());
#endif
        if (e.HttpClient.Request.Host == null) return;

        if (e.HttpClient.Request.Host.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase))
        {
            if (e.HttpClient.Request.Method.ToUpperInvariant() == "OPTIONS")
            {
                e.Ok("", new List<HttpHeader>()
                    {
                        new HttpHeader("Access-Control-Allow-Origin", e.HttpClient.Request.Headers.GetFirstHeader("Origin")?.Value ?? "*"),
                        new HttpHeader("Access-Control-Allow-Headers", "*"),
                        new HttpHeader("Access-Control-Allow-Methods", "*"),
                        new HttpHeader("Access-Control-Allow-Credentials", "true"),
                    });
                return;
            }
            var type = e.HttpClient.Request.Headers.GetFirstHeader("requestType")?.Value;
            switch (type)
            {
                case "xhr":
                    await HttpRequest(e);
                    return;
                default:
                    e.Ok(Scripts?.FirstOrDefault(x => x.JsPathUrl == e.HttpClient.Request.RequestUri.LocalPath)?.Content ?? "404", JsHeader);
                    return;
            }
        }

        //host模式下不启用加速会出现无限循环问题
        if (ProxyDomains is null || TwoLevelAgentEnable || (OnlyEnableProxyScript && IsSystemProxy)) return;

        //var item = ProxyDomains.FirstOrDefault(f => f.DomainNamesArray.Any(h => e.HttpClient.Request.RequestUri.AbsoluteUri.Contains(h, StringComparison.OrdinalIgnoreCase)));

        //if (item != null)
        //{

        //e.HttpClient.Request.Headers.AddHeader("User-Agent", "Watt Toolkit Proxy/" + ThisAssembly.Version);

        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsDomainPattern(host, RegexOptions.IgnoreCase))
                {
                    if (!e.HttpClient.IsHttps)
                    {
                        e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                        //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
                        //return;
                    }

                    if (item.Redirect)
                    {
                        var url = item.ForwardDomainName.Replace("{path}", e.HttpClient.Request.RequestUri.AbsolutePath);
                        url = url.Replace("{args}", e.HttpClient.Request.RequestUri.Query);
                        //url = url.Replace("{url}", e.HttpClient.Request.RequestUri.AbsoluteUri);
                        if (Browser2.IsHttpUrl(url))
                        {
                            e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Scheme + "://" + e.HttpClient.Request.RequestUri.Host, url));
                            //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Scheme + "://" + e.HttpClient.Request.RequestUri.Host, url));
                            return;
                        }
                        e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Host, url));
                        //e.Redirect(e.HttpClient.Request.RequestUri.AbsoluteUri.Replace(e.HttpClient.Request.RequestUri.Host, url));
                        return;
                    }

                    if (e.HttpClient.UpStreamEndPoint == null)
                    {
                        var addres = item.ForwardDomainIsNameOrIP ? item.ForwardDomainName : item.ForwardDomainIP;
                        var ip = await GetReverseProxyIp(addres, ProxyDNS, item.ForwardDomainIsNameOrIP);
                        if (ip == null || IPAddress.IsLoopback(ip) || ip.Equals(IPAddress.Any))
                            goto exit;
                        e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
                    }

                    if (!string.IsNullOrEmpty(item.UserAgent))
                    {
                        var oldua = e.HttpClient.Request.Headers.GetFirstHeader("User-Agent")?.Value;
                        e.HttpClient.Request.Headers.RemoveHeader("User-Agent");
                        var newUA = item.UserAgent.Replace("${origin}", oldua);
                        e.HttpClient.Request.Headers.AddHeader("User-Agent", newUA);
                    }

                    if (e.HttpClient.ConnectRequest?.ClientHelloInfo?.Extensions != null)
                    {
#if DEBUG
                        //Logger.Info("ClientHelloInfo Info: " + e.HttpClient.ConnectRequest.ClientHelloInfo);
                        Debug.WriteLine("ClientHelloInfo Info: " + e.HttpClient.ConnectRequest.ClientHelloInfo);
#endif
                        if (!string.IsNullOrEmpty(item.ServerName))
                        {
                            var sni = e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"];
                            e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions["server_name"] =
                                new SslExtension(sni.Value, sni.Name, item.ServerName, sni.Position);
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
    //}

    exit:
        //部分运营商将奇怪的域名解析到127.0.0.1 再此排除这些不支持的代理域名
        if (IPAddress.IsLoopback(e.ClientRemoteEndPoint.Address))
        {
            var ip = await DnsAnalysis.AnalysisDomainIpAsync(e.HttpClient.Request.Host, DefaultDnsServers).FirstOrDefaultAsync();
            if (ip == null || IPAddress.IsLoopback(ip))
            {
                e.TerminateSession();
                Log.Info(TAG, "IsLoopback OnRequest: " + e.HttpClient.Request.RequestUri.AbsoluteUri);
            }
            else
            {
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, e.ClientRemoteEndPoint.Port);
            }
        }
        return;
    }

    async Task OnResponse(object sender, SessionEventArgs e)
    {
#if DEBUG
        Debug.WriteLine("OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
        Log.Info(TAG, "OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
        if (Scripts is null)
        {
            return;
        }
        if (IsEnableScript &&
            e.HttpClient.Request.Method == "GET" &&
            e.HttpClient.Response.StatusCode == 200 &&
            e.HttpClient.Response.ContentType?.Contains("text/html", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (IsOnlyWorkSteamBrowser)
            {
                var ua = e.HttpClient.Request.Headers.GetHeaders("User-Agent");
                if (ua?.FirstOrDefault()?.Value.Contains("Valve Steam") == false)
                {
                    return;
                }
            }

            StringBuilder scriptHtml = new();

            foreach (var script in Scripts)
            {
                if (script.ExcludeDomainNamesArray != null)
                    foreach (var host in script.ExcludeDomainNamesArray)
                    {
                        if (e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host, RegexOptions.IgnoreCase))
                            goto next;
                    }

                foreach (var host in script.MatchDomainNamesArray)
                {
                    var state = host.IndexOf("/") == 0;
                    if (state)
                        state = Regex.IsMatch(e.HttpClient.Request.RequestUri.AbsoluteUri, host[1..], RegexOptions.IgnoreCase);
                    else
                        state = e.HttpClient.Request.RequestUri.AbsoluteUri.IsWildcard(host, RegexOptions.IgnoreCase);
                    if (state)
                    {
                        var t = e.HttpClient.Response.Headers.GetFirstHeader("Content-Security-Policy");
                        if (!string.IsNullOrEmpty(t?.Value))
                        {
                            e.HttpClient.Response.Headers.RemoveHeader(t);
                        }

                        if (script.JsPathUrl == null)
                            script.JsPathUrl = $"/{Guid.NewGuid()}";
                        var temp = $"<script type=\"text/javascript\" src=\"https://local.steampp.net{script.JsPathUrl}\"></script>";

                        scriptHtml.AppendLine(temp);
                    }
                }
            next:;
            }

            if (scriptHtml.Length > 0)
            {
                var doc = await e.GetResponseBodyAsString();
                var index = doc.LastIndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    index = doc.LastIndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if (index > -1)
                {
                    doc = doc.Insert(index, scriptHtml.ToString());
                    e.SetResponseBodyString(doc);
                }
            }
        }
    }

    // 允许重写默认的证书验证逻辑
    static Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
    {
        // 根据证书错误，设置IsValid为真/假
        //if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
        e.IsValid = true;
        return Task.CompletedTask;
    }

    // 允许在相互身份验证期间重写默认客户端证书选择逻辑
    static Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
    {
        // set e.clientCertificate to override
        return Task.CompletedTask;
    }

    protected override async Task<bool> StartProxyImpl()
    {
        #region 启动代理

        proxyServer.BeforeRequest += OnRequest;
        proxyServer.BeforeResponse += OnResponse;
        proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
        //proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

        try
        {
            if (PortInUse(ProxyPort)) ProxyPort = GetRandomUnusedPort();
            var explicitProxyEndPoint = new ExplicitProxyEndPoint(ProxyIp, ProxyPort, true)
            {
                // 通过不启用为每个http的域创建证书来优化性能
                //GenericCertificate = proxyServer.CertificateManager.RootCertificate
            };
            explicitProxyEndPoint.BeforeTunnelConnectRequest += ExplicitProxyEndPoint_BeforeTunnelConnectRequest;

            if (IsSystemProxy)
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

                TransparentProxyEndPoint transparentProxyEndPoint;

                transparentProxyEndPoint = new TransparentProxyEndPoint(ProxyIp, 443, true)
                {
                    // 通过不启用为每个http的域创建证书来优化性能
                    //GenericCertificate = proxyServer.CertificateManager.RootCertificate
                };
                //transparentProxyEndPoint.BeforeSslAuthenticate += TransparentProxyEndPoint_BeforeSslAuthenticate;
                proxyServer.AddEndPoint(transparentProxyEndPoint);

                try
                {
                    if (EnableHttpProxyToHttps && PortInUse(80) == false)
                        proxyServer.AddEndPoint(new TransparentProxyEndPoint(ProxyIp, 80, false));
                }
                catch { }
            }

            if (Socks5ProxyEnable)
            {
                var socks5 = new SocksProxyEndPoint(ProxyIp, Socks5ProxyPortId, true);
                socks5.BeforeSslAuthenticate += TransparentProxyEndPoint_BeforeSslAuthenticate;
                proxyServer.AddEndPoint(socks5);
            }

            if (TwoLevelAgentEnable && TwoLevelAgentIp != null)
            {
                proxyServer.UpStreamHttpsProxy = new ExternalProxy(TwoLevelAgentIp, TwoLevelAgentPortId)
                {
                    ProxyDnsRequests = true,
                    BypassLocalhost = true,
                    ProxyType = (ExternalProxyType)TwoLevelAgentProxyType,
                    UserName = TwoLevelAgentUserName,
                    Password = TwoLevelAgentPassword,
                };
                proxyServer.ForwardToUpstreamGateway = true;
            }

            IsIpv6Support = await DnsAnalysis.GetIsIpv6Support();

            proxyServer.Start();

            if (IsSystemProxy)
            {
                if (!DesktopBridge.IsRunningAsUwp && OperatingSystem2.IsWindows())
                {
                    proxyServer.SetAsSystemProxy(explicitProxyEndPoint, ProxyProtocolType.AllHttp);
                }
                else
                {
                    if (!IPlatformService.Instance.SetAsSystemProxy(true, explicitProxyEndPoint.IpAddress, explicitProxyEndPoint.Port))
                    {
                        Log.Error(TAG, "系统代理开启失败");
                        return false;
                    }
                }
            }
            if (IsProxyGOG) { WirtePemCertificateToGoGSteamPlugins(); }
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, nameof(StartProxy));
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

    Task TransparentProxyEndPoint_BeforeSslAuthenticate(object sender, BeforeSslAuthenticateEventArgs e)
    {
        e.DecryptSsl = false;
        if (ProxyDomains is null)
        {
            return Task.CompletedTask;
        }
        if (e.SniHostName.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase) || TwoLevelAgentEnable)
        {
            e.DecryptSsl = true;
            return Task.CompletedTask;
        }
        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (Uri.TryCreate(host, UriKind.RelativeOrAbsolute, out var u))
                {
                    string h;
                    if (u.IsAbsoluteUri)
                        h = u.Host;
                    else
                        h = u.OriginalString;

                    if (e.SniHostName.IsDomainPattern(h, RegexOptions.IgnoreCase))
                    {
                        e.ForwardHttpsHostName = item.ServerName;
                        e.ForwardHttpsPort = item.PortId;
                        e.DecryptSsl = true;
                        return Task.CompletedTask;
                    }
                }
            }
        }
        //var ip = Dns.GetHostAddresses(e.SniHostName).FirstOrDefault();
        //if (IPAddress.IsLoopback(ip))
        //{
        //    e.TerminateSession();
        //    return Task.CompletedTask;
        //}
        return Task.CompletedTask;
    }

    async Task ExplicitProxyEndPoint_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
    {
        e.DecryptSsl = false;
        if (ProxyDomains is null || e.HttpClient?.Request?.Host == null)
        {
            return;
        }
        if (e.HttpClient.Request.Host.Contains(LocalDomain, StringComparison.OrdinalIgnoreCase) || TwoLevelAgentEnable || OnlyEnableProxyScript)
        {
            e.DecryptSsl = true;
            return;
        }
        foreach (var item in ProxyDomains)
        {
            foreach (var host in item.DomainNamesArray)
            {
                if (e.HttpClient.Request.Url.IsDomainPattern(host, RegexOptions.IgnoreCase))
                {
                    e.DecryptSsl = true;
                    if (item.ProxyType == ProxyType.Local ||
                        item.ProxyType == ProxyType.ServerAccelerate)
                    {
                        var addres = item.ForwardDomainIsNameOrIP ? item.ForwardDomainName : item.ForwardDomainIP;
                        var ip = await GetReverseProxyIp(addres, ProxyDNS, item.ForwardDomainIsNameOrIP);
                        if (ip != null && !IPAddress.IsLoopback(ip) && !ip.Equals(IPAddress.Any))
                        {
                            e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, item.PortId);
                        }
                    }
                    return;
                }
            }
        }
        //var ip = Dns.GetHostAddresses(e.HttpClient.Request.Host).FirstOrDefault();
        //if (IPAddress.IsLoopback(ip))
        //{
        //    e.TerminateSession();
        //    return Task.CompletedTask;
        //}
        return;
    }

    public void StopProxy()
    {
        try
        {
            if (proxyServer.ProxyRunning)
            {
                proxyServer.BeforeRequest -= OnRequest;
                proxyServer.BeforeResponse -= OnResponse;
                proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
                proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
                proxyServer.Stop();
            }

            if (IsSystemProxy)
            {
                if (DesktopBridge.IsRunningAsUwp || !OperatingSystem2.IsWindows())
                {
                    IPlatformService.Instance.SetAsSystemProxy(false);
                }
                else
                {
                    proxyServer.DisableAllSystemProxies();
                }
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT(TAG);
        }
    }

    protected override void DisposeCore()
    {
        if (proxyServer.ProxyRunning)
        {
            StopProxy();
        }
        proxyServer.Dispose();
    }
}
