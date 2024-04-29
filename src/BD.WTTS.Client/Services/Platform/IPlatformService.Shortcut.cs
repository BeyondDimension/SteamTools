// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    void CreateShortcut(string pathLink,
        string targetPath,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? iconLocation = null,
        string? workingDirectory = null)
    { }
}
