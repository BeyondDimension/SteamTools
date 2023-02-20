#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static string GetMachineSecretKey()
        => Registry.LocalMachine.Read(@"SOFTWARE\Microsoft\Cryptography", "MachineGuid");

    static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

    public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
}
#endif