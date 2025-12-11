// ReSharper disable once CheckNamespace

namespace ArchiSteamFarm;

static class SharedInfo
{
    internal static string DirCreateByNotExists(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            _ = Directory.CreateDirectory(dirPath);
        }
        return dirPath;
    }

    private static string ASFSettingsExecuteFilePath { get { return ASFSettings.ArchiSteamFarmExePath.Value ?? string.Empty; } }

    internal static string ASFExecuteFilePath
    {
        get
        {
            if (!string.IsNullOrEmpty(ASFSettingsExecuteFilePath) && File.Exists(ASFSettingsExecuteFilePath))
            {
                Version = new Version(FileVersionInfo.GetVersionInfo(ASFSettingsExecuteFilePath)?.FileVersion ?? string.Empty);
                return ASFSettingsExecuteFilePath;
            }
            return string.Empty;
        }
    }

    internal static string HomeDirectory
    {
        get
        {
            return !string.IsNullOrWhiteSpace(ASFSettingsExecuteFilePath)
                ? Path.GetDirectoryName(ASFSettingsExecuteFilePath) ?? string.Empty
                : string.Empty;
        }
    }

    internal const string ASF = nameof(ASF);
    internal const string UlongCompatibilityStringPrefix = "s_";

    internal const string GlobalConfigFileName = $"{ASF}{JsonConfigExtension}";
    internal const string JsonConfigExtension = ".json";

    public static string ConfigDirectory = "config";

    internal const string IPCConfigExtension = ".config";
    internal const string IPCConfigFile = $"{nameof(IPC)}{IPCConfigExtension}";

    internal static string WebsiteDirectory = "www";

    internal static string PluginsDirectory = "plugins";

    internal const string ArchivalLogFile = "log.{#}.txt";
    internal static string ArchivalLogsDirectory = "logs";

    internal static Version Version = null;
}