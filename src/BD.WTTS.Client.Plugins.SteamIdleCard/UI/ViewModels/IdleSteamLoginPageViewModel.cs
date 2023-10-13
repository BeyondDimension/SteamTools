using BD.SteamClient.Models;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleSteamLoginPageViewModel : WindowViewModel
{
    readonly ISteamAccountService Account = Ioc.Get<ISteamAccountService>();

    public SteamLoginState SteamLoginState { get; set; }

    public IdleSteamLoginPageViewModel(ref SteamLoginState steamLoginState)
    {
        SteamLoginState = steamLoginState;
        this.Login = ReactiveCommand.Create(SteamLogin);
    }

    private async void SteamLogin()
    {
        if (!IsLoading)
        {
            IsLoading = true;

            if (SteamLoginState.SteamId == 0)
            {
                SteamLoginState.Username = UserNameText;
                SteamLoginState.Password = PasswordText;
            }
            else
            {
                this.TwofactorCode ??= string.Empty;

                if (SteamLoginState.Requires2FA)
                {
                    SteamLoginState.TwofactorCode = this.TwofactorCode;
                }
                else if (SteamLoginState.RequiresEmailAuth)
                {
                    SteamLoginState.EmailCode = this.TwofactorCode;
                }
            }

            await Account.DoLoginV2Async(SteamLoginState);

            Requires2FA = SteamLoginState.Requires2FA;
            RequiresEmailAuth = SteamLoginState.RequiresEmailAuth;

            if (SteamLoginState.Success)
            {
                if (RemenberLogin)
                {

                }
                Toast.Show(ToastIcon.Success, Strings.Success_.Format(Strings.User_Login));
                IsLoading = false;
                Close?.Invoke();
            }
            else if (SteamLoginState.Message != null)
            {
                IsLoading = false;
                Toast.Show(ToastIcon.Warning, SteamLoginState.Message);
            }
        }
    }
}
