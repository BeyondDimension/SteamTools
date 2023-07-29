#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class WindowsProtectedData : ILocalDataProtectionProvider.IProtectedData
{
    public byte[] Protect(byte[] userData)
    {
        return ProtectedData.Protect(userData, null, DataProtectionScope.LocalMachine);
    }

    public byte[] Unprotect(byte[] encryptedData)
    {
        return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
    }
}
#endif