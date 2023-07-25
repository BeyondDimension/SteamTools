using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class EditAppsPage : PageBase<EditAppsPageViewModel>
{
    public EditAppsPage()
    {
        InitializeComponent();
        DataContext ??= new EditAppsPageViewModel();
    }
}
