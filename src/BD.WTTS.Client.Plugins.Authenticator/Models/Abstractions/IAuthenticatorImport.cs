namespace BD.WTTS.Models;

public interface IAuthenticatorImport
{
    public string Name { get; }

    public string Description { get; }

    public ICommand AuthenticatorImportCommand { get; set; }
}