using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using STUN.StunResult;
using System.Net.NetworkInformation;
using System.CommandLine.Parsing;
using STUN.Enums;
using System.Net.Http;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel
{
    DateTime _initializeTime;
    readonly IHostsFileService? hostsFileService;
    readonly IPlatformService platformService = IPlatformService.Instance;
    readonly IReverseProxyService reverseProxyService = IReverseProxyService.Constants.Instance;
    readonly ICertificateManager certificateManager = ICertificateManager.Constants.Instance;
    readonly INetworkTestService networkTestService = INetworkTestService.Instance;

    public AcceleratorPageViewModel()
    {
        StartProxyCommand = ReactiveCommand.CreateFromTask(async _ =>
        {
#if LINUX
            if (!EnvironmentCheck()) return;
#endif
            //ProxyService.Current.ProxyStatus = !ProxyService.Current.ProxyStatus;
            await ProxyService.Current.StartOrStopProxyService(!ProxyService.Current.ProxyStatus);

            if (ProxyService.Current.ProxyStatus == false)
                return;

            // Create new ProxyEnableDomain for 加速服务 page
            var enableGroupDomain = ProxyService.Current.ProxyDomainsList
                .Where(list => list.ThreeStateEnable == true || list.ThreeStateEnable == null)
                .Select(list => new ProxyDomainGroupViewModel
                {
                    Name = list.Name,
                    IconUrl = list.IconUrl ?? string.Empty,
                    EnableProxyDomainVMs = new(
                        list.Items!
                            .Where(i => i.ThreeStateEnable == true)
                            .Select(i => new ProxyDomainViewModel(i.Name, i.ProxyType, "https://" + i.ListenDomainNames.Split(";")[0]))
                            .ToList()),
                })
                .ToList();

            EnableProxyDomainGroupVMs = new(enableGroupDomain);
        });

        RefreshCommand = ReactiveCommand.Create(async () =>
        {
            if (_initializeTime > DateTime.Now.AddSeconds(-2))
            {
                Toast.Show(ToastIcon.Warning, Strings.Warning_DoNotOperateFrequently);

                return;
            }

            _initializeTime = DateTime.Now;

            if (ProxyService.Current.ProxyStatus == false)
                await ProxyService.Current.InitializeAccelerateAsync();
            else
                Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseStopAccelerate);

            // 刷新时重新加载迅游游戏数据
            GameAcceleratorService.Current.LoadGames();
        });

        SelectedSTUNAddress = STUNAddress[0];

        NATCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var natCheckResult = await networkTestService.TestStunClient3489Async(testServerHostName: SelectedSTUNAddress) ?? new ClassicStunResult { NatType = NatType.Unknown };
            var (netSucc, _) = await networkTestService.TestOpenUrlAsync("https://www.baidu.com");

            var natStatus = natCheckResult.NatType switch
            {
                NatType.OpenInternet or NatType.FullCone => NatTypeSimple.Open,
                NatType.RestrictedCone or NatType.PortRestrictedCone or NatType.SymmetricUdpFirewall => NatTypeSimple.Moderate,
                NatType.Symmetric or NatType.UdpBlocked => NatTypeSimple.Strict,
                NatType.Unknown or NatType.UnsupportedServer or _ => NatTypeSimple.Unknown,
            };

            PublicEndPoint = natCheckResult.PublicEndPoint?.Address.ToString() ?? "Unknown";
            LocalEndPoint = natCheckResult.LocalEndPoint?.Address.ToString() ?? "Unknown";

            return (natStatus, netSucc);
        });
        NATCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsNATChecking);

        static string[] GetLocalDnsServers()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var dnsAddresses = ipProperties.DnsAddresses;

                if (dnsAddresses.Count > 0)
                {
                    return dnsAddresses.Select(dns => dns.ToString()).ToArray();
                }
            }
            return [];
        }

        static (string, string) ExtractIPAndDNS(string content)
        {
            // 正则表达式匹配 IP 和 DNS 信息
            var ipMatch = IpAddr().Match(content);
            var dnsMatch = DnsAddr().Match(content);

            if (ipMatch.Success && dnsMatch.Success)
            {
                string ipAddress = ipMatch.Groups[1].Value.Trim();
                string dnsAddress = dnsMatch.Groups[1].Value.Trim();
                return (ipAddress, dnsAddress);
            }
            return ("Unknown", "Unknown");
        }
        DNSCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var testDomain = DomainPendingTest == string.Empty ? "steamcommunity.com" : DomainPendingTest;
            try
            {
                long delayMs;
                IPAddress[] address;
                if (ProxySettings.UseDoh2)
                {
                    var configDoh = ProxySettings.CustomDohAddres.Value ?? string.Empty;
                    (delayMs, address) = await networkTestService.TestDNSOverHttpsAsync(testDomain, configDoh);
                }
                else
                {
                    var configDns = ProxySettings.ProxyMasterDns.Value ?? string.Empty;
                    (delayMs, address) = await networkTestService.TestDNSAsync(testDomain, configDns, 53);
                }
                DNSTestDelay = delayMs + "ms ";
                DNSTestResult = "" + address.FirstOrDefault() ?? "0.0.0.0";
            }
            catch (Exception ex)
            {
                Log.Error(nameof(AcceleratorPageViewModel), ex.ToString());
                DNSTestDelay = string.Empty;
                DNSTestResult = "error";
            }

            //string url = "https://nstool.netease.com/";
            //try
            //{
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(url);
            //    response.EnsureSuccessStatusCode();

            //    string htmlContent = await response.Content.ReadAsStringAsync();
            //    var match = PageFrameSrcUrl().Match(htmlContent);
            //    if (!match.Success)
            //        throw new HttpRequestException("Found src url failed");
            //    var src = match.Groups["src"].Value;

            //    var res = await client.GetAsync(src);
            //    var content = await res.Content.ReadAsStringAsync();

            //    var extractedData = ExtractIPAndDNS(content);

            //    PublicIPAddress = extractedData.Item1;
            //    PublicDNSAddress = extractedData.Item2;
            //}
            //catch (HttpRequestException ex)
            //{
            //    Log.Error(nameof(AcceleratorPageViewModel), ex, "Request error");
            //    PublicIPAddress = PublicDNSAddress = "Unknown";
            //}

            //var dnsServers = GetLocalDnsServers();
            //if (dnsServers != null && dnsServers.Length != 0)
            //{
            //    LocalDNSAddress = dnsServers[0];
            //}
        });
        DNSCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsDNSChecking);

        ProxySettingsCommand = ReactiveCommand.Create(() =>
        {
            var vm = new ProxySettingsWindowViewModel();
            _ = IWindowManager.Instance.ShowTaskDialogAsync(vm, vm.Title, pageContent: new ProxySettingsPage(), isOkButton: false);
        });

        if (IApplication.IsDesktop())
        {
            hostsFileService = IHostsFileService.Constants.Instance;
            SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
            DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
            EditHostsFileCommand = ReactiveCommand.Create(hostsFileService.OpenFile);
            OpenHostsDirCommand = ReactiveCommand.Create(hostsFileService.OpenFileDir);
            ResetHostsFileCommand = ReactiveCommand.CreateFromTask(hostsFileService.ResetFile);
            NetworkFixCommand = ReactiveCommand.Create(ProxyService.Current.FixNetwork);
            TrustCerCommand = ReactiveCommand.Create(TrustCer_OnClick);
            ShowCertificateCommand = ReactiveCommand.Create(ShowCertificate_OnClick);
            OpenCertificateDirCommand = ReactiveCommand.Create(() =>
            {
                certificateManager.GetCerFilePathGeneratedWhenNoFileExists();
                platformService.OpenFolder(certificateManager.PfxFilePath);
            });
        }
    }

