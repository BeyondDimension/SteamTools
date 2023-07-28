namespace BD.WTTS.Models;

public class AuthenticatorRecoverModel
{
    [Reactive]
    public UserAuthenticatorDeleteBackupResponse AuthenticatorDeleteBackup { get; set; }

    [Reactive]
    public bool IsSelected { get; set; }

    public AuthenticatorRecoverModel(UserAuthenticatorDeleteBackupResponse authenticatorDeleteBackup)
    {
        AuthenticatorDeleteBackup = authenticatorDeleteBackup;
    }
}