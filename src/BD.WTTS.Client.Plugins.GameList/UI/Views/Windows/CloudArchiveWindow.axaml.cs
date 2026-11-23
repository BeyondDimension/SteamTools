using Avalonia.Controls;
using ReactiveUI.Avalonia.Reactive;

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
