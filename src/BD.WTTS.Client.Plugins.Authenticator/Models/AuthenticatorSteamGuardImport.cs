namespace BD.WTTS.Models;

public class AuthenticatorSteamGuardImport : AuthenticatorImportBase
{
    public override string Name => "Steam 「移动验证器」导入";

    public override string Description => "通过使用 Steam 「移动验证器」内的可移植数据导入令牌";
    
    public override string IconText => "&#xEC20;";

    public sealed override ICommand AuthenticatorImportCommand { get; set; }

    public AuthenticatorSteamGuardImport(string? password = null)
    {
        AuthenticatorImportCommand = ReactiveCommand.Create(() =>
        {
            //TODO 移动验证器导入Page
        });
    }
}