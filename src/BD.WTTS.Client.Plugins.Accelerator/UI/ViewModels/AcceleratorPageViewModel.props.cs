using BD.WTTS.Client.Resources;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    public ICommand StartProxyCommand { get; }

    public ICommand RefreshCommand { get; }

    public ReactiveCommand<Unit, Unit>? SetupCertificateCommand { get; }

    public ReactiveCommand<Unit, Unit>? DeleteCertificateCommand { get; }

    public ReactiveCommand<Unit, Unit>? OpenCertificateDirCommand { get; }

    public ReactiveCommand<Unit, Unit>? EditHostsFileCommand { get; }

    public ReactiveCommand<Unit, Unit>? OpenHostsDirCommand { get; }

    public ReactiveCommand<Unit, Unit>? ResetHostsFileCommand { get; }

    public ReactiveCommand<Unit, Unit>? NetworkFixCommand { get; }

    public ReactiveCommand<Unit, Unit>? ProxySettingsCommand { get; }

    public ReactiveCommand<Unit, Unit>? TrustCerCommand { get; }
}
