using System.Globalization;

namespace BD.WTTS.Services;

enum ShaderCacheKind
{
    Driver,
    SteamGame,
    SteamTemporary,
}

sealed record ShaderCacheLocation(string Path, string Root, ShaderCacheKind Kind, string Name,
    string? SteamApps = null, uint? AppId = null);

static class ShaderCacheLocations
{
    const string Windows = "Windows";
    const string Linux = "Linux";
    const string MacOS = "macOS";
    const string Home = "%SHADER_CACHE_HOME%";

    // A wildcard denotes immediate child directories of the named parent.
    static readonly (string Platform, string Name, string Path)[] driverLocations =
    [
        (Windows, "DirectX", @"%LOCALAPPDATA%\D3DSCache"),
        (Windows, "NVIDIA DXCache", @"%LOCALAPPDATA%\NVIDIA\DXCache"),
        (Windows, "NVIDIA GLCache", @"%LOCALAPPDATA%\NVIDIA\GLCache"),
        (Windows, "NVIDIA OptiX", @"%LOCALAPPDATA%\NVIDIA\OptixCache"),
        (Windows, "NVIDIA NV_Cache", @"%PROGRAMDATA%\NVIDIA Corporation\NV_Cache"),
        (Windows, "NVIDIA NV_Cache", @"%LOCALAPPDATA%\NVIDIA Corporation\NV_Cache"),
        (Windows, "AMD DxCache", @"%LOCALAPPDATA%\AMD\DxCache"),
        (Windows, "AMD DxcCache", @"%LOCALAPPDATA%\AMD\DxcCache"),
        (Windows, "AMD GLCache", @"%LOCALAPPDATA%\AMD\GLCache"),
        (Windows, "AMD VkCache", @"%LOCALAPPDATA%\AMD\VkCache"),
        (Windows, "Intel ShaderCache", @"%LOCALAPPDATA%\Intel\ShaderCache"),
        (Linux, "Mesa", Home + "/.cache/mesa_shader_cache"),
        (Linux, "NVIDIA GLCache", Home + "/.cache/nvidia/GLCache"),
        (Linux, "NVIDIA GLCache", Home + "/.nv/GLCache"),
        (Linux, "AMD", Home + "/.cache/AMD/*"),
    ];

    static readonly (string Platform, string Path)[] steamLocations =
    [
        (Linux, Home + "/.local/share/Steam"),
        (Linux, Home + "/.steam/steam"),
        (Linux, Home + "/.var/app/com.valvesoftware.Steam/.local/share/Steam"),
        (Linux, Home + "/.var/app/com.valvesoftware.Steam/.steam/steam"),
        (MacOS, Home + "/Library/Application Support/Steam"),
    ];

    static string? Platform => OperatingSystem.IsWindows() ? Windows
        : OperatingSystem.IsLinux() ? Linux
        : OperatingSystem.IsMacOS() ? MacOS
        : null;

    public static bool IsSupported => Platform != null;

    public static IEnumerable<ShaderCacheLocation> FindDriverCaches(ShaderCacheFileStore files, CancellationToken token)
    {
        foreach (var (platform, name, template) in driverLocations)
        {
            token.ThrowIfCancellationRequested();
            if (platform != Platform || Expand(template) is not { } path)
                continue;

            if (!template.EndsWith('*'))
            {
                if (Directory.Exists(path))
                    yield return new(path, path, ShaderCacheKind.Driver, name);
                continue;
            }

            var parent = Path.GetDirectoryName(path)!;
            foreach (var child in files.FindDirectories(parent, "*", token))
                yield return new(child, parent, ShaderCacheKind.Driver, $"{name} {Path.GetFileName(child)}");
        }
    }

    public static IEnumerable<string> GetSteamInstallations(string? configuredSteam)
    {
        if (!string.IsNullOrEmpty(configuredSteam))
            yield return configuredSteam;

        foreach (var (platform, template) in steamLocations)
            if (platform == Platform && Expand(template) is { } path)
                yield return path;
    }

    public static IEnumerable<ShaderCacheLocation> FindSteamCaches(ShaderCacheFileStore files,
        IEnumerable<string> installations, IEnumerable<string> librarySteamApps, CancellationToken token)
    {
        foreach (var steamApps in librarySteamApps)
        {
            var root = Path.Combine(steamApps, "shadercache");
            foreach (var path in files.FindDirectories(root, "*", token))
            {
                var name = Path.GetFileName(path);
                if (uint.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var appId) && appId != 0)
                    yield return new(path, root, ShaderCacheKind.SteamGame, name, steamApps, appId);
            }
        }

        foreach (var installation in installations)
            foreach (var path in files.FindDirectories(installation, "shader_cache_temp_dir*", token))
                yield return new(path, installation, ShaderCacheKind.SteamTemporary, "Steam");
    }

    static string? Expand(string template)
    {
        if (template.StartsWith(Home, StringComparison.Ordinal))
        {
            var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (string.IsNullOrWhiteSpace(profile))
                return null;
            template = profile + template[Home.Length..];
        }
        return Environment.ExpandEnvironmentVariables(template);
    }
}
