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

        public override string FormatErrorMessage(string name) => "请填入不带分隔符 \"/\" 的域名";

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

        NATCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var natCheckResult = await _networkTestService.TestStunClient3489Async(testServerHostName: SelectedSTUNAddress) ?? new ClassicStunResult { NatType = NatType.Unknown };
            var (netSucc, _) = await _networkTestService.TestOpenUrlAsync("https://www.baidu.com");

            var publicEndPoint = natCheckResult.PublicEndPoint?.Address.ToString() ?? "Unknown";
            var localEndPoint = natCheckResult.LocalEndPoint?.Address.ToString() ?? "Unknown";

            var (natLevel, natTypeTip) = natCheckResult.NatType switch
            {
                // Open
                NatType.OpenInternet or NatType.FullCone => ("开放 NAT", "您可与在其网络上具有任意 NAT 类型的用户玩多人游戏和发起多人游戏。"),
                // Moderate
                NatType.RestrictedCone or NatType.PortRestrictedCone or NatType.SymmetricUdpFirewall => ("中等 NAT", "您可与一些用户玩多人游戏；但是，并且通常你将不会被选为比赛的主持人。"),
                // Strict
                NatType.Symmetric or NatType.UdpBlocked => ("严格 NAT", "您只能与具有开放 NAT 类型的用户玩多人游戏。您不能被选为比赛的主持人。"),
                // Unknown
                NatType.Unknown or NatType.UnsupportedServer or _ => ("不可用 NAT", "如果 NAT 不可用，您将无法使用群聊天或连接到某些 Xbox 游戏的多人游戏。"),
            };

            return new NATFetchResult(publicEndPoint, localEndPoint, natLevel, natTypeTip, netSucc);
        });
        NATCheckCommand
            .IsExecuting
            .ToPropertyEx(this, x => x.IsNATChecking);
        NATCheckCommand
            .Select(x => x.PublicEndPoint)
            .ToPropertyEx(this, x => x.PublicEndPoint);
        NATCheckCommand
            .Select(x => x.LocalEndPoint)
            .ToPropertyEx(this, x => x.LocalEndPoint);
        NATCheckCommand
            .Select(x => x.NATLevel)
            .Merge(Observable.Return("未知"))
            .ToPropertyEx(this, x => x.NATLevel);
        NATCheckCommand
            .Select(x => x.NATTypeTip)
            .Merge(Observable.Return("未知类型"))
            .ToPropertyEx(this, x => x.NATTypeTip);

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
        DNSCheckCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var testDomain = DomainPendingTest == string.Empty ? DefaultTestDomain : DomainPendingTest;
            try
            {
                long delayMs;
                IPAddress[] address;
                if (ProxySettings.UseDoh)
                {
                    var configDoh = ProxySettings.CustomDohAddres2.Value ?? ProxySettingsWindowViewModel.DohAddress.FirstOrDefault() ?? string.Empty;
                    (delayMs, address) = await _networkTestService.TestDNSOverHttpsAsync(testDomain, configDoh);
                }
                else
                {
                    var configDns = ProxySettings.ProxyMasterDns.Value ?? string.Empty;
                    (delayMs, address) = await _networkTestService.TestDNSAsync(testDomain, configDns, 53);
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
        }, canDNSCheck);
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
}