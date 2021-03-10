using System.Runtime.Versioning;
using System.Security.Cryptography;
using static System.Application.Services.ILocalDataProtectionProvider;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed class WindowsProtectedData : IProtectedData
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
}