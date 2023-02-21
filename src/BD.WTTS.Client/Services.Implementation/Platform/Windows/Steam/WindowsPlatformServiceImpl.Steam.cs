#if WINDOWS

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    static string? GetFullPath(string s)
    {
        if (!string.IsNullOrWhiteSpace(s))
        {
            return Path.GetFullPath(s);
        }
        return null;
    }

    const string SteamRegistryPath = @"SOFTWARE\Valve\Steam";

    public string? GetSteamDirPath()
        => GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamPath"));

    public string? GetSteamProgramPath()
        => GetFullPath(Registry.CurrentUser.Read(SteamRegistryPath, "SteamExe"));

    public string GetLastSteamLoginUserName()
        => Registry.CurrentUser.Read(SteamRegistryPath, "AutoLoginUser");

    public void SetSteamCurrentUser(string userName)
    {
        if (DesktopBridge.IsRunningAsUwp)
        {
            var reg = $"Windows Registry Editor Version 5.00{Environment.NewLine}[HKEY_CURRENT_USER\\Software\\Valve\\Steam]{Environment.NewLine}\"AutoLoginUser\"=\"{userName}\"";
            var path = IOPath.GetCacheFilePath(CacheTempDirName, "SwitchSteamUser", FileEx.Reg);
            File.WriteAllText(path, reg, Encoding.UTF8);
            var p = StartProcessRegedit($"/s \"{path}\"");
            IOPath.TryDeleteInDelay(p, path);
        }
        else
        {
            Registry.CurrentUser.AddOrUpdate(SteamRegistryPath, "AutoLoginUser", userName, RegistryValueKind.String);
        }
    }
}
#endif