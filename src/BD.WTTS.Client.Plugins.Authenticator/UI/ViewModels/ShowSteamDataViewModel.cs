using BD.WTTS.Client.Resources;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public class ShowSteamDataViewModel : ViewModelBase
{
    string? _deviceId;
    string? _revocerCode;
    string? _steamData;

    public string? DeviceId
    {
        get => _deviceId;
        set
        {
            if (value == _deviceId) return;
            _deviceId = value;
            this.RaisePropertyChanged();
        }
    }

    public string? RecoverCode
    {
        get => _revocerCode;
        set
        {
            if (value == _revocerCode) return;
            _revocerCode = value;
            this.RaisePropertyChanged();
        }
    }

    public string? SteamData
    {
        get => _steamData;
        set
        {
            if (value == _steamData) return;
            _steamData = value;
            this.RaisePropertyChanged();
        }
    }

    public ShowSteamDataViewModel()
    {
        
    }

    public ShowSteamDataViewModel(IAuthenticatorDTO authenticatorDto)
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
   
}