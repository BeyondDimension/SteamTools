using BD.WTTS.Client.Resources;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    public enum NATType
    {
        Unknown,
        Open,
        Moderate,
        Strict,
    }

    public override string Name => Strings.Welcome;

    [ObservableAsProperty]
    public bool IsNATChecking { get; }

    public ICommand StartProxyCommand { get; }

    public ICommand RefreshCommand { get; }

    public ReactiveCommand<Unit, (NATType Nat, bool PingSuccess)> NATCheckCommand { get; }

    public ReactiveCommand<Unit, Unit> ConnectTestCommand { get; }

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
