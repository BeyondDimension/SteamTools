using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleAppsPage : PageBase<IdleAppsPageViewModel>
{
    public IdleAppsPage()
    {
        InitializeComponent();
        DataContext ??= new IdleAppsPageViewModel();
    }
}
