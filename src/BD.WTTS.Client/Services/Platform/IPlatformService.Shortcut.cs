// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    void CreateShortcut(string pathLink,
        string targetPath,
        string iconSavePath,
        byte[] accountImage,
        string? arguments = null,
        string? description = null,
        string? hotkey = null,
        string? workingDirectory = null)
    { }
}
