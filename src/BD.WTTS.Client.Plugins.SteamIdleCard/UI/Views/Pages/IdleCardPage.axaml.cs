using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class IdleCardPage : PageBase<IdleCardPageViewModel>
{
    public IdleCardPage()
    {
        InitializeComponent();
        DataContext ??= new IdleCardPageViewModel();
    }
}
