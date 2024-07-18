using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Pages;

public partial class CsgoVacRepairPage : PageBase<CsgoVacRepairPageViewModel>
{
    public CsgoVacRepairPage()
    {
        InitializeComponent();
        DataContext ??= new CsgoVacRepairPageViewModel();
    }
}
