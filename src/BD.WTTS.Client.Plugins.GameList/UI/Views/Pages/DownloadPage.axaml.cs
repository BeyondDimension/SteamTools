using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class DownloadPage : PageBase<DownloadPageViewModel>
{
    public DownloadPage()
    {
        InitializeComponent();
        if (!Design.IsDesignMode)
            DataContext ??= new DownloadPageViewModel();
    }
}
