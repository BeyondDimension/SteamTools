using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace BD.WTTS.UI.Views.Controls;

public partial class DnsListManager : UserControl
{
    public DnsListManager()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.UIThread.Post(() => ServerAddress.Focus(), DispatcherPriority.Background);
    }
}
