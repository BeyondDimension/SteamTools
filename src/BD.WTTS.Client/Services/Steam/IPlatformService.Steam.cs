// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <inheritdoc cref="ISteamService.SteamDirPath"/>
    string? GetSteamDirPath() => null;

    /// <summary>
    /// 获取 Steam 动态链接库 (DLL) 文件夹目录
    /// </summary>
    /// <returns></returns>
    string? GetSteamDynamicLinkLibraryPath() => GetSteamDirPath();

    /// <inheritdoc cref="ISteamService.SteamProgramPath"/>
    string? GetSteamProgramPath() => null;

    string? GetRegistryVdfPath() => null;

    /// <inheritdoc cref="ISteamService.GetLastLoginUserName"/>
    string GetLastSteamLoginUserName() => "";

#if DEBUG
    /// <inheritdoc cref="ISteamService.SetCurrentUser(string)"/>
    [Obsolete("use SetSteamCurrentUser", true)]
    void SetCurrentUser(string userName) { }
#endif

    /// <inheritdoc cref="ISteamService.SetCurrentUser(string)"/>
    void SetSteamCurrentUser(string userName) { }
}