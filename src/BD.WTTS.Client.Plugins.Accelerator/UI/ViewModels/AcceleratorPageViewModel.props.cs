using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    public record NATFetchResult(string PublicEndPoint, string LocalEndPoint, string NATLevel, string NATTypeTip, bool PingResult);

    public enum PingStatus
    {
        Blank,
        Ok,
        Error,
    }

    public override string Name => Strings.Welcome;

    [Reactive]
    public string SelectedSTUNAddress { get; set; }

    public string[] STUNAddress { get; } =
    [
        "stun.syncthing.net",
        "stun.hot-chilli.net",
        "stun.fitauto.ru",
        "stun.miwifi.com",
    ];

    [ObservableAsProperty]
    public string NATLevel { get; } = string.Empty;

    [ObservableAsProperty]
    public string NATTypeTip { get; } = string.Empty;

    [ObservableAsProperty]
    public string LocalEndPoint { get; set; } = string.Empty;

    [ObservableAsProperty]
    public string PublicEndPoint { get; set; } = string.Empty;

    [ObservableAsProperty]
    public PingStatus PingResultStatus { get; }

    [ObservableAsProperty]
    public bool IsNATChecking { get; }

    [ObservableAsProperty]
    public bool IsDNSChecking { get; }

    [ObservableAsProperty]
    public bool IsIPv6Checking { get; }

    [Reactive]
    public string DomainPendingTest { get; set; } = string.Empty;

    [Reactive]
    public ReadOnlyCollection<ProxyDomainGroupViewModel>? EnableProxyDomainGroupVMs { get; set; }

    [Reactive]
    public string DNSTestDelay { get; set; } = string.Empty;

    [Reactive]
    public string DNSTestResult { get; set; } = string.Empty;

    [Reactive]
    public string IPv6Address { get; set; } = string.Empty;

    [Reactive]
    public bool IsSupportIPv6 { get; set; }

    public ReactiveCommand<Unit, NATFetchResult> NATCheckCommand { get; }

    public ReactiveCommand<Unit, Unit> DNSCheckCommand { get; }

    public ReactiveCommand<Unit, Unit> IPv6CheckCommand { get; }

    public ICommand StartProxyCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand? SetupCertificateCommand { get; }

    public ICommand? DeleteCertificateCommand { get; }

    public ICommand? ShowCertificateCommand { get; }

    public ICommand? OpenCertificateDirCommand { get; }

    public ICommand? EditHostsFileCommand { get; }

    public ICommand? OpenHostsDirCommand { get; }

    public ICommand? ResetHostsFileCommand { get; }

    public ICommand? NetworkFixCommand { get; }

    public ICommand? ProxySettingsCommand { get; }

    public ICommand? TrustCerCommand { get; }

    public ICommand? OpenLogFileCommand { get; }
}