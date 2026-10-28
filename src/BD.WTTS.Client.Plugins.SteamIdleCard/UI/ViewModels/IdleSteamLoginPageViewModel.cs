using BD.SteamClient.Models;

namespace BD.WTTS.UI.ViewModels;

/// <summary>The idle-card plugin only handles the authenticated account; sign-in is shared.</summary>
public sealed class IdleSteamLoginPageViewModel : WindowViewModel
{
    public SteamLoginViewModel Login { get; }

    public IdleSteamLoginPageViewModel(ref SteamLoginState steamLoginState)
    {
        Login = new SteamLoginViewModel(steamLoginState, allowCookieLogin: true, rememberLogin: true);
        Login.AddTo(this);
        Login.LoginSucceededAsync = _ =>
        {
            Toast.Show(ToastIcon.Success, Strings.Success_.Format(Strings.User_Login));
            Close?.Invoke(false);
            return Task.CompletedTask;
        };
    }
}
