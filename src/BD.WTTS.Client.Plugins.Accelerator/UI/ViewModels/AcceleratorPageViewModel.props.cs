using BD.WTTS.Client.Resources;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

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
