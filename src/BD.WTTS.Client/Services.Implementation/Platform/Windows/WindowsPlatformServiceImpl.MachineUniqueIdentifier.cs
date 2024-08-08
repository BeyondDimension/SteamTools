#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static string GetMachineSecretKey()
        => Registry.LocalMachine.Read(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid");

    [Mobius(
"""
BD.Common8.Security.Helpers.MachineUniqueIdentifier
""")]
    static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

    [Mobius(
"""
BD.Common8.Security.Helpers.MachineUniqueIdentifier
""")]
    public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
}
#endif