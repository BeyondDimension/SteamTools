using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class EditAppsPage : UserControl
{
    public EditAppsPage()
    {
        InitializeComponent();
        DataContext ??= new EditAppsPageViewModel();
    }
}
