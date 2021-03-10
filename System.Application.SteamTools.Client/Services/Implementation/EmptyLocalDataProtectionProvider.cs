using System.Security.Cryptography;

namespace System.Application.Services.Implementation
{
    public sealed class EmptyLocalDataProtectionProvider : LocalDataProtectionProviderBase
    {
        public EmptyLocalDataProtectionProvider(ILocalDataProtectionProvider.IProtectedData protectedData, ILocalDataProtectionProvider.IDataProtectionProvider dataProtectionProvider) : base(protectedData, dataProtectionProvider)
        {
            MachineSecretKey = AESUtils.GetParameters(Environment.MachineName);
        }

        protected override (byte[] key, byte[] iv) MachineSecretKey { get; }
    }
}