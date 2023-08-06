using BD.WTTS.UI.Views.Pages;
using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorHOTPGeneralImport : IAuthenticatorImport
{
    public string Name => Strings.LocalAuth_2FAImport.Format(Strings.HOTP);

    public string Description => Strings.LocalAuth_HOTPImport;

    public ResIcon IconName => ResIcon.Attach;

    public ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorHOTPGeneralImport()
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await IAuthenticatorImport.VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(
                    new AuthenticatorGeneralImportPageViewModel(IAuthenticatorImport.SaveAuthenticator, CreateAuthenticatorValueDto),
                    Name,
                    pageContent: new AuthenticatorGeneralImportPage(), isOkButton: false);
        });
    }

    protected async Task<IAuthenticatorValueDTO?> CreateAuthenticatorValueDto(string secretCode)
    {
        try
        {
            var privateKey = await AuthenticatorHelper.DecodePrivateKey(secretCode);

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