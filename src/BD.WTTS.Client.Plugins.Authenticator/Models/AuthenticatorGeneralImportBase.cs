namespace BD.WTTS.Models;

public abstract class AuthenticatorGeneralImportBase : AuthenticatorImportBase
{
    public abstract override string Name { get; }
    
    public abstract override string Description { get; }
    
    public abstract override string IconText { get; }
    
    public abstract override ICommand AuthenticatorImportCommand { get; set; }

    protected abstract Task<IAuthenticatorValueDTO?> CreateAuthenticatorValueDto(string secretCode);
}