namespace BD.WTTS.Models;

public class AuthenticatorRecoverModel
{
    public UserAuthenticatorDeleteBackupResponse AuthenticatorDeleteBackup { get; set; }

    public bool IsSelected { get; set; }

    public AuthenticatorRecoverModel(UserAuthenticatorDeleteBackupResponse authenticatorDeleteBackup)
    {
        AuthenticatorDeleteBackup = authenticatorDeleteBackup;
    }
}
