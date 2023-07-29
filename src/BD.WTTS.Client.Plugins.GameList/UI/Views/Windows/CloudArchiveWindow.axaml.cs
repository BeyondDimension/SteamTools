using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Windows;

public partial class CloudArchiveWindow : ReactiveAppWindow<CloudArchiveAppPageViewModel>
{
    public CloudArchiveWindow()
    {
        InitializeComponent();
    }

    public CloudArchiveWindow(int appid) : this()
    {
        DataContext ??= new CloudArchiveAppPageViewModel(appid);
    }
}
