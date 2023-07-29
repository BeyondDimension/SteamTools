#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    static string GetMachineSecretKey()
    {
        const string filePath = $"/etc/machine-id";
        return File.ReadAllText(filePath);
    }

    static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey =
        IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

    public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
}
#endif