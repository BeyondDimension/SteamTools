// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    void CreateShortcut(string pathLink,
        string targetPath,
        string iconLocation,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? workingDirectory = null)
    { }
}
