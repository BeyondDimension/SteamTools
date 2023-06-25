using Avalonia.Controls;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views;

public partial class SteamLoginImportPage : UserControl
{
    public SteamLoginImportPage()
    {
        InitializeComponent();
        DataContext = new SteamLoginImportViewModel();
    }
    
    public SteamLoginImportPage(string? password)
    {
        InitializeComponent();
        DataContext = new SteamLoginImportViewModel(password);
    }

    // void Stepper_OnBacking(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnBacking(sender, args);
    // }
    //
    // void Stepper_OnNexting(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnNexting(sender, args);
    // }
    //
    // void Stepper_OnSkiping(Stepper sender, CancelEventArgs args)
    // {
    //     (DataContext as SteamImportAuthenticator)?.StepperOnSkiping(sender, args);
    // }
}
