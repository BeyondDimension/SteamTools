using Avalonia.Media;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public class ProxyDomainViewModel : ReactiveObject
{
    private readonly INetworkTestService networkTestService = INetworkTestService.Instance;

    public string Name { get; }

    public ProxyType ProxyType { get; }

    public string Url { get; }

    [Reactive]
    public string DelayMillseconds { get; set; } = string.Empty;

    [ObservableAsProperty]
    public bool RetestButtonVisible { get; }

    [ObservableAsProperty]
    public IBrush DelayColor { get; } = null!;

    public ReactiveCommand<Unit, Unit> RetestCommand { get; set; }

    public ProxyDomainViewModel(string name, ProxyType proxyType, string url)
    {
        Name = name;
        ProxyType = proxyType;
        Url = url;

        RetestCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            DelayMillseconds = "-";
            var (success, delayMs) = await networkTestService.TestOpenUrlAsync(Url);
            DelayMillseconds = success ? delayMs.ToString()! + " ms" : "error";
        });

        this.WhenAnyValue(x => x.DelayMillseconds)
            .Select(d => d != string.Empty && d != "-")
            .ToPropertyEx(this, x => x.RetestButtonVisible);

        const int DelayMiddle = 1000;
        this.WhenAnyValue(x => x.DelayMillseconds)
            .Select(d => d switch
            {
                "Timeout" or "error" => Brushes.Red,
                var s when s.Split(' ') is [var num, "ms"] && int.TryParse(num, out int ms)
                   => ms switch
                   {
                       <= DelayMiddle => Brushes.Green,    // 0-700 ms range
                       > DelayMiddle => Brushes.Orange,    // Above 700 ms
                   },
                _ => Brushes.Black,
            })
            .ToPropertyEx(this, x => x.DelayColor);
    }
}

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    public enum NatTypeSimple
    {
        Unknown,
        Open,
        Moderate,
        Strict,
    }

    public override string Name => Strings.Welcome;

    [ObservableAsProperty]
    public bool IsNATChecking { get; }

    [ObservableAsProperty]
    public bool IsDNSChecking { get; }

    [Reactive]
    public ReadOnlyCollection<ProxyDomainViewModel>? EnableProxyDomainVMs { get; set; }

    [Reactive]
    public string LocalEndPoint { get; set; } = string.Empty;

    [Reactive]
    public string PublicEndPoint { get; set; } = string.Empty;

    [Reactive]
    public string PublicIPAddress { get; set; } = string.Empty;

    [Reactive]
    public string PublicDNSAddress { get; set; } = string.Empty;

    [Reactive]
    public string LocalDNSAddress { get; set; } = string.Empty;

    public ReactiveCommand<Unit, (NatTypeSimple Nat, bool PingSuccess)> NATCheckCommand { get; }

    public ReactiveCommand<Unit, Unit> DNSCheckCommand { get; }

    public ReactiveCommand<Unit, Unit> ConnectTestCommand { get; }

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
}