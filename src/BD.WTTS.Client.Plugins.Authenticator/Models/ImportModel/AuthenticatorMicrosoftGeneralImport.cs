using BD.WTTS.UI.Views.Pages;
using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorMicrosoftGeneralImport : IAuthenticatorImport
{
    public string Name => Strings.LocalAuth_2FAImport.Format(Strings.Microsoft);

    public string Description => Strings.LocalAuth_MicrosoftImport;

    public ResIcon IconName => ResIcon.Attach;

    public ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorMicrosoftGeneralImport()
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
                Toast.Show(ToastIcon.Error, Strings.LocalAuth_Import_DecodePrivateKeyError.Format("Microsoft"));
                return null;
            }

            var auth = new MicrosoftAuthenticator();
            auth.Enroll(privateKey);

            if (auth.ServerTimeDiff == 0L)
            {
                Toast.Show(ToastIcon.Error, Strings.Error_CannotConnectTokenVerificationServer);
                return null;
            }

            return auth;
        }
        catch (Exception ex)
        {
            ex.LogAndShowT();
            return null;
        }
    }
}