using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public class SteamDetailPageViewModel : ViewModelBase
{
    public string? DeviceId { get; }

    public string? RecoverCode { get; }

    public string? SteamData { get; }

    public SteamDetailPageViewModel(IAuthenticatorDTO authenticatorDto)
    {
        try
        {
            if (authenticatorDto.Value is SteamAuthenticator steamAuthenticator)
            {
                SteamData =
                    Serializable.GetIndented(steamAuthenticator.SteamData, Serializable.JsonImplType.SystemTextJson);
                RecoverCode = steamAuthenticator.RecoveryCode;
                DeviceId = steamAuthenticator.DeviceId;

                // var viewModel = new TextBoxWindowViewModel()
                // {
                //     InputType = TextBoxWindowViewModel.TextBoxInputType.ReadOnlyText,
                //     Value = steamData, 
                //         
                // };
                //await IWindowManager.Instance.ShowTaskDialogAsync(viewModel, Strings.LocalAuth_ShowAuthInfo);
            }
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
        }
    }
}