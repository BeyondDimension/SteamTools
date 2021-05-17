namespace System.Security.Cryptography
{
    public interface IProtectedData
    {
        public static IProtectedData Instance
        {
            get
            {
                var value = DI.Get_Nullable<IProtectedData>();
                return value ?? throw new PlatformNotSupportedException();
            }
        }

        public enum DataProtectionScope
        {
            CurrentUser = 0,

            LocalMachine = 1
        }

        byte[] Protect(byte[] userData, byte[]? optionalEntropy, DataProtectionScope scope);

        byte[] Unprotect(byte[] encryptedData, byte[]? optionalEntropy, DataProtectionScope scope);
    }
}