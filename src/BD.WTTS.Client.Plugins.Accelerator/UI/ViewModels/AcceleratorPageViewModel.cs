using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel
{
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
                Toast.Show(ToastIcon.Warning, "请先停止加速");
        });
    }
}