namespace BD.WTTS.UI.ViewModels;

public class AuthenticatorImportPageViewModel : ViewModelBase
{
    [Reactive]
    public ObservableCollection<AuthenticatorImportBase> AuthenticatorImportBases { get; set; }

    public AuthenticatorImportPageViewModel()
    {
        AuthenticatorImportBases = new ObservableCollection<AuthenticatorImportBase>();

        Initialize();
    }

    public void Initialize()
    {
        AuthenticatorImportBases.Clear();
        AuthenticatorImportBases.Add(new AuthenticatorSteamLoginImport());
        AuthenticatorImportBases.Add(new AuthenticatorSteamGuardImport());
        AuthenticatorImportBases.Add(new AuthenticatorSdaFileImport());
        AuthenticatorImportBases.Add(new AuthenticatorWattToolKitV2Import());
        AuthenticatorImportBases.Add(new AuthenticatorWattToolKitV1Import());
        AuthenticatorImportBases.Add(new AuthenticatorWinAuthFileImport());
        AuthenticatorImportBases.Add(new AuthenticatorGoogleGeneralImport());
        AuthenticatorImportBases.Add(new AuthenticatorMicrosoftGeneralImport());
        AuthenticatorImportBases.Add(new AuthenticatorHOTPGeneralImport());
    }
}