using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows")]
    internal sealed class WindowsProtectedData : ILocalDataProtectionProvider.IProtectedData, IProtectedData
    {
        static DataProtectionScope GetScope(IProtectedData.DataProtectionScope dataProtectionScope) => dataProtectionScope switch
        {
            IProtectedData.DataProtectionScope.CurrentUser => DataProtectionScope.CurrentUser,
            IProtectedData.DataProtectionScope.LocalMachine => DataProtectionScope.LocalMachine,
            _ => throw new ArgumentOutOfRangeException(nameof(dataProtectionScope), dataProtectionScope, null),
        };

        public byte[] Protect(byte[] userData)
        {
            return ProtectedData.Protect(userData, null, DataProtectionScope.LocalMachine);
        }

        public byte[] Protect(byte[] userData, byte[]? optionalEntropy, IProtectedData.DataProtectionScope scope)
        {
            return ProtectedData.Protect(userData, optionalEntropy, GetScope(scope));
        }

        public byte[] Unprotect(byte[] encryptedData)
        {
            return ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.LocalMachine);
        }

        public byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy, IProtectedData.DataProtectionScope scope)
        {
            return ProtectedData.Unprotect(encryptedData, optionalEntropy, GetScope(scope));
        }
    }
}