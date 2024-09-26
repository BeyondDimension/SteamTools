using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    [GeneratedRegex(DomainValidationAttribute.RegPattern)]
    private static partial Regex DomainRegExp();

    public class DomainValidationAttribute : RegularExpressionAttribute
    {
        public const string RegPattern = @"^[^/]+\.[^/]{2,}$";

        public override string FormatErrorMessage(string name) => "请填入不带分隔符 \"/\" 的域名";

        public DomainValidationAttribute() : base(RegPattern)
        {
        }
    }

    public enum NatTypeSimple
    {
        Unknown,
        Open,
        Moderate,
        Strict,
    }

    public override string Name => Strings.Welcome;

    public string DefaultTestDomain { get; } = "store.steampowered.com";

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
    public bool IsNATChecking { get; }

    [ObservableAsProperty]
    public bool IsDNSChecking { get; }

    [ObservableAsProperty]
    public bool IsIPv6Checking { get; }

    [DomainValidation]
    [Reactive]
    public string DomainPendingTest { get; set; } = string.Empty;

    [Reactive]
    public ReadOnlyCollection<ProxyDomainGroupViewModel>? EnableProxyDomainGroupVMs { get; set; }

    [Reactive]
    public string LocalEndPoint { get; set; } = string.Empty;

    [Reactive]
    public string PublicEndPoint { get; set; } = string.Empty;

    [Reactive]
    public string DNSTestDelay { get; set; } = string.Empty;

    [Reactive]
    public string DNSTestResult { get; set; } = string.Empty;

    [Reactive]
    public string IPv6Address { get; set; } = string.Empty;

    [Reactive]
    public bool IsSupportIPv6 { get; set; }

    public ReactiveCommand<Unit, (NatTypeSimple Nat, bool PingSuccess)> NATCheckCommand { get; }

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