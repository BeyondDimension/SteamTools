using static System.Application.Services.ILocalDataProtectionProvider;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ILocalDataProtectionProvider"/>
    public class LocalDataProtectionProvider : LocalDataProtectionProviderBase
    {
        readonly IPlatformService platformService;

        public LocalDataProtectionProvider(
            IProtectedData protectedData,
            IDataProtectionProvider dataProtectionProvider,
            IPlatformService platformService) : base(protectedData, dataProtectionProvider)
        {
            this.platformService = platformService;
        }

        protected override (byte[] key, byte[] iv) MachineSecretKey => platformService.MachineSecretKey;
    }
}