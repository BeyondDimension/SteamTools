using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Models;

public class AuthenticatorSteamGuardImport : IAuthenticatorImport
{
    public string Name => Strings.LocalAuth_Import.Format(Strings.SteamGuard);

    public string Description => Strings.LocalAuth_SteamGuardImport;

    public ResIcon IconName => ResIcon.Contact;

    public ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSteamGuardImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await IAuthenticatorImport.VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(

                    new SteamGuardImportPageViewModel(IAuthenticatorImport.SaveAuthenticator),
                    Name,
                    pageContent: new SteamGuardImportPage(), isOkButton: false);
        });
    }
}