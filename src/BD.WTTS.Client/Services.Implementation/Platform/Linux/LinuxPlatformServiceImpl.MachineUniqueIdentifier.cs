#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    [Mobius(
"""
BD.Common8.Security.Helpers.MachineUniqueIdentifier
""")]
    static string GetMachineSecretKey()
    {
        const string filePath = $"/etc/machine-id";
        return File.ReadAllText(filePath);
    }

    [Mobius(
"""
BD.Common8.Security.Helpers.MachineUniqueIdentifier
""")]
    static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey =
        IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

    [Mobius(
"""
BD.Common8.Security.Helpers.MachineUniqueIdentifier
""")]
    public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
}
#endif