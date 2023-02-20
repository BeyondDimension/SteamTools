#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    public string? GetSteamDirPath()
    {
        var value = $"/Users/{Environment.UserName}/Library/Application Support/Steam";
        return value;
    }

    string? IPlatformService.GetSteamDynamicLinkLibraryPath()
    {
        var value = $"/Users/{Environment.UserName}/Library/Application Support/Steam/Steam.AppBundle/Steam/Contents/MacOS";
        return value;
    }

    const string OSXSteamProgramPath = $"/Applications/Steam.app/Contents/MacOS/steam_osx";

    public string? GetSteamProgramPath() => OSXSteamProgramPath;

    public string? GetRegistryVdfPath()
    {
        var value = $"/Users/{Environment.UserName}/Library/Application Support/Steam/registry.vdf";
        return value;
    }
}
#endif