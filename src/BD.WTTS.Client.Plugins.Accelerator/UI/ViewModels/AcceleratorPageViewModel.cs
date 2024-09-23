using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.WTTS.UI.Views.Pages;
using STUN.StunResult;
using STUN.Enums;

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
        ProxyService.Current.WhenValueChanged(x => x.ProxyStatus)
            .Where(x => x == true)
            .Subscribe(_ =>
            {
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
                                .Select(i => new ProxyDomainViewModel(i.Name, i.ProxyType, "https://" + i.ListenDomainNames.Split(";")[0],
                                                                    i.Items?
                                                                        .Select(c => new ProxyDomainViewModel(c.Name, c.ProxyType, "https://" + c.ListenDomainNames.Split(';')[0]))
                                                                        .ToList()))
                                .ToList()),
                    })
                    .ToList();

                EnableProxyDomainGroupVMs = enableGroupDomain.AsReadOnly();
            });

        StartProxyCommand = ReactiveCommand.CreateFromTask(async _ =>
        {
#if LINUX
            if (!EnvironmentCheck()) return;
#endif
            //ProxyService.Current.ProxyStatus = !ProxyService.Current.ProxyStatus;
            await ProxyService.Current.StartOrStopProxyService(!ProxyService.Current.ProxyStatus);
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

        DNSCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var testDomain = DomainPendingTest == string.Empty ? "store.steampowered.com" : DomainPendingTest;
            try
            {
                long delayMs;
                IPAddress[] address;
                if (ProxySettings.UseDoh)
                {
                    var configDoh = ProxySettings.CustomDohAddres2.Value ?? ProxySettingsWindowViewModel.DohAddress.FirstOrDefault() ?? string.Empty;
                    (delayMs, address) = await networkTestService.TestDNSOverHttpsAsync(testDomain, configDoh);
                }
                else
                {
                    var configDns = ProxySettings.ProxyMasterDns.Value ?? string.Empty;
                    (delayMs, address) = await networkTestService.TestDNSAsync(testDomain, configDns, 53);
                }
                if (address.Length == 0)
                    throw new Exception("Parsing failed. Return empty ip address.");

                DNSTestDelay = delayMs + "ms ";
                DNSTestResult = string.Empty + address.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(AcceleratorPageViewModel), ex.ToString());
                DNSTestDelay = string.Empty;
                DNSTestResult = "error";
            }
        });
        DNSCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsDNSChecking);

        IPv6CheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var result = await IMicroServiceClient.Instance.Accelerate.GetMyIP(ipV6: true);
            if (result.IsSuccess)
            {
                IsSupportIPv6 = true;
                IPv6Address = result.Content ?? string.Empty;
            }
            else
            {
                IsSupportIPv6 = false;
                IPv6Address = string.Empty;
            }
        });
        IPv6CheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsIPv6Checking);

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
            OpenLogFileCommand = ReactiveCommand.Create(() =>
            {
                platformService.OpenFolder(IPCSubProcessFileSystem.GetLogDirPath(Plugin.Instance.UniqueEnglishName));
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
}