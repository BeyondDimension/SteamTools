using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class DownloadPage : UserControl
{
    public DownloadPage()
    {
        InitializeComponent();
        DataContext ??= new DownloadPageViewModel();
    }
}
