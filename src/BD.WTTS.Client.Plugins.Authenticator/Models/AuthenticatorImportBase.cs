namespace BD.WTTS.Models;

public abstract class AuthenticatorImportBase : IAuthenticatorImport
{
    public abstract string Name { get; }
    
    public abstract string Description { get; }
    
    public abstract ICommand AuthenticatorImportCommand { get; set; }
    
    public async Task<bool> VerifyMaxValue()
    {
        var auths = await AuthenticatorService.GetAllSourceAuthenticatorAsync();
        if (auths.Length < IAccountPlatformAuthenticatorRepository.MaxValue) return true;
        Toast.Show(ToastIcon.Info, "已达到本地令牌数量上限");
        return false;
    }
}