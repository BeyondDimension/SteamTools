using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Models;

public class AuthenticatorSteamGuardImport : AuthenticatorImportBase
{
    public override string Name => Strings.LocalAuth_Import.Format(Strings.SteamGuard);

    public override string Description => Strings.LocalAuth_SteamGuardImport;

    public override ResIcon IconName => ResIcon.MobileLocked;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSteamGuardImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new AuthenticatorSteamGuardViewModel(SaveAuthenticator),
                    Name,
                    pageContent: new AuthenticatorSteamGuardImportPage(), isOkButton: false);
        });
    }
}