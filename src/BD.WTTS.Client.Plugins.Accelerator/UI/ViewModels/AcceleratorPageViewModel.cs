using AppResources = BD.WTTS.Client.Resources.Strings;

using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel
{
    readonly IHostsFileService? hostsFileService;
    readonly IPlatformService platformService = IPlatformService.Instance;
    readonly IReverseProxyService reverseProxyService = IReverseProxyService.Constants.Instance;
    readonly ICertificateManager certificateManager = ICertificateManager.Constants.Instance;

    public AcceleratorPageViewModel()
    {
        StartProxyCommand = ReactiveCommand.Create(() =>
        {
            ProxyService.Current.ProxyStatus = !ProxyService.Current.ProxyStatus;
        });

        RefreshCommand = ReactiveCommand.Create(async () =>
        {
            if (ProxyService.Current.ProxyStatus == false)
                await ProxyService.Current.InitializeAccelerateAsync();
            else
                Toast.Show(ToastIcon.Warning, AppResources.Warning_PleaseStopAccelerate);
        });

        ProxySettingsCommand = ReactiveCommand.Create(() =>
        {
            var vm = new ProxySettingsWindowViewModel();
            IWindowManager.Instance.ShowTaskDialogAsync(vm, vm.Title, pageContent: new ProxySettingsPage(), isOkButton: false);
        });

        if (IApplication.IsDesktop())
        {
            hostsFileService = IHostsFileService.Instance;
            SetupCertificateCommand = ReactiveCommand.Create(SetupCertificate_OnClick);
            DeleteCertificateCommand = ReactiveCommand.Create(DeleteCertificate_OnClick);
            EditHostsFileCommand = ReactiveCommand.Create(hostsFileService.OpenFile);
            OpenHostsDirCommand = ReactiveCommand.Create(hostsFileService.OpenFileDir);
            ResetHostsFileCommand = ReactiveCommand.Create(hostsFileService.ResetFile);
            NetworkFixCommand = ReactiveCommand.Create(ProxyService.Current.FixNetwork);
            TrustCerCommand = ReactiveCommand.Create(TrustCer_OnClick);
            OpenCertificateDirCommand = ReactiveCommand.Create(() =>
            {
                certificateManager.GetCerFilePathGeneratedWhenNoFileExists();
                platformService.OpenFolder(certificateManager.PfxFilePath);
            });
        }
    }

    public async void TrustCer_OnClick()
    {
        certificateManager.GetCerFilePathGeneratedWhenNoFileExists();
        await certificateManager.PlatformTrustRootCertificateGuideAsync();
    }

    public async void SetupCertificate_OnClick()
    {
        await certificateManager.SetupRootCertificateAsync();
    }

    public bool DeleteCertificate_OnClick()
    {
        return certificateManager.DeleteRootCertificate();
    }
}