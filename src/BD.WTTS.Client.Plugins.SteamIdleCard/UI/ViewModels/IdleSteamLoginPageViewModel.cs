using BD.SteamClient.Models;
using BD.SteamClient.Services;
using SteamKit2;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleSteamLoginPageViewModel : WindowViewModel
{
    readonly ISteamAccountService Account = Ioc.Get<ISteamAccountService>();
    readonly ISteamSessionService SteamSession = Ioc.Get<ISteamSessionService>();

    public SteamLoginState SteamLoginState { get; set; }

    public IdleSteamLoginPageViewModel(ref SteamLoginState steamLoginState)
    {
        SteamLoginState = steamLoginState;
        this.Login = ReactiveCommand.Create(SteamLoginAsync);
        this.CookieLogin = ReactiveCommand.Create(CookieLoginAsync);
    }

    private async void SteamLoginAsync()
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
                    var session = SteamSession.RentSession(SteamLoginState.SteamId.ToString());

                    if (session != null)
                        await SteamSession.SaveSession(session);
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

    private async void CookieLoginAsync()
    {
        if (!IsLoading)
        {
            IsLoading = true;
            if (!string.IsNullOrEmpty(SeesionId) && !string.IsNullOrEmpty(SteamLoginSecure))
            {
                var temps = SteamLoginSecure.Split("%7C%7C");
                if (temps.Length == 2)
                {
                    var steamid = temps[0];
                    var accecstoken = temps[1];
                    var cookieContainer = new CookieContainer();
                    cookieContainer.Add(new Cookie("steamLoginSecure", SteamLoginSecure, "/", "steamcommunity.com"));
                    cookieContainer.Add(new Cookie("sessionid", SeesionId, "/", "steamcommunity.com"));
                    cookieContainer.Add(new Cookie("steamLoginSecure", SteamLoginSecure, "/", "steampowered.com"));
                    cookieContainer.Add(new Cookie("sessionid", SeesionId, "/", "steampowered.com"));
                    SteamSession session = new SteamSession()
                    {
                        SteamId = steamid,
                        AccessToken = accecstoken,
                        RefreshToken = string.Empty,
                        CookieContainer = cookieContainer,
                    };

                    SteamSession.AddOrSetSeesion(session);

                    if (RemenberLogin)
                    {
                        await SteamSession.SaveSession(session);
                    }

                    SteamLoginState.Success = true;
                    Toast.Show(ToastIcon.Success, Strings.Success_.Format(Strings.User_Login));
                    IsLoading = false;
                    Close?.Invoke();
                }
            }
            IsLoading = false;
        }
    }
}
