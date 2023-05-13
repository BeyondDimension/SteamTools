namespace BD.WTTS.UI.ViewModels;

public sealed class HomePageViewModel : TabItemViewModel
{
    public override string? Name => resourceManager.GetString("Welcome");

    public HomePageViewModel()
    {

    }
}
