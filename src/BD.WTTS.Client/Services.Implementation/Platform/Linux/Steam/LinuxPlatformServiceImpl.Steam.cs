#if LINUX
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class LinuxPlatformServiceImpl
{
    public string? GetSteamDirPath()
        => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.steam/steam";

    public string? GetSteamProgramPath() => "/usr/bin/steam";

    public string? GetRegistryVdfPath()
        => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/.steam/registry.vdf";
}
#endif