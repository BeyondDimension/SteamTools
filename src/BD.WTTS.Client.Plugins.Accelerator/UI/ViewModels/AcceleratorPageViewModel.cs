using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AcceleratorPageViewModel
{
    public AcceleratorPageViewModel()
    {
        StartProxyCommand = ReactiveCommand.Create(() =>
        {
            ProxyService.Current.ProxyStatus = !ProxyService.Current.ProxyStatus;
            Toast.Show(ProxyService.Current.ProxyStatus ? "加速已启动" : "加速已停止");
        });

        RefreshCommand = ReactiveCommand.Create(async () =>
        {
            if (ProxyService.Current.ProxyStatus == false)
                await ProxyService.Current.InitializeAccelerateAsync();
            else
                Toast.Show("请先停止加速");
        });
    }
}