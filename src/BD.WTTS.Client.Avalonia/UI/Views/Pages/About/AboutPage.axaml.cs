using Avalonia.Controls;
using BD.WTTS.Services;

namespace BD.WTTS.UI.Views.Pages;

public partial class AboutPage : ReactiveUserControl<AboutPageViewModel>
{
    public AboutPage()
    {
        InitializeComponent();
        DataContext = IViewModelManager.Instance.Get<AboutPageViewModel>();
    }
}
