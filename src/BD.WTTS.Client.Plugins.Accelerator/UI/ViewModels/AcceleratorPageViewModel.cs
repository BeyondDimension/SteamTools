using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using STUN.StunResult;
using System.Net.NetworkInformation;

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

        // https://support.xbox.com/zh-CN/help/hardware-network/connect-network/xbox-one-nat-error
        NATCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var natCheckResult = await networkTestService.TestStunClient3489Async() ?? new ClassicStunResult { NatType = STUN.Enums.NatType.Unknown };
            var (netSucc, _) = await networkTestService.TestOpenUrlAsync("https://www.baidu.com");

            var natStatus = natCheckResult.NatType switch
            {
                STUN.Enums.NatType.OpenInternet or STUN.Enums.NatType.FullCone => NATType.Open,
                STUN.Enums.NatType.RestrictedCone or STUN.Enums.NatType.PortRestrictedCone or STUN.Enums.NatType.SymmetricUdpFirewall => NATType.Moderate,
                STUN.Enums.NatType.Symmetric or STUN.Enums.NatType.UdpBlocked => NATType.Strict,
                STUN.Enums.NatType.Unknown or STUN.Enums.NatType.UnsupportedServer or _ => NATType.Unknown,
            };

            return (natStatus, netSucc);
        });
        NATCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsNATChecking);

        ConnectTestCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await Task.Delay(200);
            var enableDomains = ProxyService.Current.GetEnableProxyDomains();
            foreach (var enableDomain in enableDomains)
            {
                var firstMatchDomain = enableDomain.MatchDomainNames.Split(';')[0];
                var shouldBeSame = enableDomain.DomainNamesArray[0];
                Log.Info(shouldBeSame, firstMatchDomain);
            }
        });

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
}