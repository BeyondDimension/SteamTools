using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorPage : PageBase<AcceleratorPageViewModel>
{
    public AcceleratorPage()
    {
        InitializeComponent();
        this.SetViewModel<AcceleratorPageViewModel>(true);
    }
}