#if LINUX

    public bool EnvironmentCheck()
    {
        try
        {
            var path = Path.Combine(IOPath.BaseDirectory!, "script", "environment_check.sh");
            var shellStr = $"if [ -x \"{path}\" ]; then '{path}' -c; else chmod +x '{path}'; '{path}' -c; fi";
            var p = Process.Start(Process2.BinBash, new string[] { "-c", shellStr });
            p.WaitForExit();
            if (p.ExitCode == 200)
                return true;
            platformService.OpenFolder(Path.Combine(IOPath.BaseDirectory, "script"));
            MessageBox.Show($"请在终端手动运行:environment_check.sh 安装依赖环境");
            return false;
        }
        catch (Exception e)
        {
            MessageBox.Show($"环境检查错误:{e}");
            return false;
        }
    }

#endif

    public void TrustCer_OnClick()
    {
        certificateManager.TrustRootCertificate();
    }

    public void SetupCertificate_OnClick()
    {
#if LINUX
        if (!EnvironmentCheck()) return;
#endif
        var r = certificateManager.SetupRootCertificate();
        if (r)
        {
            Toast.Show(ToastIcon.Success, Strings.CommunityFix_SetupCertificate_Success);
        }
        else
        {
            Toast.Show(ToastIcon.Error, Strings.CommunityFix_SetupCertificate_Fail);
        }
    }

    public void DeleteCertificate_OnClick()
    {
#if LINUX
        if (!EnvironmentCheck()) return;
#endif
        var r = certificateManager.DeleteRootCertificate();
        if (r)
        {
            Toast.Show(ToastIcon.Success, Strings.CommunityFix_DeleteCertificate_Success);
        }
        else
        {
            Toast.Show(ToastIcon.Error, Strings.CommunityFix_DeleteCertificate_Fail);
        }
    }

    void ShowCertificate_OnClick()
    {
        var certInfo = certificateManager.GetCertificateInfo();
        MessageBox.Show(certInfo, "证书信息");
    }

    [GeneratedRegex(@"<iframe[^>]+src\s*=\s*['""](?<src>[^'""]+)['""]", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex PageFrameSrcUrl();

    [GeneratedRegex(@"您的IP地址信息:\s*([^<]+)")]
    private static partial Regex IpAddr();

    [GeneratedRegex(@"您的DNS地址信息:\s*([^<]+)")]
    private static partial Regex DnsAddr();
}