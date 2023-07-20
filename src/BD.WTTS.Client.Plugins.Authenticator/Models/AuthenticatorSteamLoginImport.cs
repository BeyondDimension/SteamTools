using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Models;

public class AuthenticatorSteamLoginImport : AuthenticatorImportBase
{
    public override string Name => "Steam 登陆导入";

    public override string Description => Strings.Steam_UserLoginTip;
    
    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSteamLoginImport()
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(async () =>
        {
            if (await VerifyMaxValue())
                await IWindowManager.Instance.ShowTaskDialogAsync(new SteamLoginImportViewModel(SaveAuthenticator),
                    Name,
                    pageContent: new SteamLoginImportPage(), isOkButton: false);
        });
    }
}