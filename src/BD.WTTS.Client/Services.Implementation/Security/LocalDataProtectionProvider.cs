using static BD.Common.Services.ILocalDataProtectionProvider;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="ILocalDataProtectionProvider"/>
[Mobius(
"""
Mobius.Services.LocalDataProtectionProviderBase
""")]
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