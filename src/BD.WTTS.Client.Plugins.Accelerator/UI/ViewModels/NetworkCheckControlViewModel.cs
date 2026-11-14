using STUN.Enums;
using STUN.StunResult;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public partial class NetworkCheckControlViewModel : ViewModelBase
{
    [GeneratedRegex(DomainValidationAttribute.RegPattern)]
    private static partial Regex DomainRegExp();

    public class DomainValidationAttribute : RegularExpressionAttribute
    {
        public const string RegPattern = @"^[^/]+\.[^/]{2,}$";

        public override string FormatErrorMessage(string name) => Strings.L10nAccel_148;

        public DomainValidationAttribute() : base(RegPattern)
        {
        }
    }

    private readonly INetworkTestService _networkTestService = INetworkTestService.Instance;

    public record NATFetchResult(string PublicEndPoint, string LocalEndPoint, string NATLevel, string NATTypeTip, bool PingResult);

    public string DefaultTestDomain { get; } = "store.steampowered.com";

    [Reactive]
    public string SelectedSTUNAddress { get; set; }

    public string[] STUNAddress { get; } =
    [
        NetworkTestService.DEFAULT_TESTSERVER_HOSTNAME,
        "stun.hot-chilli.net",
        "stun.fitauto.ru",
    ];

    [Reactive]
    public string NATLevel { get; set; } = Strings.L10nAccel_131;

    [Reactive]
    public string NATTypeTip { get; set; } = Strings.L10nAccel_132;

    [ObservableAsProperty]
    public string LocalEndPoint { get; } = string.Empty;

    [ObservableAsProperty]
    public string PublicEndPoint { get; } = string.Empty;

    [ObservableAsProperty]
    public bool PingOkVisible { get; }

    [ObservableAsProperty]
    public bool PingErrorVisible { get; }

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

    public NetworkCheckControlViewModel()
    {
        SelectedSTUNAddress = STUNAddress[0];

        NATCheckCommand = ReactiveCommand.CreateFromTask(NATCheckAsync);
        NATCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsNATChecking);
        NATCheckCommand
            .Select(x => x.PublicEndPoint)
            .ToPropertyEx(this, x => x.PublicEndPoint);
        NATCheckCommand
            .Select(x => x.LocalEndPoint)
            .ToPropertyEx(this, x => x.LocalEndPoint);
        NATCheckCommand.Subscribe(x =>
        {
            NATLevel = x.NATLevel;
            NATTypeTip = x.NATTypeTip;
        });

        var hidePingResultStream = NATCheckCommand
            .IsExecuting
            .Where(x => x == true)
            .Select(x => false);
        NATCheckCommand
            .Select(x => x.PingResult == true)
            .Merge(hidePingResultStream)
            .ToPropertyEx(this, x => x.PingOkVisible);
        NATCheckCommand
            .Select(x => x.PingResult == false)
            .Merge(hidePingResultStream)
            .ToPropertyEx(this, x => x.PingErrorVisible);

        var canDNSCheck = this.WhenAnyValue(x => x.DomainPendingTest)
            .Select(domain => domain == string.Empty || DomainRegExp().IsMatch(domain));
        DNSCheckCommand = ReactiveCommand.CreateFromTask(DNSCheckAsync, canDNSCheck);
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

        IPv6CheckCommand.Execute().Subscribe();
    }

    async Task<NATFetchResult> NATCheckAsync()
    {
        try
        {
            return await NATCheckCoreAsync();
        }
        catch
        {
            return new("", "", "", "", false);
        }
    }

    async Task<NATFetchResult> NATCheckCoreAsync()
    {
        var natCheckResult = await _networkTestService.TestStunClient3489Async(testServerHostName: SelectedSTUNAddress) ?? new ClassicStunResult { NatType = NatType.Unknown };
        var (netSucc, _) = await _networkTestService.TestOpenUrlAsync("https://www.baidu.com");

        var publicEndPoint = natCheckResult.PublicEndPoint?.Address.ToString() ?? "Unknown";
        var localEndPoint = natCheckResult.LocalEndPoint?.Address.ToString() ?? "Unknown";

        var (natLevel, natTypeTip) = GetNatText(natCheckResult.NatType);
        return new NATFetchResult(publicEndPoint, localEndPoint, natLevel, natTypeTip, netSucc);
    }

    static (string, string) GetNatText(NatType natType) => natType switch
        {
            // Open
            NatType.OpenInternet or NatType.FullCone => (Strings.L10nAccel_133, Strings.L10nAccel_134),
            // Moderate
            NatType.RestrictedCone or NatType.PortRestrictedCone or NatType.SymmetricUdpFirewall => (Strings.L10nAccel_135, Strings.L10nAccel_136),
            // Strict
            NatType.Symmetric or NatType.UdpBlocked => (Strings.L10nAccel_137, Strings.L10nAccel_138),
            // Unknown
            NatType.Unknown or NatType.UnsupportedServer or _ => (Strings.L10nAccel_139, Strings.L10nAccel_140),
        };

    async Task DNSCheckAsync()
    {
        var testDomain = DomainPendingTest == string.Empty ? DefaultTestDomain : DomainPendingTest;
        ProxySettings.MigrateDnsServerList();
        var configured = ProxySettings.ProxyMasterDns.Value;
        var servers = (configured ?? string.Empty).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()).Where(x => x.Length > 0).ToArray();
        if (servers.Length == 0)
            servers = IDnsAnalysisService.DefaultResolverServerList
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        DNSTestDelay = string.Empty;
        DNSTestResult = string.Empty;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var hadTimeout = false;
        foreach (var server in servers)
        {
            try
            {
                var (delayMs, address) = server.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                    ? await _networkTestService.TestDNSOverHttpsAsync(testDomain, server, cancellationToken: timeout.Token)
                    : await _networkTestService.TestDNSAsync(testDomain,
                        string.Equals(server, "System Default", StringComparison.OrdinalIgnoreCase) ? "System Default" : server,
                        53, cancellationToken: timeout.Token);
                if (address.Length > 0)
                {
                    DNSTestDelay = delayMs + "ms ";
                    DNSTestResult = string.Format(Strings.L10nAccel_141, address[0], server);
                    return;
                }
            }
            catch (OperationCanceledException) when (timeout.IsCancellationRequested)
            {
                DNSTestResult = string.Format(Strings.L10nAccel_142, server);
                return;
            }
            catch (OperationCanceledException)
            {
                hadTimeout = true;
            }
            catch (Exception ex)
            {
                Log.Error(nameof(AcceleratorPageViewModel), ex.ToString());
            }
        }
        DNSTestResult = hadTimeout ? Strings.L10nAccel_143 : Strings.L10nAccel_144;
    }
}