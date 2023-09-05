using BD.SteamClient.Models;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleSteamLoginViewModel : ViewModelBase
{
    readonly ISteamAccountService Account = Ioc.Get<ISteamAccountService>();

    public SteamLoginState SteamLoginState { get; set; }

    public IdleSteamLoginViewModel(ref SteamLoginState steamLoginState)
    {
        SteamLoginState = steamLoginState;
        this.Login = ReactiveCommand.Create(SteamLogin);
    }

    private async void SteamLogin()
    {
        SteamLoginState.Username = UserNameText;
        SteamLoginState.Password = PasswordText;
        await Account.DoLoginV2Async(SteamLoginState);

        if (SteamLoginState.Success)
            return;

        if (!SteamLoginState.Success && SteamLoginState.Requires2FA)
        {
            Requires2FA = SteamLoginState.Requires2FA;
            if (string.IsNullOrEmpty(this.TwofactorCode))
                return;
            SteamLoginState.TwofactorCode = this.TwofactorCode;
        }
        else if (!SteamLoginState.Success && SteamLoginState.RequiresEmailAuth)
        {
            RequiresEmailAuth = SteamLoginState.RequiresEmailAuth;
            if (string.IsNullOrEmpty(this.EmailAuthText))
                return;
            SteamLoginState.EmailCode = this.EmailAuthText;
        }
        await Account.DoLoginV2Async(SteamLoginState);
    }
}
