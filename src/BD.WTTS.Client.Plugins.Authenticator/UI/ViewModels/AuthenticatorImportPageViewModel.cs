using Avalonia.Controls;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.ViewModels;

public record AuthenticatorImportMethod(string Name, string Description, string? Image, Type PageType);

public class AuthenticatorImportPageViewModel : ViewModelBase
{
    public static string Name => Strings.PleaseSelect + Strings.AuthImport;

    public IReadOnlyCollection<IAuthenticatorImport> AuthenticatorImports { get; }

    public IReadOnlyCollection<AuthenticatorImportMethod> AuthenticatorImportMethods { get; }

    public AuthenticatorImportPageViewModel()
    {
        AuthenticatorImports = new List<IAuthenticatorImport>
        {
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
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Mafile), Strings.LocalAuth_SDAImport, null, typeof(SteamGuardImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV2), Strings.LocalAuth_WattToolKitV2Import, null, typeof(SteamGuardImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WattToolKitV1), Strings.LocalAuth_WattToolKitV1Import, null, typeof(SteamGuardImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.WinAuth), Strings.LocalAuth_WinAuthImport, null, typeof(SteamGuardImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Google), Strings.LocalAuth_GoogleImport, null, typeof(AuthenticatorGeneralImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.Microsoft), Strings.LocalAuth_MicrosoftImport, null, typeof(AuthenticatorGeneralImportPage)),
            new AuthenticatorImportMethod(Strings.LocalAuth_Import.Format(Strings.HOTP), Strings.LocalAuth_HOTPImport, null, typeof(AuthenticatorGeneralImportPage)),
        };
    }
}