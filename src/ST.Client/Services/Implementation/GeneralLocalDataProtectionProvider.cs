using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    [Obsolete]
    public sealed class GeneralLocalDataProtectionProvider : LocalDataProtectionProviderBase
    {
        public GeneralLocalDataProtectionProvider(ILocalDataProtectionProvider.IProtectedData protectedData, ILocalDataProtectionProvider.IDataProtectionProvider dataProtectionProvider) : base(protectedData, dataProtectionProvider)
        {
            MachineSecretKey = AESUtils.GetParameters(Environment.MachineName);
        }

        protected override (byte[] key, byte[] iv) MachineSecretKey { get; }
    }
}
