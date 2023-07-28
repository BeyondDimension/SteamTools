using BD.WTTS.UI.Views.Pages;
using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorHOTPGeneralImport : AuthenticatorGeneralImportBase
{
    public override string Name => Strings.LocalAuth_2FAImport.Format(Strings.HOTP);

    public override string Description => Strings.LocalAuth_HOTPImport;

    public override ResIcon IconName => ResIcon.Attach;

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorHOTPGeneralImport()
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new AuthenticatorGeneralImportPageViewModel(SaveAuthenticator, CreateAuthenticatorValueDto),
                    Name,
                    pageContent: new AuthenticatorGeneralImportPage(), isOkButton: false);
        });
    }

    protected override async Task<IAuthenticatorValueDTO?> CreateAuthenticatorValueDto(string secretCode)
    {
        try
        {
            var privateKey = await AuthenticatorService.DecodePrivateKey(secretCode);

            if (string.IsNullOrEmpty(privateKey))
            {
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_Import_DecodePrivateKeyError.Format("HOTP"));
                return null;
            }

            var auth = new HOTPAuthenticator();
            auth.Enroll(privateKey);
            return auth;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return null;
        }
    }
}