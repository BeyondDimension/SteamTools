using SysIOPath = System.IO.Path;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public class SteamAppSaveFile : ReactiveObject
{
    public SteamAppSaveFile(uint appid, string? root, string? path, string? pattern)
    {
        ParentAppId = appid;
        Root = root;
        Path = path;
        Pattern = pattern;
    }

    public uint ParentAppId { get; set; }

    public string? Root { get; set; }

    public string? Path { get; set; }

    public string? Pattern { get; set; }

    string? _FormatDirPath;

    public string? FormatDirPath
    {
        get => _FormatDirPath;
        set => this.RaiseAndSetIfChanged(ref _FormatDirPath, value);
    }

    string? _FormatFilePath;

    public string? FormatFilePath
    {
        get => _FormatFilePath;
        set => this.RaiseAndSetIfChanged(ref _FormatFilePath, value);
    }

    public bool Recursive { get; set; }

    public string? Platforms { get; set; }

    public void FormatPathGenerate()
    {
        if (string.IsNullOrEmpty(Root) || string.IsNullOrEmpty(Path) || string.IsNullOrEmpty(Pattern))
        {
            return;
        }

        var path = "";

        if (string.Equals(Root, "WinAppDataRoaming", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else if (string.Equals(Root, "WinAppDataLocal", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
        else if (string.Equals(Root, "WinAppDataLocalLow", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "Low";
        }
        else if (string.Equals(Root, "WinMyDocuments", StringComparison.OrdinalIgnoreCase))
        {
            path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
        else if (string.Equals(Root, "gameinstall", StringComparison.OrdinalIgnoreCase))
        {
            path = SteamConnectService.Current.SteamApps.Lookup(ParentAppId).Value?.InstalledDir;
        }
        else
        {
            path = "unknown";
        }

        if (string.IsNullOrEmpty(path))
            return;

        var p = Path.Replace("{64BitSteamID}", SteamConnectService.Current.CurrentSteamUser?.SteamId64.ToString())
                .Replace("{Steam3AccountID}", SteamConnectService.Current.CurrentSteamUser?.SteamId32.ToString());

        if (SysIOPath.IsPathRooted(p))
        {
            p = p.Remove(0, SysIOPath.GetPathRoot(p).Length);
        }

        path = SysIOPath.Combine(path, p);

        FormatDirPath = SysIOPath.GetFullPath(path);

        FormatFilePath = SysIOPath.GetFullPath(SysIOPath.Combine(path, Pattern));
    }
}
