namespace BD.WTTS.Models;

public interface IAuthenticatorImport
{
    public string Name { get; }

    public string Description { get; }

    public ResIcon IconName { get; }

    public ICommand AuthenticatorImportCommand { get; set; }

    Task<bool> VerifyMaxValue();

    Task SaveAuthenticator(IAuthenticatorDTO authenticatorDto);
}