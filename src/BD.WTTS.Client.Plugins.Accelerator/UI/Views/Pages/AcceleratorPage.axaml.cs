using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorPage : ReactiveUserControl<AcceleratorPageViewModel>
{
    public AcceleratorPage()
    {
        InitializeComponent();
        DataContext = new AcceleratorPageViewModel();
    }
}
