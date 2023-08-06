namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorImportPageViewModel : ViewModelBase
{
    public IEnumerable<IAuthenticatorImport> AuthenticatorImports { get; }

    public AuthenticatorImportPageViewModel()
    {
        AuthenticatorImports = new List<IAuthenticatorImport>
        {
            new AuthenticatorSteamLoginImport(),
            new AuthenticatorSteamGuardImport(),
            new AuthenticatorSdaFileImport(),
            new AuthenticatorWattToolKitV2Import(),
            new AuthenticatorWattToolKitV1Import(),
            new AuthenticatorWinAuthFileImport(),
            new AuthenticatorGoogleGeneralImport(),
            new AuthenticatorMicrosoftGeneralImport(),
            new AuthenticatorHOTPGeneralImport(),
        };
    }
}