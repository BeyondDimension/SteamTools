// ReSharper disable once CheckNamespace
using BD.WTTS.Helpers;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.Services;

partial class ProxyService
{
    const ProxyMode defaultProxyMode = ProxyMode.Hosts;
    const ushort httpsPort = 443;
    ProxyMode proxyMode = defaultProxyMode;

    static string? proxyTokenCache = null;

    /// <summary>
    /// 启动代理服务
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<OperateProxyServiceResult> StartProxyServiceAsync()
    {
        try
        {
            var result = await StartProxyServiceCoreAsync();
            return result;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    async Task<OperateProxyServiceResult> StartProxyServiceCoreAsync()
    {
        IReadOnlyCollection<AccelerateProjectDTO>? proxyDomains = EnableProxyDomains;

        if (!proxyDomains.Any_Nullable())
        {
            Toast.Show(ToastIcon.Warning, Strings.CommunityFix_AccEmpty);
        }

        TracepointHelper.TrackEvent("StartProxy", new Dictionary<string, string> {
            { "ProxyType", ProxySettings.ProxyMode.Value.ToString() },
            { "ProxyScriptStatus", ProxySettings.IsEnableScript.Value.ToString() },
        });

        IReadOnlyCollection<ScriptIPCDTO>? scripts = default;
        bool isEnableScript = ProxySettings.IsEnableScript.Value;
        bool isOnlyWorkSteamBrowser = ProxySettings.IsOnlyWorkSteamBrowser.Value;
        ushort proxyPort = ProxySettings.SystemProxyPortId.Value == 0 ? ProxySettings.SystemProxyPortId.Default : ProxySettings.SystemProxyPortId.Value;
        string? proxyIp = ProxySettings.SystemProxyIp.Value;
        proxyMode = defaultProxyMode;
        bool isProxyGOG = ProxySettings.IsProxyGOG.Value;
        bool onlyEnableProxyScript = ProxySettings.OnlyEnableProxyScript.Value;
        bool enableHttpProxyToHttps = ProxySettings.EnableHttpProxyToHttps.Value;
        bool socks5ProxyEnable = ProxySettings.Socks5ProxyEnable.Value ||
            (OperatingSystem.IsAndroid() && // Android VPN 模式使用 tun2socks
                ProxySettings.ProxyModeValue == ProxyMode.VPN);
        ushort socks5ProxyPortId = ProxySettings.Socks5ProxyPortId.Value;
        if (!ModelValidatorProvider.IsPortId(socks5ProxyPortId))
            socks5ProxyPortId = IProxySettings.DefaultSocks5ProxyPortId;
        bool twoLevelAgentEnable = ProxySettings.TwoLevelAgentEnable.Value;
        ExternalProxyType twoLevelAgentProxyType = ProxySettings.TwoLevelAgentProxyType.Value;
        if (!twoLevelAgentProxyType.IsDefined())
            twoLevelAgentProxyType = IReverseProxyService.Constants.DefaultTwoLevelAgentProxyType;
        string? twoLevelAgentIp = ReverseProxySettings.GetTwoLevelAgentIp(ProxySettings.TwoLevelAgentIp.Value);
        ushort twoLevelAgentPortId = ProxySettings.TwoLevelAgentPortId.Value;
        if (!ModelValidatorProvider.IsPortId(twoLevelAgentPortId))
            twoLevelAgentPortId = IProxySettings.DefaultTwoLevelAgentPortId;
        string? twoLevelAgentUserName = ProxySettings.TwoLevelAgentUserName.Value;
        string? twoLevelAgentPassword = ProxySettings.TwoLevelAgentPassword.Value;
        ProxySettings.MigrateDnsServerList();
        string? proxyDNS = ProxySettings.ProxyMasterDns.Value;
        if (string.IsNullOrWhiteSpace(proxyDNS))
            proxyDNS = IDnsAnalysisService.DefaultResolverServerList;
        bool isSupportIpv6 = await RefreshIpv6Support();

        string? proxyToken = proxyDomains.Any_Nullable(s => s.ProxyType == ProxyType.ServerAccelerate || s.Items.Any_Nullable(x => x.ProxyType == ProxyType.ServerAccelerate)) ?
            await TryRequestServerSideProxyToken() : null;

        if (ProxySettings.ProxyBeforeDNSCheck.Value)
        {
            var servers = proxyDNS.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            if (!await CheckDnsServersAsync(servers, timeout.Token))
                Toast.Show(ToastIcon.Warning, "DNS 预检查未通过或超时，加速仍会按配置顺序尝试服务器；请检查网络或解析设置。");
        }

        Lazy<IPAddress> proxyIp_ = new(() => ReverseProxySettings.GetProxyIp(proxyIp));
        void SetProxyIp(IPAddress proxyIPAddress)
        {
            proxyIp_ = new(proxyIPAddress);
            proxyIp = proxyIPAddress.ToString();
        }

        if (isEnableScript)
        {
            await EnableProxyScripts.ContinueWith(e =>
            {
                scripts = e.Result?.Select(item => new ScriptIPCDTO(
                               item.LocalId,
                               item.CachePath,
                               item.MatchDomainNamesArray,
                               item.ExcludeDomainNames,
                               item.Order)
                          ).ToImmutableArray();
                //.Select(item =>
                //          new ScriptIPCDTO(
                //               item.LocalId,
                //               item.CachePath,
                //               item.MatchDomainNamesArray,
                //               item.ExcludeDomainNames)
                //          );
            });
        }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
#if MACCATALYST
                    if (OperatingSystem.IsMacOS())
#endif
        {
            isOnlyWorkSteamBrowser = ProxySettings.IsOnlyWorkSteamBrowser.Value;
            proxyMode = ProxySettings.ProxyMode.Value;
            if (!proxyMode.IsDefined())
                proxyMode = defaultProxyMode;
            isProxyGOG = ProxySettings.IsProxyGOG.Value;
        }
#endif
        // macOS\Linux 上目前因权限问题仅支持 0.0.0.0(IPAddress.Any)
        if ((OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            && IPAddress.IsLoopback(proxyIp_.Value))
        {
            SetProxyIp(IPAddress.Any);
        }

        //reverseProxyService.HostProxyPortId = ProxySettings.HostProxyPortId;

        this.RaisePropertyChanged(nameof(EnableProxyDomains));
        this.RaisePropertyChanged(nameof(EnableProxyScripts));

        switch (proxyMode)
        {
#if REMOVE_DNS_INTERCEPT
            case ProxyMode.DNSIntercept:
#endif
            case ProxyMode.Hosts:
                var inUsePort = SocketHelper.IsUsePort(proxyIp_.Value, httpsPort);
                if (inUsePort)
                {
                    if (!OperatingSystem.IsLinux())
                    //{
                    //    //if (string.IsNullOrWhiteSpace(Plugin.Instance.SubProcessPath))
                    //    //{
                    //    //}
                    //    //    var processPath = Environment.ProcessPath;
                    //    //    processPath.ThrowIsNull();
                    //    //    var urlUnixHostAccess = string.Format(
                    //    //        Constants.Urls.OfficialWebsite_UnixHostAccess_,
                    //    //        WebUtility.UrlEncode(processPath));
                    //    //    Browser2.Open(urlUnixHostAccess);
                    //}
                    //else
                    {
                        string? error_CommunityFix_StartProxyFaild443 = default;
                        if (OperatingSystem.IsWindows())
                        {
#pragma warning disable CA1416 // 验证平台兼容性
                            var p = SocketHelper.GetProcessByTcpPort(httpsPort);
#pragma warning restore CA1416 // 验证平台兼容性
                            if (p != null)
                            {
                                error_CommunityFix_StartProxyFaild443 =
                                    Strings.CommunityFix_StartProxyFaild443___.Format(
                                        httpsPort, p.ProcessName, p.Id);
                            }
                        }
                        error_CommunityFix_StartProxyFaild443 ??=
                            Strings.CommunityFix_StartProxyFaild443_.Format(httpsPort);
                        return error_CommunityFix_StartProxyFaild443;
                    }
                }
                break;

            case ProxyMode.System:
                if (OperatingSystem.IsWindows() &&
                    proxyIp_.Value.Equals(IPAddress.Any))
                {
                    SetProxyIp(IPAddress.Loopback);
                }
                break;

            case ProxyMode.PAC:
                if (OperatingSystem.IsWindows() &&
                    proxyIp_.Value.Equals(IPAddress.Any))
                {
                    SetProxyIp(IPAddress.Loopback);
                }
                break;
#if WINDOWS && !REMOVE_DNS_INTERCEPT
            case ProxyMode.DNSIntercept:
                {
                    await Mobius.Helpers.WinDivertInitHelper.InitializeAsync();
                }
                break;
#endif
        }

        if (!OperatingSystem.IsWindows())
        {
            var checkRootCertificateCode = ICertificateManager.Constants.CheckRootCertificate(
                  platformService,
                  ICertificateManager.Constants.Instance);
            if (checkRootCertificateCode != StartProxyResultCode.Ok)
            {
                return Strings.Error_CheckRootCertificateFailed_;
            }
        }

        ReverseProxySettings reverseProxySettings = new(proxyDomains, scripts,
            isEnableScript, isOnlyWorkSteamBrowser, proxyPort,
            proxyIp, proxyMode, isProxyGOG, onlyEnableProxyScript,
            enableHttpProxyToHttps, socks5ProxyEnable, socks5ProxyPortId,
            twoLevelAgentEnable, twoLevelAgentProxyType, twoLevelAgentIp,
            twoLevelAgentPortId, twoLevelAgentUserName, twoLevelAgentPassword, proxyDNS, isSupportIpv6, proxyToken);
        byte[] reverseProxySettings_ = Serializable.SMP2(reverseProxySettings);
        var startProxyResult = await reverseProxyService.StartProxyAsync(reverseProxySettings_);
        if (startProxyResult)
        {
            switch (proxyMode)
            {
                case ProxyMode.System:
                case ProxyMode.PAC:
                    try
                    {
                        var configured = proxyMode == ProxyMode.System
                            ? await platformService.SetAsSystemProxyAsync(true, proxyIp_.Value, proxyPort)
                            : await platformService.SetAsSystemPACProxyAsync(true, $"http://{proxyIp_.Value}:{proxyPort}/pac");
                        if (!configured)
                        {
                            await reverseProxyService.StopProxyAsync();
                            return proxyMode == ProxyMode.System ? Strings.CommunityFix_SetAsSystemProxyFail : Strings.CommunityFix_SetAsSystemPACProxyFail;
                        }
                    }
                    catch
                    {
                        await reverseProxyService.StopProxyAsync();
                        throw;
                    }
                    break;
                case ProxyMode.Hosts: // 启动加速服务后修改 Hosts 文件
                    if (proxyDomains.Any_Nullable())
                    {
                        var localhost = IPAddress.Any.Equals(proxyIp_.Value) ?
                            IPAddress.Loopback.ToString() : proxyIp!;
                        var hosts = proxyDomains.SelectMany(s =>
                        {
                            if (s == null) return default!;

                            return s.ListeningDomainNamesArray.Select(host =>
                            {
                                if (host.Contains(' '))
                                {
                                    var h = host.Split(' ');
                                    return new KeyValuePair<string, string>(h[1], h[0]);
                                }
                                return new KeyValuePair<string, string>(host, localhost);
                            });
                        }).ToDictionaryIgnoreRepeat(x => x.Key, y => y.Value);
                        if (isEnableScript)
                        {
                            hosts.TryAdd(IReverseProxyService.Constants.LocalDomain, localhost);
                        }
                        var updateHostsResult = await hostsFileService.UpdateHosts(hosts);
                        if (updateHostsResult.ResultType != OperationResultType.Success)
                        {
                            if (OperatingSystem2.IsMacOS() || OperatingSystem2.IsLinux())
                            {
                                Browser2.Open(Constants.Urls.OfficialWebsite_UnixHostAccess);
                                //platformService.RunShell($" \\cp \"{Path.Combine(IOPath.CacheDirectory, "hosts")}\" \"{platformService.HostsFilePath}\"");
                            }
                            await reverseProxyService.StopProxyAsync();
                            return Strings.OperationHostsError_.Format(updateHostsResult.Message);
                        }
                    }
                    break;
            }
            _StartAccelerateTime = DateTimeOffset.Now;
            StartTimer();
            return default;
        }
        else
        {
            if (startProxyResult.Code == StartProxyResultCode.BindPortError)
            {
                if (!string.IsNullOrWhiteSpace(Plugin.Instance.SubProcessPath))
                {
                    Process.Start("pkexec", new string[] { "setcap", "cap_net_bind_service=+eip", Plugin.Instance.SubProcessPath }).WaitForExit();
                }
                //新线程等待 IPC 错误返回后 Kill 自己 Linux 修改权限需要重新启动
                reverseProxyService.Exit();
                return Strings.Error_BindPortError_.Format(proxyPort);
            }
            else
            {
                if (CertificateFailurePresenter.IsCertificateFailure(startProxyResult.Code))
                    return new OperateProxyServiceResult("Certificate operation failed.", CertificateError: startProxyResult.Code);
                var errorString = startProxyResult.Code switch
                {
                    StartProxyResultCode.Exception => startProxyResult.Exception?.ToString() ?? nameof(StartProxyResultCode.Exception),
                    _ => $"{Strings.Error_BindPortError_.Format(proxyPort)}StaartProxyErrorCode:${startProxyResult.Code}"
                };
                return errorString;
            }
        }
    }

    /// <summary>
    /// 停止代理服务
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    async Task<OperateProxyServiceResult> StopProxyServiceAsync(bool isExit = false)
    {
        try
        {
            var result = await StopProxyServiceCoreAsync(isExit);
            return result;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    async Task<OperateProxyServiceResult> StopProxyServiceCoreAsync(bool isExit)
    {
        bool callSet = true;
#if WINDOWS
        if (isExit && !platformService.IsAdministrator)
        {
            // Windows 平台非管理员权限进程退出时候忽略，由管理员进程退出时执行清空系统代理配置
            callSet = false;
        }
#endif
        switch (proxyMode) // 先停止接入代理流量
        {
#if REMOVE_DNS_INTERCEPT
            case ProxyMode.DNSIntercept:
#endif
            case ProxyMode.Hosts:
                var needClear = hostsFileService.ContainsHostsByTag();
                if (needClear)
                {
                    var removeHostsResult = await hostsFileService.RemoveHostsByTag();
                    if (removeHostsResult.ResultType != OperationResultType.Success)
                    {
                        // 退出时候不打开浏览器
                        if (!isExit && (OperatingSystem.IsMacOS() ||
                            (OperatingSystem.IsLinux() && !platformService.IsAdministrator)))
                        {
                            Browser2.Open(Constants.Urls.OfficialWebsite_UnixHostAccess);
                        }

                        // 还原 Hosts 失败阻止停止代理，因停止可能会导致写入部分 hosts 文件中的域名无法访问
                        if (!isExit)
                        {
                            return Strings.OperationHostsError_.Format(removeHostsResult.Message);
                        }
                    }
                }
                break;

            case ProxyMode.System:
                if (callSet)
                {
                    await platformService.SetAsSystemProxyAsync(false);
                }
                break;

            case ProxyMode.PAC:
                if (callSet)
                {
                    await platformService.SetAsSystemPACProxyAsync(false);
                }
                break;
#if WINDOWS && !REMOVE_DNS_INTERCEPT
            case ProxyMode.DNSIntercept:
                {
                    // 停止时也调用初始化
                    await Mobius.Helpers.WinDivertInitHelper.InitializeAsync();
                }
                break;
#endif
        }
        StopTimer(); // 停止 UI 计时器
#if MACOS
        reverseProxyService.StopProxyAsync().GetAwaiter(); // 停止代理服务
#else
        await reverseProxyService.StopProxyAsync(); // 停止代理服务
#endif
        return default;
    }

    /// <summary>
    /// 尝试获取服务端代理token
    /// </summary>
    /// <returns></returns>
    static async Task<string?> TryRequestServerSideProxyToken()
    {
        try
        {
            if (!string.IsNullOrEmpty(proxyTokenCache))
                return proxyTokenCache;

            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(4.99D)); // 5 秒超时
            var resp = await IMicroServiceClient.Instance.Account
                .GenerateServerSideProxyToken(cts.Token);

            if (resp != null && !string.IsNullOrEmpty(resp.Content?.AccessToken))
            {
                proxyTokenCache = resp.Content.AccessToken;
            }

            return proxyTokenCache;
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
        return null;
    }

    /// <summary>
    /// 操作代理服务返回结果
    /// </summary>
    /// <param name="Error"></param>
    readonly record struct OperateProxyServiceResult(string? Error, Exception? Exception = null, StartProxyResultCode? CertificateError = null)
    {
        public static implicit operator OperateProxyServiceResult(string error) => new(error);

        public static implicit operator OperateProxyServiceResult(Exception exception) => new(exception.Message, exception);

        /// <summary>
        /// 将返回结果使用 Toast 显示并且返回当前应设置的代理状态
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<bool> OnStartedShowToastReturnProxyStatus()
        {
            bool proxyStatus;
            if (string.IsNullOrWhiteSpace(Error))
            {
                Toast.Show(ToastIcon.Success, Strings.CommunityFix_StartProxySuccess);
                proxyStatus = true;
            }
            else if (CertificateError is { } certificateError)
            {
                await CertificateFailurePresenter.ShowAsync(certificateError);
                proxyStatus = false;
            }
            else if (Exception != null)
            {
                Exception.LogAndShowT(nameof(ProxyService));
                proxyStatus = false;
            }
            else
            {
                Toast.Show(ToastIcon.Error, Error);
                proxyStatus = false;
            }
            return proxyStatus;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool OnStopedShowToastReturnProxyStatus()
        {
            bool proxyStatus = default;
            if (!string.IsNullOrWhiteSpace(Error))
            {
                Toast.Show(ToastIcon.Error, Error);
                proxyStatus = true;
            }
            else if (Exception != null)
            {
                Exception.LogAndShowT(nameof(ProxyService));
                proxyStatus = true;
            }
            return proxyStatus;
        }
    }

    static async Task<bool> CheckDnsServersAsync(IEnumerable<string> servers, CancellationToken cancellationToken)
    {
        const string testDomain = "dnscheck-test.steampp.net";
        foreach (var server in servers)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            try
            {
                var (_, addresses) = server.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? await INetworkTestService.Instance.TestDNSOverHttpsAsync(testDomain, server, cancellationToken: cancellationToken)
                    : await INetworkTestService.Instance.TestDNSAsync(testDomain,
                        string.Equals(server, "System Default", StringComparison.OrdinalIgnoreCase) ? "System Default" : server,
                        53, cancellationToken: cancellationToken);
                if (addresses.Length > 0)
                    return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error(nameof(StartOrStopProxyService), ex.ToString(), "DNS检测出错");
            }
        }
        return false;
    }
}
