// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.Helpers;

public static class ArchiCryptoHelper
{
    public enum ECryptoMethod : byte
    {
        PlainText,
        AES,
        ProtectedDataForCurrentUser,
        EnvironmentVariable,
        File,
    }

    public enum EHashingMethod : byte
    {
        PlainText,
        SCrypt,
        Pbkdf2,
    }
}