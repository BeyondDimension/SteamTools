using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Models;

public class AuthenticatorSteamLoginImport : IAuthenticatorImport
{
    public string Name => Strings.Auth_SteamLoginImport;

    public string Description => Strings.Steam_UserLoginTip;

    public ResIcon IconName => ResIcon.Contact;

    public ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSteamLoginImport()
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await IAuthenticatorImport.VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(new SteamLoginImportViewModel(IAuthenticatorImport.SaveAuthenticator),
                    Name,
                    pageContent: new SteamLoginImportPage(), isOkButton: false);
        });
    }
}