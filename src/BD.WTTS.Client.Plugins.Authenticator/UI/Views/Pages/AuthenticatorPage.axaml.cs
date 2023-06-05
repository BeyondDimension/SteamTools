using Avalonia.Controls;
using Avalonia.ReactiveUI;
using BD.WTTS.StepControls_Test;

namespace BD.WTTS.UI.Views.Pages;
public partial class AuthenticatorPage : ReactiveUserControl<AuthenticatorPageViewModel>
{
    public AuthenticatorPage()
    {
        InitializeComponent();
        //stepControl.AddStep("Step1", new TextBlock() { Text = "Test1" });
        //stepControl.AddStep("Step2", new TextBlock() { Text = "Test2" });
        //stepControl.AddStep("Step3", new TextBlock() { Text = "Test3" });
        //stepControl.AddStep("Step4", new StepTestPage());
        DataContext = new AuthenticatorPageViewModel();
    }
}
