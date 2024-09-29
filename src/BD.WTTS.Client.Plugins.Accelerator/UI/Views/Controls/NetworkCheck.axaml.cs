using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class NetworkCheck : UserControl, IViewFor<AcceleratorPageViewModel>
{
    public NetworkCheck()
    {
        InitializeComponent();

        Loaded += (_, _) =>
        {
            ViewModel = DataContext as AcceleratorPageViewModel;

            this.OneWayBind(ViewModel, vm => vm.PingResultStatus, v => v.PingOK.IsVisible, result => result == AcceleratorPageViewModel.PingStatus.Ok);
            this.OneWayBind(ViewModel, vm => vm.PingResultStatus, v => v.PingError.IsVisible, result => result == AcceleratorPageViewModel.PingStatus.Error);
            this.OneWayBind(ViewModel, vm => vm.NATLevel, v => v.NATTextBlock.Text);
            this.OneWayBind(ViewModel, vm => vm.NATTypeTip, v => v.NATTypeTip.Text);
        };
    }

    public AcceleratorPageViewModel? ViewModel { get; set; }

    object? IViewFor.ViewModel { get; set; }
}