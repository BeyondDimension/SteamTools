namespace BD.WTTS.UI.ViewModels;

public partial class ArchiSteamFarmExePathSettingsPageViewModel : ViewModelBase
{
    public ICommand SelectProgramPath { get; }

    public ArchiSteamFarmExePathSettingsPageViewModel()
    {
        SelectProgramPath = ReactiveCommand.Create(ASFService.Current.SelectASFProgramLocation);
    }
}
