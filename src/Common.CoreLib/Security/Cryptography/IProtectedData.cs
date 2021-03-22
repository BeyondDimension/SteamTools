namespace System.Security.Cryptography
{
    public interface IProtectedData
    {
        public static IProtectedData Instance => DI.Get<IProtectedData>();

        public enum DataProtectionScope
        {
            CurrentUser = 0,

            LocalMachine = 1
        }

        byte[] Protect(byte[] userData, byte[]? optionalEntropy, DataProtectionScope scope);

        byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy, DataProtectionScope scope);
    }
}