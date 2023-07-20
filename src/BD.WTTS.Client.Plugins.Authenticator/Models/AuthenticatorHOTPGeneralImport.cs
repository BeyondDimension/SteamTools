using BD.WTTS.UI.Views.Pages;
using WinAuth;

namespace BD.WTTS.Models;

public class AuthenticatorHOTPGeneralImport : AuthenticatorGeneralImportBase
{
    public override string Name => "HOTP 通用密钥导入";
    
    public override string Description => "通过使用添加 HOTP 移动验证器时生成的通用密钥导入令牌";

    public override string IconText => "&#xE723;";
    
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