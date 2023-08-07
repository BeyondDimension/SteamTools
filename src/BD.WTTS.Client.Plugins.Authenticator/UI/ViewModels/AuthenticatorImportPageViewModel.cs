using Avalonia.Controls;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public record AuthenticatorImportMethod(string Name, string Description, string? Image, Type PageType);

public class AuthenticatorImportPageViewModel : ViewModelBase
{
    public IReadOnlyCollection<IAuthenticatorImport> AuthenticatorImports { get; }

    public IReadOnlyCollection<AuthenticatorImportMethod> AuthenticatorImportMethods { get; }

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

        AuthenticatorImportMethods = new List<AuthenticatorImportMethod>
        {
            new AuthenticatorImportMethod(Strings.Auth_SteamLoginImport, Strings.Steam_UserLoginTip, null, typeof(SteamLoginImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.SteamGuard), Strings.LocalAuth_SteamGuardImport, null, typeof(SteamGuardImportPage)),
        };
    }
}