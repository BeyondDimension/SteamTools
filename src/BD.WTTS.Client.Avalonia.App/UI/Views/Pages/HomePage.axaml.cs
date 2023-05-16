using Avalonia.Controls;
using BD.WTTS.Services;

namespace BD.WTTS.UI.Views.Pages;

public partial class HomePage : UserControl
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = IViewModelManager.Instance.Get<HomePageViewModel>();
    }
}
